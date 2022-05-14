﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Health.Dicom.Client.Models;

namespace Microsoft.Health.Dicom.Client;

public partial interface IDicomWebClient
{
    Task<DicomWebResponse<DicomOperationReference>> StartExportAsync(
        ExportSource source,
        ExportDestination destination,
        string partitionName = default,
        CancellationToken cancellationToken = default);
}
