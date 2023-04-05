// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FellowOakDicom;

namespace Microsoft.Health.Dicom.Client;

public partial interface IDicomWebClient
{
    Task<DicomWebResponse> UpdateStudyAsync(IReadOnlyList<string> studyInstanceUids, DicomDataset dataset, int partitionKey = default, CancellationToken cancellationToken = default);
}
