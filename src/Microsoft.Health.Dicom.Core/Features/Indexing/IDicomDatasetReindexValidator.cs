﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Dicom;
using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;

namespace Microsoft.Health.Dicom.Core.Features.Indexing
{
    public interface IDicomDatasetReindexValidator
    {
        IReadOnlyCollection<QueryTag> Validate(DicomDataset dicomDataset, IReadOnlyCollection<QueryTag> queryTags);
    }
}
