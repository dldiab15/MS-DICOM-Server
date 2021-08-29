// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;
using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.Core.Models.Operations;
using Microsoft.Health.Dicom.Functions.Extensions;
using Microsoft.Health.Dicom.Functions.Indexing.Models;

namespace Microsoft.Health.Dicom.Functions.Indexing
{
    public partial class ReindexDurableFunction
    {
        /// <summary>
        /// Asynchronously creates an index for the provided query tags over the previously added data.
        /// </summary>
        /// <remarks>
        /// Durable functions are reliable, and their implementations will be executed repeatedly over the lifetime of
        /// a single instance.
        /// </remarks>
        /// <param name="context">The context for the orchestration instance.</param>
        /// <param name="logger">A diagnostic logger.</param>
        /// <returns>A task representing the <see cref="ReindexInstancesAsync"/> operation.</returns>
        [FunctionName(nameof(ReindexInstancesAsync))]
        public async Task ReindexInstancesAsync(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger logger)
        {
            EnsureArg.IsNotNull(context, nameof(context));

            logger = context.CreateReplaySafeLogger(logger);
            ReindexInput input = context.GetInput<ReindexInput>();

            // The ID should be a GUID as generated by the trigger, but we'll assert here just to make sure!
            if (!context.HasInstanceGuid())
            {
                return;
            }

            // Fetch the set of query tags that require re-indexing
            IReadOnlyList<ExtendedQueryTagStoreEntry> queryTags = await GetOperationQueryTagsAsync(context, input);
            logger.LogInformation(
                "Found {Count} extended query tag paths to re-index {{{TagPaths}}}.",
                queryTags.Count,
                string.Join(", ", queryTags.Select(x => x.Path)));

            List<int> queryTagKeys = queryTags.Select(x => x.Key).ToList();
            if (queryTags.Count > 0)
            {
                List<WatermarkRange> batches = await GetBatchesAsync(context, input.Completed, logger);
                if (batches.Count > 0)
                {
                    // Note that batches are in reverse order because we start from the highest watermark
                    var batchRange = WatermarkRange.Between(batches[^1].Start, batches[0].End);

                    logger.LogInformation("Beginning to re-index the range {Range}.", batchRange);
                    await Task.WhenAll(batches
                        .Select(x => context.CallActivityWithRetryAsync(
                            nameof(ReindexBatchAsync),
                            _options.ActivityRetryOptions,
                            new ReindexBatch { QueryTags = queryTags, WatermarkRange = x })));

                    // Create a new orchestration with the same instance ID to process the remaining data
                    logger.LogInformation("Completed re-indexing the range {Range}. Continuing with new execution...", batchRange);

                    WatermarkRange completed = input.Completed.Count == 0
                        ? batchRange
                        : WatermarkRange.Between(batchRange.Start, input.Completed.End);

                    context.SetCustomStatus(
                        new OperationCustomStatus
                        {
                            PercentComplete = (int)((double)(completed.End - completed.Start) / (completed.End - 1) * 100),
                            ResourceIds = queryTags.Select(x => x.Path).ToList(),
                        });

                    context.ContinueAsNew(
                        new ReindexInput
                        {
                            QueryTagKeys = queryTagKeys,
                            Completed = completed,
                        });
                }
                else
                {
                    IReadOnlyList<int> completed = await context.CallActivityWithRetryAsync<IReadOnlyList<int>>(
                        nameof(CompleteReindexingAsync),
                        _options.ActivityRetryOptions,
                        queryTagKeys);

                    logger.LogInformation(
                        "Completed re-indexing for the following extended query tags {{{QueryTagKeys}}}.",
                        string.Join(", ", completed));

                    context.SetCustomStatus(
                        new OperationCustomStatus
                        {
                            PercentComplete = 100,
                            ResourceIds = queryTags.Select(x => x.Path).ToList(),
                        });
                }
            }
            else
            {
                logger.LogWarning(
                    "Could not find any query tags for the re-indexing operation '{OperationId}'.",
                    context.InstanceId);

                context.SetCustomStatus(
                    new OperationCustomStatus
                    {
                        PercentComplete = 100,
                        ResourceIds = null,
                    });
            }
        }

        private Task<IReadOnlyList<ExtendedQueryTagStoreEntry>> GetOperationQueryTagsAsync(IDurableOrchestrationContext context, ReindexInput input)
            // Determine the set of query tags that should be indexed and only continue if there is at least 1.
            // For the first time this orchestration executes, assign all of the tags in the input to the operation,
            // otherwise simply fetch the tags from the database for this operation.
            => input.Completed.Count == 0
                ? context.CallActivityWithRetryAsync<IReadOnlyList<ExtendedQueryTagStoreEntry>>(
                    nameof(AssignReindexingOperationAsync),
                    _options.ActivityRetryOptions,
                    input.QueryTagKeys)
                : context.CallActivityWithRetryAsync<IReadOnlyList<ExtendedQueryTagStoreEntry>>(
                    nameof(GetQueryTagsAsync),
                    _options.ActivityRetryOptions,
                    null);

        private async Task<List<WatermarkRange>> GetBatchesAsync(
            IDurableOrchestrationContext context,
            WatermarkRange completed,
            ILogger logger)
        {
            // If we haven't completed any range yet, fetch the maximum watermark from the database.
            // Otherwise, create a WatermarkRange based on the latest progress.
            long end;
            if (completed.Count > 0)
            {
                end = completed.Start;
                logger.LogInformation("Previous execution finished range {Range}.", completed);
            }
            else
            {
                long maxWatermark = await context.CallActivityWithRetryAsync<long>(
                    nameof(GetMaxInstanceWatermarkAsync),
                    _options.ActivityRetryOptions,
                    null);

                end = maxWatermark + 1;
                logger.LogInformation("Found maximum watermark {Max}.", maxWatermark);
            }

            // Note that the watermark sequence starts at 1!
            var batches = new List<WatermarkRange>();
            for (; end > 1 && batches.Count < _options.MaxParallelBatches; end -= _options.BatchSize)
            {
                int count = (int)Math.Min(end - 1, _options.BatchSize);
                batches.Add(new WatermarkRange(end - count, count));
            }

            return batches;
        }
    }
}
