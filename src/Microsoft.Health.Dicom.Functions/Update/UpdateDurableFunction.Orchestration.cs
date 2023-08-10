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
using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.Core.Features.Partitioning;
using Microsoft.Health.Dicom.Functions.Registration;
using Microsoft.Health.Dicom.Functions.Update.Models;
using Microsoft.Health.Operations.Functions.DurableTask;

namespace Microsoft.Health.Dicom.Functions.Update;

public partial class UpdateDurableFunction
{
    /// <summary>
    /// Asynchronously updates list of instances in a study
    /// </summary>
    /// <remarks>
    /// Durable functions are reliable, and their implementations will be executed repeatedly over the lifetime of
    /// a single instance.
    /// </remarks>
    /// <param name="context">The context for the orchestration instance.</param>
    /// <param name="logger">A diagnostic logger.</param>
    /// <returns>A task representing the <see cref="UpdateInstancesAsync"/> operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> or <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">Orchestration instance ID is invalid.</exception>
    [FunctionName(nameof(UpdateInstancesAsync))]
    [Obsolete("This function is obsolete and will be removed in upcoming release. Use UpdateInstancesAsyncV2 instead.")]
    public async Task UpdateInstancesAsync(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger logger)
    {
        EnsureArg.IsNotNull(context, nameof(context)).ThrowIfInvalidOperationId();
        logger = context.CreateReplaySafeLogger(EnsureArg.IsNotNull(logger, nameof(logger)));
        ReplaySafeCounter<int> replaySafeCounter = context.CreateReplaySafeCounter(_updateMeter.UpdatedInstances);

        UpdateCheckpoint input = context.GetInput<UpdateCheckpoint>();

        if (input.NumberOfStudyCompleted < input.TotalNumberOfStudies)
        {
            string studyInstanceUid = input.StudyInstanceUids[input.NumberOfStudyCompleted];

            logger.LogInformation("Beginning to update all instances new watermark in a study.");

            IReadOnlyList<InstanceFileState> instanceWatermarks = await context.CallActivityWithRetryAsync<IReadOnlyList<InstanceFileState>>(
                nameof(UpdateInstanceWatermarkAsync),
                _options.RetryOptions,
                new UpdateInstanceWatermarkArguments(input.PartitionKey, studyInstanceUid));

            logger.LogInformation("Updated all instances new watermark in a study. Found {InstanceCount} instance for study", instanceWatermarks.Count);

            var totalNoOfInstances = input.TotalNumberOfInstanceUpdated;
            int numberofStudyFailed = input.NumberOfStudyFailed;

            if (instanceWatermarks.Count > 0)
            {
                try
                {
                    await context.CallActivityWithRetryAsync(
                        nameof(UpdateInstanceBlobsAsync),
                        _options.RetryOptions,
                        new UpdateInstanceBlobArguments(input.PartitionKey, instanceWatermarks, input.ChangeDataset));

                    await context.CallActivityWithRetryAsync(
                        nameof(CompleteUpdateStudyAsync),
                        _options.RetryOptions,
                        new CompleteStudyArguments(input.PartitionKey, studyInstanceUid, input.ChangeDataset));

                    totalNoOfInstances += instanceWatermarks.Count;
                }
                catch (FunctionFailedException ex)
                {
                    logger.LogError(ex, "Failed to update instances for study", ex);
                    var errors = new List<string>
                    {
                        $"Failed to update instances for study {studyInstanceUid}",
                    };

                    if (input.Errors != null)
                        errors.AddRange(errors);

                    input.Errors = errors;

                    numberofStudyFailed++;

                    // Cleanup the new version when the update activity fails
                    await TryCleanupActivity(context, instanceWatermarks, new Partition(input.PartitionKey, Partition.UnknownName));
                }
            }

            var numberOfStudyCompleted = input.NumberOfStudyCompleted + 1;

            if (input.TotalNumberOfStudies != numberOfStudyCompleted)
            {
                logger.LogInformation("Completed updating the instances for a study. {Updated}. Continuing with new execution...", instanceWatermarks.Count);
            }
            else
            {
                await context.CallActivityWithRetryAsync(
                    nameof(DeleteOldVersionBlobAsync),
                    _options.RetryOptions,
                    instanceWatermarks);
            }

            context.ContinueAsNew(
                new UpdateCheckpoint
                {
                    StudyInstanceUids = input.StudyInstanceUids,
                    ChangeDataset = input.ChangeDataset,
                    PartitionKey = input.PartitionKey,
                    NumberOfStudyCompleted = numberOfStudyCompleted,
                    NumberOfStudyFailed = numberofStudyFailed,
                    TotalNumberOfInstanceUpdated = totalNoOfInstances,
                    Errors = input.Errors,
                    CreatedTime = input.CreatedTime ?? await context.GetCreatedTimeAsync(_options.RetryOptions),
                });
        }
        else
        {
            if (input.Errors?.Count > 0)
            {
                logger.LogWarning("Update operation completed with errors. {NumberOfStudyUpdated}, {NumberOfStudyFailed}, {TotalNumberOfInstanceUpdated}.",
                    input.NumberOfStudyCompleted - input.NumberOfStudyFailed,
                    input.NumberOfStudyFailed,
                    input.TotalNumberOfInstanceUpdated);

                // Throwing the exception so that it can set the operation status to Failed
                throw new OperationErrorException("Update operation completed with errors.");
            }
            else
            {
                logger.LogInformation("Update operation completed successfully. {NumberOfStudyUpdated}, {TotalNumberOfInstanceUpdated}.",
                    input.NumberOfStudyCompleted,
                    input.TotalNumberOfInstanceUpdated);
            }

            if (input.TotalNumberOfInstanceUpdated > 0)
            {
                replaySafeCounter.Add(input.TotalNumberOfInstanceUpdated);
            }
        }
    }

