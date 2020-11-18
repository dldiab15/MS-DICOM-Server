﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Dicom;
using Hl7.Fhir.Model;

namespace Microsoft.Health.DicomCast.Core.Features.Worker.FhirTransaction
{
    /// <summary>
    /// Provides functionality to synchronize DICOM properties to a specific <see cref="Patient"/> resource property.
    /// </summary>
    public interface IPatientPropertySynchronizer
    {
        /// <summary>
        /// Synchronizes the DICOM properties to <paramref name="patient"/>.
        /// </summary>
        /// <param name="dataset">The DICOM properties.</param>
        /// <param name="patient">The <see cref="Patient"/> resource.</param>
        /// <param name="requestMode">The <see cref="FhirTransactionRequestMode"/> to determine how Fhir is being called.</param>
        void Synchronize(DicomDataset dataset, Patient patient, FhirTransactionRequestMode requestMode);
    }
}
