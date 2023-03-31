// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.SqlServer.Features.Schema;
using Microsoft.Health.Dicom.SqlServer.Features.Schema.Model;
using Microsoft.Health.SqlServer.Features.Client;
using Microsoft.Health.SqlServer.Features.Storage;

namespace Microsoft.Health.Dicom.SqlServer.Features.Retrieve;

internal class SqlInstanceStoreV32 : SqlInstanceStoreV23
{
    public SqlInstanceStoreV32(SqlConnectionWrapperFactory sqlConnectionWrapperFactory) : base(sqlConnectionWrapperFactory)
    {
    }

    public override SchemaVersion Version => SchemaVersion.V32;

    public override async Task<IReadOnlyList<InstanceMetadata>> GetInstanceIdentifiersInStudyByWatermarkAsync(int batchSize, int partitionKey, string studyInstanceUid, long? maxWatermark, CancellationToken cancellationToken = default)
    {
        var results = new List<InstanceMetadata>();

        using (SqlConnectionWrapper sqlConnectionWrapper = await SqlConnectionWrapperFactory.ObtainSqlConnectionWrapperAsync(cancellationToken))
        using (SqlCommandWrapper sqlCommandWrapper = sqlConnectionWrapper.CreateRetrySqlCommand())
        {
            VLatest.GetInstancesByStudyAndWatermark.PopulateCommand(
                sqlCommandWrapper,
                batchSize,
                partitionKey,
                studyInstanceUid,
                maxWatermark);

            using (var reader = await sqlCommandWrapper.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    (string rStudyInstanceUid, string rSeriesInstanceUid, string rSopInstanceUid, long watermark, long? originalWatermark, long? newWatermark) = reader.ReadRow(
                       VLatest.Instance.StudyInstanceUid,
                       VLatest.Instance.SeriesInstanceUid,
                       VLatest.Instance.SopInstanceUid,
                       VLatest.Instance.Watermark,
                       VLatest.Instance.OriginalWatermark,
                       VLatest.Instance.NewWatermark);

                    results.Add(
                        new InstanceMetadata(
                            new VersionedInstanceIdentifier(
                                rStudyInstanceUid,
                                rSeriesInstanceUid,
                                rSopInstanceUid,
                                watermark,
                                partitionKey),
                            new InstanceProperties() { OriginalVersion = originalWatermark, NewVersion = newWatermark }));
                }
            }
        }

        return results;
    }
}