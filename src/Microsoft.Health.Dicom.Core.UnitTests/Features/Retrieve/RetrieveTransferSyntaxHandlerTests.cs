// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Health.Dicom.Core.Exceptions;
using Microsoft.Health.Dicom.Core.Features.Retrieve;
using Microsoft.Health.Dicom.Core.Messages;
using Microsoft.Health.Dicom.Core.Messages.Retrieve;
using Xunit;

namespace Microsoft.Health.Dicom.Core.UnitTests.Features.Retrieve;

public class RetrieveTransferSyntaxHandlerTests
{
    private readonly RetrieveTransferSyntaxHandler _handler;
    private static readonly AcceptHeaderDescriptor ValidStudyAcceptHeaderDescriptor = RetrieveTransferSyntaxHandler
        .AcceptableDescriptors[ResourceType.Study]
        .Descriptors
        .First();

    public static IEnumerable<object[]> UnacceptableHeadersList()
    {
        yield return new object[]
        {
            new List<AcceptHeader>
            {
                new(
                    "unsupportedMediaType",
                    ValidStudyAcceptHeaderDescriptor.PayloadType,
                    ValidStudyAcceptHeaderDescriptor.TransferSyntaxWhenMissing)
            },
            ResourceType.Study
        };
        yield return new object[]
        {
            new List<AcceptHeader>
            {
                new(
                    ValidStudyAcceptHeaderDescriptor.MediaType,
                    ValidStudyAcceptHeaderDescriptor.PayloadType,
                    "unsupportedTransferSyntax")
            },
            ResourceType.Study
        };
        yield return new object[]
        {
            new List<AcceptHeader>
            {
                new(
                    "unsupportedMediaType",
                    ValidStudyAcceptHeaderDescriptor.PayloadType,
                    ValidStudyAcceptHeaderDescriptor.TransferSyntaxWhenMissing),
                new(
                    ValidStudyAcceptHeaderDescriptor.MediaType,
                    ValidStudyAcceptHeaderDescriptor.PayloadType,
                    "unsupportedTransferSyntax")
            },
            ResourceType.Study
        };
    }

    public RetrieveTransferSyntaxHandlerTests()
    {
        _handler = new RetrieveTransferSyntaxHandler(NullLogger<RetrieveTransferSyntaxHandler>.Instance);
    }

    [Fact]
    public void GivenASingleRequestedAcceptHeader_WhenRequestedMatchesHeadersWeAccept_ThenShouldReturnAcceptedHeaderWithTransferSyntaxAndDescriptorThatMatched()
    {
        AcceptHeader requestedAcceptHeader = new AcceptHeader(
            ValidStudyAcceptHeaderDescriptor.MediaType,
            ValidStudyAcceptHeaderDescriptor.PayloadType,
            ValidStudyAcceptHeaderDescriptor.TransferSyntaxWhenMissing
            );

        string returnedTransferSyntax = _handler.GetTransferSyntax(
            ResourceType.Study,
            new[] { requestedAcceptHeader },
            out AcceptHeaderDescriptor matchedAcceptHeaderDescriptor,
            out AcceptHeader matchedAcceptHeader);

        Assert.Equal(requestedAcceptHeader.TransferSyntax, returnedTransferSyntax);
        Assert.Equal(ValidStudyAcceptHeaderDescriptor, matchedAcceptHeaderDescriptor);
        Assert.Equal(requestedAcceptHeader, matchedAcceptHeader);
    }

    [Theory]
    [MemberData(nameof(UnacceptableHeadersList))]
    public void GivenNoMatchedAcceptHeaders_WhenGetTransferSyntax_ThenShouldThrowNotAcceptableException(
        List<AcceptHeader> requestedAcceptHeaders,
        ResourceType requestedResourceType)
    {
        Assert.ThrowsAny<NotAcceptableException>(() => _handler.GetTransferSyntax(
            requestedResourceType,
            requestedAcceptHeaders,
            out AcceptHeaderDescriptor selectedAcceptHeaderDescriptor,
            out AcceptHeader selectedAcceptedHeader));
    }

