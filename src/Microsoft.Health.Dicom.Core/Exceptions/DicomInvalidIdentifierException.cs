﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Dicom.Core.Exceptions
{
    public class DicomInvalidIdentifierException : DicomServerException
    {
        public DicomInvalidIdentifierException(string value, string name)
            : base(string.Format(DicomCoreResource.DicomIdentifierInvalid, name, value))
        {
        }
    }
}