    /// <summary>
    /// Asynchronously updates list of instances in a study
    /// </summary>
    /// <remarks>
    /// Durable functions are reliable, and their implementations will be executed repeatedly over the lifetime of
    /// a single instance.
    /// </remarks>
    /// <param name="context">The context for the orchestration instance.</param>
    /// <param name="logger">A diagnostic logger.</param>
    /// <returns>A task representing the <see cref="UpdateInstancesV2Async"/> operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> or <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">Orchestration instance ID is invalid.</exception>
    [FunctionName(nameof(UpdateInstancesV2Async))]
    public async Task UpdateInstancesV2Async(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger logger)
    {
        EnsureArg.IsNotNull(context, nameof(context)).ThrowIfInvalidOperationId();
        logger = context.CreateReplaySafeLogger(EnsureArg.IsNotNull(logger, nameof(logger)));
        ReplaySafeCounter<int> replaySafeCounter = context.CreateReplaySafeCounter(_updateMeter.UpdatedInstances);
        IReadOnlyList<InstanceMetadata> instanceMetadataList;
        UpdateCheckpoint input = context.GetInput<UpdateCheckpoint>();
        input.Partition ??= new Partition(input.PartitionKey, Partition.UnknownName);

        if (input.NumberOfStudyCompleted < input.TotalNumberOfStudies)
        {
            string studyInstanceUid = input.StudyInstanceUids[input.NumberOfStudyCompleted];

            logger.LogInformation("Beginning to update all instances new watermark in a study.");

            IReadOnlyList<InstanceMetadata> instances = await context
                .CallActivityWithRetryAsync<IReadOnlyList<InstanceMetadata>>(
                    nameof(UpdateInstanceWatermarkV2Async),
                    _options.RetryOptions,
                    new UpdateInstanceWatermarkArguments(input.Partition, studyInstanceUid));
            var instanceWatermarks = instances.Select(x => x.ToInstanceFileState()).ToList();

            logger.LogInformation("Updated all instances new watermark in a study. Found {InstanceCount} instance for study", instances.Count);

            var totalNoOfInstances = input.TotalNumberOfInstanceUpdated;
            int numberofStudyFailed = input.NumberOfStudyFailed;

            if (instances.Count > 0)
            {
                try
                {
                    instanceMetadataList = await context.CallActivityWithRetryAsync<IReadOnlyList<InstanceMetadata>>(
                        nameof(UpdateInstanceBlobsV2Async),
                        _options.RetryOptions,
                        new UpdateInstanceBlobArguments(input.Partition, instances, input.ChangeDataset));

                    await context.CallActivityWithRetryAsync(
                        nameof(CompleteUpdateStudyV2Async),
                        _options.RetryOptions,
                        new CompleteStudyArguments(input.Partition.Key, studyInstanceUid, input.ChangeDataset, GetInstanceMetadatas(instanceMetadataList)));

                    totalNoOfInstances += instances.Count;
                }
                catch (FunctionFailedException ex)
                {
                    logger.LogError(ex, "Failed to update instances for study", ex);
                    var errors = new List<string>
                    {
                        $"Failed to update instances for study {studyInstanceUid}",
                    };

                    if (input.Errors != null)
                        errors.AddRange(errors);

                    input.Errors = errors;

                    numberofStudyFailed++;

                    // Cleanup the new version when the update activity fails
                    await TryCleanupActivityV2(context, instanceWatermarks, input.Partition);
                }
            }

            var numberOfStudyCompleted = input.NumberOfStudyCompleted + 1;

            if (input.TotalNumberOfStudies != numberOfStudyCompleted)
            {
                logger.LogInformation("Completed updating the instances for a study. {Updated}. Continuing with new execution...", instances.Count);
            }
            else
            {
                await context.CallActivityWithRetryAsync(
                    nameof(DeleteOldVersionBlobV2Async),
                    _options.RetryOptions,
                    new CleanupBlobArguments(instanceWatermarks, input.Partition));
            }

            context.ContinueAsNew(
                new UpdateCheckpoint
                {
                    StudyInstanceUids = input.StudyInstanceUids,
                    ChangeDataset = input.ChangeDataset,
                    Partition = input.Partition,
                    PartitionKey = input.PartitionKey,
                    NumberOfStudyCompleted = numberOfStudyCompleted,
                    NumberOfStudyFailed = numberofStudyFailed,
                    TotalNumberOfInstanceUpdated = totalNoOfInstances,
                    Errors = input.Errors,
                    CreatedTime = input.CreatedTime ?? await context.GetCreatedTimeAsync(_options.RetryOptions),
                });
        }
        else
        {
            if (input.Errors?.Count > 0)
            {
                logger.LogWarning("Update operation completed with errors. {NumberOfStudyUpdated}, {NumberOfStudyFailed}, {TotalNumberOfInstanceUpdated}.",
                    input.NumberOfStudyCompleted - input.NumberOfStudyFailed,
                    input.NumberOfStudyFailed,
                    input.TotalNumberOfInstanceUpdated);

                // Throwing the exception so that it can set the operation status to Failed
                throw new OperationErrorException("Update operation completed with errors.");
            }
            else
            {
                logger.LogInformation("Update operation completed successfully. {NumberOfStudyUpdated}, {TotalNumberOfInstanceUpdated}.",
                    input.NumberOfStudyCompleted,
                    input.TotalNumberOfInstanceUpdated);
            }

            if (input.TotalNumberOfInstanceUpdated > 0)
            {
                replaySafeCounter.Add(input.TotalNumberOfInstanceUpdated);
            }
        }
    }

