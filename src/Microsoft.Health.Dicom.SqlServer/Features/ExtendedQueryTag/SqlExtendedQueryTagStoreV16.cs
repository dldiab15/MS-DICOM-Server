﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Dicom.Core.Exceptions;
using Microsoft.Health.Dicom.SqlServer.Features.Schema;
using Microsoft.Health.Dicom.SqlServer.Features.Schema.Model;
using Microsoft.Health.SqlServer.Features.Client;
using Microsoft.Health.SqlServer.Features.Storage;

namespace Microsoft.Health.Dicom.SqlServer.Features.ExtendedQueryTag;

internal class SqlExtendedQueryTagStoreV16 : SqlExtendedQueryTagStoreV8
{
    public SqlExtendedQueryTagStoreV16(
       SqlConnectionWrapperFactory sqlConnectionWrapperFactory,
       ILogger<SqlExtendedQueryTagStoreV16> logger)
        : base(sqlConnectionWrapperFactory, logger)
    {
    }

    public override SchemaVersion Version => SchemaVersion.V16;

    public override async Task DeleteExtendedQueryTagAsync(string tagPath, string vr, CancellationToken cancellationToken = default)
    {
        using (SqlConnectionWrapper sqlConnectionWrapper = await ConnectionWrapperFactory.ObtainSqlConnectionWrapperAsync(cancellationToken))
        using (SqlCommandWrapper sqlCommandWrapper = sqlConnectionWrapper.CreateRetrySqlCommand())
        {
            VLatest.DeleteExtendedQueryTagV16.PopulateCommand(sqlCommandWrapper, tagPath, (byte)ExtendedQueryTagLimit.ExtendedQueryTagVRAndDataTypeMapping[vr], batchSize: 1000);

            try
            {
                await sqlCommandWrapper.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (SqlException ex)
            {
                throw ex.Number switch
                {
                    SqlErrorCodes.NotFound => new ExtendedQueryTagNotFoundException(
                        string.Format(CultureInfo.InvariantCulture, DicomSqlServerResource.ExtendedQueryTagNotFound, tagPath)),
                    _ => new DataStoreException(ex),
                };
            }
        }
    }
}
