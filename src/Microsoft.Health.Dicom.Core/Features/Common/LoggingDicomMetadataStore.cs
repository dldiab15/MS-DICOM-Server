﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Dicom;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Dicom.Core.Extensions;

namespace Microsoft.Health.Dicom.Core.Features.Common
{
    public class LoggingDicomMetadataStore : IDicomMetadataStore
    {
        private static readonly Action<ILogger, string, Exception> LogAddInstanceMetadataDelegate =
               LoggerMessage.Define<string>(
                   LogLevel.Debug,
                   default,
                   "Adding DICOM instance metadata file with '{DicomInstanceIdentifier}'.");

        private static readonly Action<ILogger, string, Exception> LogDeleteInstanceMetadataDelegate =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                default,
                "Deleting DICOM instance metadata file with '{DicomInstanceIdentifier}'.");

        private static readonly Action<ILogger, string, Exception> LogGetInstanceMetadataDelegate =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                default,
                "Getting the DICOM instance metadata file with '{DicomInstanceIdentifier}'.");

        private static readonly Action<ILogger, Exception> LogOperationSucceededDelegate =
            LoggerMessage.Define(
                LogLevel.Debug,
                default,
                "The operation completed successfully.");

        private static readonly Action<ILogger, Exception> LogOperationFailedDelegate =
            LoggerMessage.Define(
                LogLevel.Warning,
                default,
                "The operation failed.");

        private readonly IDicomMetadataStore _dicomMetadataStore;
        private readonly ILogger _logger;

        public LoggingDicomMetadataStore(IDicomMetadataStore dicomMetadataStore, ILogger<LoggingDicomMetadataStore> logger)
        {
            EnsureArg.IsNotNull(dicomMetadataStore, nameof(dicomMetadataStore));
            EnsureArg.IsNotNull(logger, nameof(logger));

            _dicomMetadataStore = dicomMetadataStore;
            _logger = logger;
        }

        public async Task AddInstanceMetadataAsync(DicomDataset dicomDataset, long version, CancellationToken cancellationToken)
        {
            LogAddInstanceMetadataDelegate(_logger, dicomDataset.ToVersionedDicomInstanceIdentifier(version).ToString(), null);

            try
            {
                await _dicomMetadataStore.AddInstanceMetadataAsync(dicomDataset, version, cancellationToken);

                LogOperationSucceededDelegate(_logger, null);
            }
            catch (Exception ex)
            {
                LogOperationFailedDelegate(_logger, ex);

                throw;
            }
        }

        public async Task DeleteInstanceMetadataAsync(VersionedDicomInstanceIdentifier dicomInstanceIdentifier, CancellationToken cancellationToken)
        {
            LogDeleteInstanceMetadataDelegate(_logger, dicomInstanceIdentifier.ToString(), null);

            try
            {
                await _dicomMetadataStore.DeleteInstanceMetadataAsync(dicomInstanceIdentifier, cancellationToken);

                LogOperationSucceededDelegate(_logger, null);
            }
            catch (Exception ex)
            {
                LogOperationFailedDelegate(_logger, ex);

                throw;
            }
        }

        public async Task<DicomDataset> GetInstanceMetadataAsync(VersionedDicomInstanceIdentifier dicomInstanceIdentifier, CancellationToken cancellationToken)
        {
            LogGetInstanceMetadataDelegate(_logger, dicomInstanceIdentifier.ToString(), null);

            try
            {
                DicomDataset dicomDataset = await GetInstanceMetadataAsync(dicomInstanceIdentifier, cancellationToken);

                LogOperationSucceededDelegate(_logger, null);

                return dicomDataset;
            }
            catch (Exception ex)
            {
                LogOperationFailedDelegate(_logger, ex);

                throw;
            }
        }
    }
}
