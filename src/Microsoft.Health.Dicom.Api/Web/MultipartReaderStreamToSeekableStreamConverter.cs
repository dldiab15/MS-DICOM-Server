﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.AspNetCore.WebUtilities;

namespace Microsoft.Health.Dicom.Api.Web
{
    /// <summary>
    /// Provides functionality to convert stream into a seekable stream.
    /// </summary>
    internal class MultipartReaderStreamToSeekableStreamConverter : ISeekableStreamConverter
    {
        private const int DefaultBufferThreshold = 1024 * 30; // 30KB

        /// <inheritdoc />
        public async Task<Stream> ConvertAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(stream);

            int bufferThreshold = DefaultBufferThreshold;
            long? bufferLimit = null;
            Stream seekableStream = null;

            if (!stream.CanSeek)
            {
                try
                {
                    seekableStream = new FileBufferingReadStream(stream, bufferThreshold, bufferLimit, AspNetCoreTempDirectory.TempDirectoryFactory);
                    await seekableStream.DrainAsync(cancellationToken);
                }
                catch (InvalidDataException)
                {
                    // This will result in bad request, we need to handle this differently when we make the processing serial.
                    throw new DicomFileLengthLimitExceededException(AspNetCoreMultipartReader.DicomFileSizeLimit);
                }
                catch (IOException ex)
                {
                    throw new InvalidMultipartBodyPartException(ex);
                }
                catch (Exception)
                {
                    if (seekableStream != null)
                    {
                        await seekableStream.DisposeAsync();
                    }

                    throw;
                }
            }
            else
            {
                seekableStream = stream;
            }

            seekableStream.Seek(0, SeekOrigin.Begin);
            return seekableStream;
        }

        private static class AspNetCoreTempDirectory
        {
            private static string _tempDirectory;

            public static string TempDirectory
            {
                get
                {
                    if (_tempDirectory == null)
                    {
                        // Look for folders in the following order.
                        var temp = Environment.GetEnvironmentVariable("ASPNETCORE_TEMP") ?? // ASPNETCORE_TEMP - User set temporary location.
                                   Path.GetTempPath();                                      // Fall back.

                        if (!Directory.Exists(temp))
                        {
                            throw new DirectoryNotFoundException(temp);
                        }

                        _tempDirectory = temp;
                    }

                    return _tempDirectory;
                }
            }

            public static Func<string> TempDirectoryFactory => () => TempDirectory;
        }
    }
}
