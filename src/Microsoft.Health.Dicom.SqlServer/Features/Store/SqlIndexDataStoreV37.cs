﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using FellowOakDicom;
using Microsoft.Data.SqlClient;
using Microsoft.Health.Dicom.Core.Exceptions;
using Microsoft.Health.Dicom.Core.Extensions;
using Microsoft.Health.Dicom.Core.Features.Common;
using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;
using Microsoft.Health.Dicom.Core.Models;
using Microsoft.Health.Dicom.SqlServer.Features.ExtendedQueryTag;
using Microsoft.Health.Dicom.SqlServer.Features.Schema;
using Microsoft.Health.Dicom.SqlServer.Features.Schema.Model;
using Microsoft.Health.SqlServer.Features.Client;
using Microsoft.Health.SqlServer.Features.Storage;

namespace Microsoft.Health.Dicom.SqlServer.Features.Store;

internal class SqlIndexDataStoreV37 : SqlIndexDataStoreV35
{
    public SqlIndexDataStoreV37(SqlConnectionWrapperFactory sqlConnectionWrapperFactory)
        : base(sqlConnectionWrapperFactory)
    {
    }

    public override SchemaVersion Version => SchemaVersion.V37;

    public override async Task EndCreateInstanceIndexAsync(int partitionKey, DicomDataset dicomDataset, long watermark, IEnumerable<QueryTag> queryTags, FileProperties fileProperties = null, long? instanceKey = null, bool allowExpiredTags = false, bool hasFrameMetadata = false, CancellationToken cancellationToken = default)
    {
        EnsureArg.IsNotNull(dicomDataset, nameof(dicomDataset));
        EnsureArg.IsNotNull(queryTags, nameof(queryTags));

        using (SqlConnectionWrapper sqlConnectionWrapper = await SqlConnectionWrapperFactory.ObtainSqlConnectionWrapperAsync(cancellationToken))
        using (SqlCommandWrapper sqlCommandWrapper = sqlConnectionWrapper.CreateRetrySqlCommand())
        {
            VLatest.UpdateInstanceStatusV37.PopulateCommand(
                sqlCommandWrapper,
                partitionKey,
                dicomDataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty),
                dicomDataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty),
                dicomDataset.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, string.Empty),
                watermark,
                (byte)IndexStatus.Created,
                allowExpiredTags ? null : ExtendedQueryTagDataRowsBuilder.GetMaxTagKey(queryTags),
                hasFrameMetadata,
                fileProperties?.Path,
                fileProperties?.ETag,
                instanceKey
            );

            try
            {
                await sqlCommandWrapper.ExecuteScalarAsync(cancellationToken);
            }
            catch (SqlException ex)
            {
                throw ex.Number switch
                {
                    SqlErrorCodes.NotFound => new InstanceNotFoundException(),
                    SqlErrorCodes.Conflict when ex.State == 10 => new ExtendedQueryTagsOutOfDateException(),
                    _ => new DataStoreException(ex),
                };
            }
        }
    }

    public override async Task<InstanceStorageKey> BeginCreateInstanceIndexAsync(int partitionKey, DicomDataset dicomDataset, IEnumerable<QueryTag> queryTags, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(dicomDataset, nameof(dicomDataset));
        EnsureArg.IsNotNull(queryTags, nameof(queryTags));

        using (SqlConnectionWrapper sqlConnectionWrapper = await SqlConnectionWrapperFactory.ObtainSqlConnectionWrapperAsync(cancellationToken))
        using (SqlCommandWrapper sqlCommandWrapper = sqlConnectionWrapper.CreateRetrySqlCommand())
        {
            var rows = ExtendedQueryTagDataRowsBuilder.Build(dicomDataset, queryTags.Where(tag => tag.IsExtendedQueryTag), Version);
            VLatest.AddInstanceV37TableValuedParameters parameters = new VLatest.AddInstanceV37TableValuedParameters(
                rows.StringRows,
                rows.LongRows,
                rows.DoubleRows,
                rows.DateTimeWithUtcRows,
                rows.PersonNameRows);

            VLatest.AddInstanceV37.PopulateCommand(
                sqlCommandWrapper,
                partitionKey,
                dicomDataset.GetString(DicomTag.StudyInstanceUID),
                dicomDataset.GetString(DicomTag.SeriesInstanceUID),
                dicomDataset.GetString(DicomTag.SOPInstanceUID),
                dicomDataset.GetFirstValueOrDefault<string>(DicomTag.PatientID),
                dicomDataset.GetFirstValueOrDefault<string>(DicomTag.PatientName),
                dicomDataset.GetFirstValueOrDefault<string>(DicomTag.ReferringPhysicianName),
                dicomDataset.GetStringDateAsDate(DicomTag.StudyDate),
                dicomDataset.GetFirstValueOrDefault<string>(DicomTag.StudyDescription),
                dicomDataset.GetFirstValueOrDefault<string>(DicomTag.AccessionNumber),
                dicomDataset.GetFirstValueOrDefault<string>(DicomTag.Modality),
                dicomDataset.GetStringDateAsDate(DicomTag.PerformedProcedureStepStartDate),
                dicomDataset.GetStringDateAsDate(DicomTag.PatientBirthDate),
                dicomDataset.GetFirstValueOrDefault<string>(DicomTag.ManufacturerModelName),
                (byte)IndexStatus.Creating,
                dicomDataset.InternalTransferSyntax?.UID.UID,
                parameters);

            try
            {
                using SqlDataReader reader = await sqlCommandWrapper.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                {
                    throw new DataStoreException("Error creating index");
                }

                (long watermark, long? instanceKey) = reader.ReadRow(VLatest.Instance.Watermark,
                    VLatest.Instance.InstanceKey);
                return (watermark, instanceKey);
            }
            catch (SqlException ex) when (ex.Number == SqlErrorCodes.Conflict && ex.State == (byte)IndexStatus.Creating)
            {
                throw new PendingInstanceException();
            }
            catch (SqlException ex) when (ex.Number == SqlErrorCodes.Conflict)
            {
                throw new InstanceAlreadyExistsException();
            }
            catch (SqlException ex)
            {
                throw new DataStoreException(ex);
            }
        }
    }
}