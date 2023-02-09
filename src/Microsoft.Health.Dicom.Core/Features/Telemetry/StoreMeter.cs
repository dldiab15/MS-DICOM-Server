// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics.Metrics;

namespace Microsoft.Health.Dicom.Core.Features.Telemetry;

public sealed class StoreMeter : IDisposable
{
    private readonly Meter _meter;

    public StoreMeter()
    {
        _meter = new Meter($"{OpenTelemetryLabels.BaseMeterName}.Store", "1.0");
        IndexTagValidationError = _meter.CreateCounter<int>(nameof(IndexTagValidationError), description: "Index tag validation error");
        DroppedInvalidTag = _meter.CreateCounter<int>(nameof(DroppedInvalidTag), description: "Drop invalid dicom json");
    }

    public Counter<int> IndexTagValidationError { get; }

    public Counter<int> DroppedInvalidTag { get; }

    public void Dispose()
        => _meter.Dispose();
}
