﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Globalization;
using Dicom;
using EnsureThat;
using Microsoft.Health.Dicom.Core.Exceptions;
using Microsoft.Health.Dicom.Core.Extensions;

namespace Microsoft.Health.Dicom.Core.Features.Validation
{
    internal class DicomElementMaxLengthValidation : DicomElementValidation
    {
        public DicomElementMaxLengthValidation(int maxLength)
        {
            MaxLength = maxLength;
        }

        public int MaxLength { get; }

        public override void Validate(DicomElement dicomElement)
        {
            base.Validate(dicomElement);

            string value = dicomElement.Get<string>();
            Validate(value, MaxLength, dicomElement.Tag.GetFriendlyName(), dicomElement.ValueRepresentation);
        }

        public static void Validate(string value, int maxLength, string name, DicomVR vr)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            EnsureArg.IsNotNull(vr, nameof(vr));
            if (value?.Length > maxLength)
            {
                throw new DicomElementValidationException(
                    ValidationErrorCode.ValueExceedsMaxLength,
                    name,
                    vr,
                    string.Format(CultureInfo.InvariantCulture, DicomCoreResource.ValueLengthExceedsMaxLength, maxLength),
                    value);
            }
        }
    }
}
