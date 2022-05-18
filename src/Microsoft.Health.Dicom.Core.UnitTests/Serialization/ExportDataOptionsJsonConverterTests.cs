﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Health.Dicom.Core.Models;
using Microsoft.Health.Dicom.Core.Models.Common;
using Microsoft.Health.Dicom.Core.Models.Export;
using Microsoft.Health.Dicom.Core.Serialization;
using Xunit;

namespace Microsoft.Health.Dicom.Core.UnitTests.Serialization;

public class ExportDataOptionsJsonConverterTests
{
    private readonly JsonSerializerOptions _serializerOptions;

    public ExportDataOptionsJsonConverterTests()
    {
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };
        _serializerOptions.Converters.Add(new DicomIdentifierJsonConverter());
        _serializerOptions.Converters.Add(new ExportDataOptionsJsonConverterFactory());
        _serializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    [Fact]
    public void GivenSourceOptionsJson_WhenReading_ThenDeserialize()
    {
        const string json = @"{
  ""type"": ""identifiers"",
  ""settings"": {
    ""values"": [
      ""1.2"",
      ""3.4.5/67"",
      ""8.9.10/1.112.13/1.4""
    ]
  }
}";

        ExportDataOptions<ExportSourceType> actual = JsonSerializer.Deserialize<ExportDataOptions<ExportSourceType>>(json, _serializerOptions);
        Assert.Equal(ExportSourceType.Identifiers, actual.Type);

        var options = actual.Settings as IdentifierExportOptions;
        Assert.Equal(3, options.Values.Count);
        Assert.True(options.Values.SequenceEqual(
            new DicomIdentifier[]
            {
                DicomIdentifier.ForStudy("1.2"),
                DicomIdentifier.ForSeries("3.4.5", "67"),
                DicomIdentifier.ForInstance("8.9.10", "1.112.13", "1.4"),
            }));
    }

    [Fact]
    public void GivenDestinationOptionsJson_WhenReading_ThenDeserialize()
    {
        const string json = @"{
  ""type"": ""azureblob"",
  ""settings"": {
    ""connectionString"": ""BlobEndpoint=https://unit-test.blob.core.windows.net/;Foo=Bar"",
    ""containerName"": ""mycontainer"",
    ""containerUri"": ""https://unit-test.blob.core.windows.net/mycontainer"",
    ""secret"": {
      ""name"": ""foo"",
      ""version"": ""1""
    }
  }
}";

        ExportDataOptions<ExportDestinationType> actual = JsonSerializer.Deserialize<ExportDataOptions<ExportDestinationType>>(json, _serializerOptions);
        Assert.Equal(ExportDestinationType.AzureBlob, actual.Type);

        var options = actual.Settings as AzureBlobExportOptions;
        Assert.Equal("BlobEndpoint=https://unit-test.blob.core.windows.net/;Foo=Bar", options.ConnectionString);
        Assert.Equal("mycontainer", options.ContainerName);
        Assert.Equal(new Uri("https://unit-test.blob.core.windows.net/mycontainer"), options.ContainerUri);
        Assert.Equal("foo", options.Secret.Name);
        Assert.Equal("1", options.Secret.Version);
    }

    [Fact]
    public void GivenSourceOptions_WhenWriting_ThenSerialize()
    {
        var expected = new ExportDataOptions<ExportSourceType>(
            ExportSourceType.Identifiers,
            new IdentifierExportOptions
            {
                Values = new DicomIdentifier[]
                {
                    DicomIdentifier.ForStudy("1.2"),
                    DicomIdentifier.ForSeries("3.4.5", "67"),
                    DicomIdentifier.ForInstance("8.9.10", "1.112.13", "1.4"),
                }
            });

        string actual = JsonSerializer.Serialize(expected, _serializerOptions);
        Assert.Equal(
@"{
  ""type"": ""identifiers"",
  ""settings"": {
    ""values"": [
      ""1.2"",
      ""3.4.5/67"",
      ""8.9.10/1.112.13/1.4""
    ]
  }
}",
            actual);
    }

    [Fact]
    public void GivenDestinationOptions_WhenWriting_ThenSerialize()
    {
        var expected = new ExportDataOptions<ExportDestinationType>(
            ExportDestinationType.AzureBlob,
            new AzureBlobExportOptions
            {
                ConnectionString = "BlobEndpoint=https://unit-test.blob.core.windows.net/;Foo=Bar",
                ContainerName = "mycontainer",
                ContainerUri = new Uri("https://unit-test.blob.core.windows.net/mycontainer"),
                Secret = new SecretKey { Name = "foo", Version = "1" },
            });

        string actual = JsonSerializer.Serialize(expected, _serializerOptions);
        Assert.Equal(
@"{
  ""type"": ""azureBlob"",
  ""settings"": {
    ""containerUri"": ""https://unit-test.blob.core.windows.net/mycontainer"",
    ""connectionString"": ""BlobEndpoint=https://unit-test.blob.core.windows.net/;Foo=Bar"",
    ""containerName"": ""mycontainer"",
    ""secret"": {
      ""name"": ""foo"",
      ""version"": ""1""
    }
  }
}",
            actual);
    }
}
