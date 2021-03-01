﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Dicom;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Health.Dicom.Core.Features.Common;
using Microsoft.Health.Dicom.Core.Features.CustomTag;
using Microsoft.Health.Dicom.Core.Features.Retrieve;
using Microsoft.Health.Dicom.Core.Features.Store;
using Microsoft.Health.Dicom.SqlServer.Features.CustomTag;
using Microsoft.Health.Dicom.SqlServer.Features.Retrieve;
using Microsoft.Health.Dicom.SqlServer.Features.Schema;
using Microsoft.Health.Dicom.SqlServer.Features.Schema.Model;
using Microsoft.Health.Dicom.SqlServer.Features.Storage;
using Microsoft.Health.Dicom.SqlServer.Features.Store;
using Microsoft.Health.SqlServer;
using Microsoft.Health.SqlServer.Configs;
using Microsoft.Health.SqlServer.Features.Client;
using Microsoft.Health.SqlServer.Features.Schema;
using Microsoft.Health.SqlServer.Features.Schema.Manager;
using Microsoft.Health.SqlServer.Features.Storage;
using NSubstitute;
using Polly;
using Xunit;

namespace Microsoft.Health.Dicom.Tests.Integration.Persistence
{
    public class SqlDataStoreTestsFixture : IAsyncLifetime
    {
        private const string LocalConnectionString = "server=(local);Integrated Security=true";

        private readonly string _masterConnectionString;
        private readonly string _databaseName;
        private readonly SchemaInitializer _schemaInitializer;

        public SqlDataStoreTestsFixture()
        {
            string initialConnectionString = Environment.GetEnvironmentVariable("SqlServer:ConnectionString") ?? LocalConnectionString;

            _databaseName = $"DICOMINTEGRATIONTEST_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{BigInteger.Abs(new BigInteger(Guid.NewGuid().ToByteArray()))}";
            _masterConnectionString = new SqlConnectionStringBuilder(initialConnectionString) { InitialCatalog = "master" }.ToString();
            TestConnectionString = new SqlConnectionStringBuilder(initialConnectionString) { InitialCatalog = _databaseName }.ToString();

            var config = new SqlServerDataStoreConfiguration
            {
                ConnectionString = TestConnectionString,
                Initialize = true,
                SchemaOptions = new SqlServerSchemaOptions
                {
                    AutomaticUpdatesEnabled = true,
                },
            };

            var scriptProvider = new ScriptProvider<SchemaVersion>();

            var baseScriptProvider = new BaseScriptProvider();

            var mediator = Substitute.For<IMediator>();

            var sqlConnectionStringProvider = new DefaultSqlConnectionStringProvider(config);

            var sqlConnectionFactory = new DefaultSqlConnectionFactory(sqlConnectionStringProvider);

            var schemaManagerDataStore = new SchemaManagerDataStore(sqlConnectionFactory);

            var schemaUpgradeRunner = new SchemaUpgradeRunner(scriptProvider, baseScriptProvider, mediator, NullLogger<SchemaUpgradeRunner>.Instance, sqlConnectionFactory, schemaManagerDataStore);

            var schemaInformation = new SchemaInformation((int)SchemaVersion.V1, (int)SchemaVersion.V2);

            _schemaInitializer = new SchemaInitializer(config, schemaUpgradeRunner, schemaInformation, sqlConnectionFactory, sqlConnectionStringProvider, NullLogger<SchemaInitializer>.Instance);

            var dicomSqlIndexSchema = new SqlIndexSchema(schemaInformation, NullLogger<SqlIndexSchema>.Instance);

            SqlTransactionHandler = new SqlTransactionHandler();

            SqlConnectionWrapperFactory = new SqlConnectionWrapperFactory(SqlTransactionHandler, new SqlCommandWrapperFactory(), sqlConnectionFactory);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSqlServerTableRowParameterGenerators();
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            var addInstanceTvpGenerator = serviceProvider.GetRequiredService<VLatest.AddInstanceTvpGenerator<(IReadOnlyList<CustomTagStoreEntry>, DicomDataset)>>();

            IndexDataStore = new SqlIndexDataStore(
                dicomSqlIndexSchema,
                SqlConnectionWrapperFactory,
                addInstanceTvpGenerator,
                new DicomTagParser());

            InstanceStore = new SqlInstanceStore(SqlConnectionWrapperFactory);

            CustomTagStore = new SqlCustomTagStore(SqlConnectionWrapperFactory, schemaInformation, NullLogger<SqlCustomTagStore>.Instance);

            TestHelper = new SqlIndexDataStoreTestHelper(TestConnectionString);
        }

        public SqlTransactionHandler SqlTransactionHandler { get; }

        public SqlConnectionWrapperFactory SqlConnectionWrapperFactory { get; }

        public string TestConnectionString { get; }

        public IIndexDataStore IndexDataStore { get; }

        public IInstanceStore InstanceStore { get; }

        public ICustomTagStore CustomTagStore { get; }

        public SqlIndexDataStoreTestHelper TestHelper { get; }

        public async Task InitializeAsync()
        {
            // Create the database
            using (var sqlConnection = new SqlConnection(_masterConnectionString))
            {
                await sqlConnection.OpenAsync();

                using (SqlCommand command = sqlConnection.CreateCommand())
                {
                    command.CommandTimeout = 600;
                    command.CommandText = $"CREATE DATABASE {_databaseName}";
                    await command.ExecuteNonQueryAsync();
                }
            }

            // verify that we can connect to the new database. This sometimes does not work right away with Azure SQL.
            await Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: 7,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                .ExecuteAsync(async () =>
                {
                    using (var sqlConnection = new SqlConnection(TestConnectionString))
                    {
                        await sqlConnection.OpenAsync();
                        using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                        {
                            sqlCommand.CommandText = "SELECT 1";
                            await sqlCommand.ExecuteScalarAsync();
                        }
                    }
                });

            await _schemaInitializer.InitializeAsync();
        }

        public async Task DisposeAsync()
        {
            using (var sqlConnection = new SqlConnection(_masterConnectionString))
            {
                await sqlConnection.OpenAsync();
                SqlConnection.ClearAllPools();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandTimeout = 600;
                    sqlCommand.CommandText = $"DROP DATABASE IF EXISTS {_databaseName}";
                    await sqlCommand.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
