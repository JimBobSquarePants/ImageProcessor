// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers;
using System.IO;

namespace ImageProcessor
{
    internal static class StreamExtensions
    {
#if !SUPPORTS_SPAN_STREAM
        // This is a port of the CoreFX implementation and is MIT Licensed:
        // https://github.com/dotnet/corefx/blob/17300169760c61a90cab8d913636c1058a30a8c1/src/Common/src/CoreLib/System/IO/Stream.cs#L742
        public static int Read(this Stream stream, Span<byte> buffer)
        {
            // This uses ArrayPool<byte>.Shared, rather than taking a MemoryAllocator,
            // in order to match the signature of the framework method that exists in
            // .NET Core.
            byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                int numRead = stream.Read(sharedBuffer, 0, buffer.Length);
                if ((uint)numRead > (uint)buffer.Length)
                {
                    throw new IOException("Stream was too long.");
                }

                new Span<byte>(sharedBuffer, 0, numRead).CopyTo(buffer);
                return numRead;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sharedBuffer);
            }
        }

        // This is a port of the CoreFX implementation and is MIT Licensed:
        // https://github.com/dotnet/corefx/blob/17300169760c61a90cab8d913636c1058a30a8c1/src/Common/src/CoreLib/System/IO/Stream.cs#L775
        public static void Write(this Stream stream, ReadOnlySpan<byte> buffer)
        {
            // This uses ArrayPool<byte>.Shared, rather than taking a MemoryAllocator,
            // in order to match the signature of the framework method that exists in
            // .NET Core.
            byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                buffer.CopyTo(sharedBuffer);
                stream.Write(sharedBuffer, 0, buffer.Length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sharedBuffer);
            }
        }
#endif
    }
}
