﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EnsureThat;
using FellowOakDicom;
using Microsoft.Health.Dicom.Client;
using Microsoft.Health.Dicom.Client.Models;
using Microsoft.Health.Dicom.Tests.Common;
using Microsoft.Health.Dicom.Web.Tests.E2E.Common;
using Microsoft.Health.Dicom.Web.Tests.E2E.Extensions;
using Microsoft.Health.Operations;
using Xunit;
using FunctionsStartup = Microsoft.Health.Dicom.Functions.App.Startup;
using WebStartup = Microsoft.Health.Dicom.Web.Startup;

namespace Microsoft.Health.Dicom.Web.Tests.E2E.Rest;

public class ExportTests : IClassFixture<WebJobsIntegrationTestFixture<WebStartup, FunctionsStartup>>, IAsyncLifetime
{
    private readonly IDicomWebClient _client;
    private readonly DicomInstancesManager _instanceManager;
    private readonly BlobContainerClient _containerClient;
    private readonly ExportDataOptions<ExportDestinationType> _destination;

    private const string ExpectedPathPattern = "{0}/Results/{1}/{2}/{3}.dcm";
    private const string ExportContainer = "ExportE2E";

    public ExportTests(WebJobsIntegrationTestFixture<WebStartup, FunctionsStartup> fixture)
    {
        _client = EnsureArg.IsNotNull(fixture, nameof(fixture)).GetDicomWebClient();
        _instanceManager = new DicomInstancesManager(_client);

        if (!fixture.IsInProcess)
            throw new NotSupportedException();

        _containerClient = new BlobContainerClient("UseDevelopmentStorage=true", ExportContainer);
        _destination = ExportDestination.ForAzureBlobStorage("UseDevelopmentStorage=true", ExportContainer);
    }

    [Fact] // TODO: Enable as BVT
    public async Task GivenFiles_WhenExport_ThenSuccessfullyCopy()
    {
        // Define DICOM files
        string studyUid1 = TestUidGenerator.Generate();
        DicomDataset instance1 = Samples.CreateRandomInstanceDataset(studyUid1);
        DicomDataset instance2 = Samples.CreateRandomInstanceDataset(studyUid1);
        DicomDataset instance3 = Samples.CreateRandomInstanceDataset(studyUid1);

        string studyUid2 = TestUidGenerator.Generate();
        string seriesUid2 = TestUidGenerator.Generate();
        DicomDataset instance4 = Samples.CreateRandomInstanceDataset(studyUid2, seriesUid2);
        DicomDataset instance5 = Samples.CreateRandomInstanceDataset(studyUid2, seriesUid2);
        DicomDataset instance6 = Samples.CreateRandomInstanceDataset(studyUid2, seriesUid2);

        string studyUid3 = TestUidGenerator.Generate();
        string seriesUid3 = TestUidGenerator.Generate();
        DicomDataset instance7 = Samples.CreateRandomInstanceDataset();
        DicomDataset instance8 = Samples.CreateRandomInstanceDataset(studyUid3);
        DicomDataset instance9 = Samples.CreateRandomInstanceDataset(studyUid3, seriesUid3);

        var instances = new Dictionary<DicomIdentifier, DicomDataset>
        {
            { DicomIdentifier.ForInstance(instance1), instance1 },
            { DicomIdentifier.ForInstance(instance2), instance2 },
            { DicomIdentifier.ForInstance(instance3), instance3 },
            { DicomIdentifier.ForInstance(instance4), instance4 },
            { DicomIdentifier.ForInstance(instance5), instance5 },
            { DicomIdentifier.ForInstance(instance6), instance6 },
            { DicomIdentifier.ForInstance(instance7), instance7 },
            { DicomIdentifier.ForInstance(instance8), instance8 },
            { DicomIdentifier.ForInstance(instance9), instance9 },
        };

        // Upload files
        await Task.WhenAll(instances.Select(x => _instanceManager.StoreAsync(new DicomFile(x.Value))));

        // Begin Export
        DicomWebResponse<DicomOperationReference> response = await _client.StartExportAsync(
            ExportSource.ForIdentifiers(
                DicomIdentifier.ForStudy(studyUid1),
                DicomIdentifier.ForSeries(studyUid2, seriesUid2),
                DicomIdentifier.ForInstance(instance7),
                DicomIdentifier.ForInstance(instance8),
                DicomIdentifier.ForInstance(instance9)),
            _destination);

        // Wait for the operation to complete
        DicomOperationReference operation = await response.GetValueAsync();
        OperationState<DicomOperation> result = await _client.WaitForCompletionAsync(operation.Id);
        Assert.Equal(OperationStatus.Completed, result.Status);

        // Validate the results
        Assert.True(await _containerClient.ExistsAsync());
        List<BlobItem> actual = await _containerClient
            .GetBlobsAsync()
            .ToListAsync();

        Assert.Equal(instances.Count, actual.Count);
        foreach (BlobItem blob in actual)
        {
            BlobClient blobClient = _containerClient.GetBlobClient(blob.Name);
            using Stream data = await blobClient.OpenReadAsync();
            DicomFile file = await GetDicomFileAsync(data);

            DicomIdentifier identifier = DicomIdentifier.ForInstance(file.Dataset);
            Assert.True(instances.Remove(identifier)); // TODO: Compare DICOM file
            Assert.Equal(GetExpectedPath(operation.Id, identifier), blob.Name);
        }
    }

    public Task InitializeAsync()
        => Task.CompletedTask;

    public Task DisposeAsync()
        => _instanceManager.DisposeAsync().AsTask();

    private static async Task<DicomFile> GetDicomFileAsync(Stream stream)
    {
        // DicomFile requires that the stream be seekable
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer);
        buffer.Position = 0;

        return await DicomFile.OpenAsync(buffer);
    }

    private static string GetExpectedPath(Guid operationId, DicomIdentifier identifer)
        => string.Format(
            CultureInfo.InvariantCulture,
            ExpectedPathPattern,
            operationId.ToString(OperationId.FormatSpecifier),
            identifer.StudyInstanceUid,
            identifer.SeriesInstanceUid,
            identifer.SopInstanceUid);
}
