// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Dicom.Core.Features.ChangeFeed;
using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.Functions.BlobMigration.Models;
using Microsoft.Health.Dicom.Functions.Migration;
using Microsoft.Health.Operations.Functions.DurableTask;

namespace Microsoft.Health.Dicom.Functions.BlobMigration;

public partial class CleanupDeletedDurableFunction
{
    /// <summary>
    /// Asynchronously deletes dangling DICOM deleted instances.
    /// It goes through DICOM instances in the past, deletes old format blob.
    /// </summary>
    /// <param name="context">The context for the orchestration instance.</param>
    /// <param name="logger">A diagnostic logger.</param>
    /// <returns>A task representing the <see cref="CleanupDeletedFilesAsync"/> operation.</returns>
    [FunctionName(nameof(CleanupDeletedFilesAsync))]
    public async Task CleanupDeletedFilesAsync(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger logger)
    {
        // The ID should be a GUID as generated by the trigger, but we'll assert here just to make sure!
        EnsureArg.IsNotNull(context, nameof(context)).ThrowIfInvalidOperationId();
        BlobMigrationCheckpoint input = context.GetInput<BlobMigrationCheckpoint>();
        EnsureArg.IsNotNull(input, nameof(input));
        EnsureArg.IsNotNull(input.Batching, nameof(input.Batching));

        logger = context.CreateReplaySafeLogger(logger);

        // Get the max watermark of all deleted instances for the exit scenario
        long maxWatermark = input.MaxWatermark ?? await context.CallActivityWithRetryAsync<long>(
            nameof(GetMaxDeletedChangeFeedWatermarkAsync),
            _options.RetryOptions,
            new CleanupDeletedBatchArguments
            {
                FilterTimeStamp = input.FilterTimeStamp
            });

        IReadOnlyList<ChangeFeedEntry> changeFeedEntries = await context.CallActivityWithRetryAsync<IReadOnlyList<ChangeFeedEntry>>(
            nameof(GetDeletedChangeFeedInstanceBatchesAsync),
            _options.RetryOptions,
            new CleanupDeletedBatchArguments
            {
                StartWatermark = input.Completed.HasValue ? input.Completed.Value.Start : default,
                EndWatermark = input.Completed.HasValue ? input.Completed.Value.End : default,
                BatchSize = input.Batching.Size,
                FilterTimeStamp = input.FilterTimeStamp,
            });

        bool hasReachedToEnd = input.Completed.HasValue && input.Completed.Value.End >= maxWatermark;

        if (changeFeedEntries.Count > 0)
        {
            var batchRange = new WatermarkRange(changeFeedEntries[0].Sequence, changeFeedEntries[^1].Sequence);

            logger.LogInformation("Beginning to delete the range {Range}.", batchRange);

            var instanceIdentifiers = changeFeedEntries.Select(x => new VersionedInstanceIdentifier(x.StudyInstanceUid, x.SeriesInstanceUid, x.SopInstanceUid, x.OriginalVersion)).ToList();
            await context.CallActivityWithRetryAsync(nameof(CleanupDeletedBatchAsync), _options.RetryOptions, instanceIdentifiers);

            // Create a new orchestration with the same instance ID to process the remaining data
            logger.LogInformation("Completed deleting the range {Range}. Total files deleted in range: '{NumFiles}'. " +
                "Continuing with new execution...", batchRange, batchRange.End - batchRange.Start + 1);

            WatermarkRange nextRange = new WatermarkRange(batchRange.End + 1, batchRange.End + input.Batching.Size);

            // Using completed to populate next set of ranges
            context.ContinueAsNew(
                new BlobMigrationCheckpoint
                {
                    Completed = nextRange,
                    CreatedTime = input.CreatedTime ?? await context.GetCreatedTimeAsync(_options.RetryOptions),
                    Batching = input.Batching,
                    MaxWatermark = maxWatermark,
                    FilterTimeStamp = null, // For next batch, ensure the timestamp is null so that we can use the watermark to fetch next batch
                });
        }
        else if (!hasReachedToEnd && input.Completed.HasValue)
        {
            logger.LogInformation("No change feed deleted entries in this batch {MaxWatermark} - {CurrentWatermark}. Queuing for next batch.", maxWatermark, input.Completed.Value.End);
            WatermarkRange nextRange = new WatermarkRange(input.Completed.Value.End + 1, input.Completed.Value.End + input.Batching.Size);

            context.ContinueAsNew(
                new BlobMigrationCheckpoint
                {
                    Completed = nextRange,
                    CreatedTime = input.CreatedTime ?? await context.GetCreatedTimeAsync(_options.RetryOptions),
                    Batching = input.Batching,
                    MaxWatermark = maxWatermark,
                    FilterTimeStamp = null, // For next batch, ensure the timestamp is null so that we can use the watermark to fetch next batch
                });
        }
        else
        {
            logger.LogInformation("Completed deleting files.");
        }
    }
}