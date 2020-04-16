﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Dicom.Core.Exceptions
{
    public class DicomFrameNotFoundException : DicomResourceNotFoundException
    {
        public DicomFrameNotFoundException()
            : base(DicomCoreResource.FrameNotFound)
        {
        }

        public DicomFrameNotFoundException(Exception innerException)
            : base(DicomCoreResource.FrameNotFound, innerException)
        {
        }
    }
}
