﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Dicom;
using Microsoft.Health.Dicom.Core.Exceptions;
using Microsoft.Health.Dicom.Core.Features.Validation;
using Xunit;

namespace Microsoft.Health.Dicom.Core.UnitTests.Features.Validation
{
    public class DicomUidValidationTests
    {

        [Fact]
        public void GivenValidateUid_WhenValidating_ThenShouldPass()
        {
            DicomElement element = new DicomUniqueIdentifier(DicomTag.DigitalSignatureUID, "13.14.520");
            new DicomUidValidation().Validate(element);
        }

        [Theory]
        [InlineData("abc.123")] // a is invalid character
        [InlineData("11|")] // | is invalid character
        [InlineData("0123456789012345678901234567890123456789012345678901234567890123456789")] // value is too long
        public void GivenInvalidUidWhenValidating_ThenShouldThrow(string value)
        {
            DicomElement element = new DicomUniqueIdentifier(DicomTag.DigitalSignatureUID, value);
            Assert.Throws<InvalidIdentifierException>(() => new DicomUidValidation().Validate(element));

        }

    }
}