    private IReadOnlyList<InstanceMetadata> GetInstanceMetadatas(IReadOnlyList<InstanceMetadata> instanceMetadataList)
    {
        // when external store not enabled, do not update file properties
        return _externalStoreEnabled ? instanceMetadataList : new List<InstanceMetadata>();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Using a generic exception to catch all scenarios.")]
    [Obsolete("This function is obsolete and will be removed in upcoming release. Use UpdateInstancesAsyncV2 instead.")]
    private async Task TryCleanupActivity(IDurableOrchestrationContext context, IReadOnlyList<InstanceFileState> instanceWatermarks)
    {
        try
        {
            await context.CallActivityWithRetryAsync(
                nameof(CleanupNewVersionBlobAsync),
                _options.RetryOptions,
                instanceWatermarks);
        }
        catch (Exception) { }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Using a generic exception to catch all scenarios.")]
    private async Task TryCleanupActivityV2(IDurableOrchestrationContext context, IReadOnlyList<InstanceFileState> instanceWatermarks, Partition partition)
    {
        try
        {
            await context.CallActivityWithRetryAsync(
                nameof(CleanupNewVersionBlobV2Async),
                _options.RetryOptions,
                new CleanupBlobArguments(instanceWatermarks, partition));
        }
        catch (Exception) { }
    }
}