    [Fact]
    public void GivenMultipleMatchedAcceptHeadersWithDifferentQuality_WhenHeadersRequestedAreAllSupported_ThenShouldReturnHighestQuality()
    {
        Assert.True(ValidStudyAcceptHeaderDescriptor.AcceptableTransferSyntaxes.Count > 1);

        AcceptHeader requestedAcceptHeader1 = new AcceptHeader(
            ValidStudyAcceptHeaderDescriptor.MediaType,
            ValidStudyAcceptHeaderDescriptor.PayloadType,
            ValidStudyAcceptHeaderDescriptor.AcceptableTransferSyntaxes.First(),
            quality: 0.5
        );

        AcceptHeader requestedAcceptHeader2 = new AcceptHeader(
            ValidStudyAcceptHeaderDescriptor.MediaType,
            ValidStudyAcceptHeaderDescriptor.PayloadType,
            ValidStudyAcceptHeaderDescriptor.AcceptableTransferSyntaxes.Last(),
            quality: 0.9
        );

        string returnedTransferSyntax = _handler.GetTransferSyntax(
            ResourceType.Study,
            new[]
            {
                requestedAcceptHeader1,
                requestedAcceptHeader2
            },
            out AcceptHeaderDescriptor matchedAcceptHeaderDescriptor,
            out AcceptHeader matchedAcceptHeader);

        Assert.Equal(requestedAcceptHeader2.TransferSyntax, returnedTransferSyntax);
        Assert.Equal(requestedAcceptHeader2, matchedAcceptHeader);
        Assert.Equal(ValidStudyAcceptHeaderDescriptor, matchedAcceptHeaderDescriptor);
    }

    [Fact]
    public void GivenMultipleMatchedAcceptHeadersWithDifferentQuality_WhenTransferSyntaxRequestedOfHigherQualityNotSupported_ThenShouldReturnNextHighestQuality()
    {
        // When we multiple headers requested, but the one with highest quality "preference"
        // is requested with a TransferSyntax that we do not support,
        // we should use the next highest quality requested with a supported TransferSyntax.

        Assert.True(ValidStudyAcceptHeaderDescriptor.AcceptableTransferSyntaxes.Count > 1);

        AcceptHeader requestedAcceptHeader1 = new AcceptHeader(
            ValidStudyAcceptHeaderDescriptor.MediaType,
            ValidStudyAcceptHeaderDescriptor.PayloadType,
            ValidStudyAcceptHeaderDescriptor.AcceptableTransferSyntaxes.First(),
            quality: 0.3
        );

        AcceptHeader requestedAcceptHeader2 = new AcceptHeader(
            ValidStudyAcceptHeaderDescriptor.MediaType,
            ValidStudyAcceptHeaderDescriptor.PayloadType,
            ValidStudyAcceptHeaderDescriptor.AcceptableTransferSyntaxes.Last(),
            quality: 0.5
        );

        AcceptHeader requestedAcceptHeader3 = new AcceptHeader(
            ValidStudyAcceptHeaderDescriptor.MediaType,
            ValidStudyAcceptHeaderDescriptor.PayloadType,
            "unsupportedTransferSyntax",
            quality: 0.7
        );

        string transferSyntax = _handler
            .GetTransferSyntax(
                ResourceType.Study,
                new[]
                {
                    requestedAcceptHeader1,
                    requestedAcceptHeader2,
                    requestedAcceptHeader3
                },
                out AcceptHeaderDescriptor matchedAcceptHeaderDescriptor,
                out AcceptHeader matchedAcceptHeader);

        Assert.Equal(requestedAcceptHeader2.TransferSyntax, transferSyntax);
        Assert.Equal(requestedAcceptHeader2.Quality, matchedAcceptHeader.Quality);
    }
}
