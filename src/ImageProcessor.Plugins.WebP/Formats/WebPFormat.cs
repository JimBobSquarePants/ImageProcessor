// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using ImageProcessor.Quantizers;

namespace ImageProcessor.Formats
{
    /// <summary>
    /// Provides the necessary information to support webp images.
    /// Adapted from <see href="http://groups.google.com/a/webmproject.org/forum/#!topic/webp-discuss/1coeidT0rQU"/>
    /// by Jose M. Piñeiro.
    /// </summary>
    public sealed class WebPFormat : FormatBase
    {
        private static readonly ImageFormat WebP = new ImageFormat(new Guid("{2500426f-ed67-4a31-8523-e304537dd9a7}"));

        /// <inheritdoc/>
        public override byte[][] FileHeaders { get; } = new[] { Encoding.ASCII.GetBytes("RIFF") };

        /// <inheritdoc/>
        public override string[] FileExtensions { get; } = new[] { "webp" };

        /// <inheritdoc/>
        public override string MimeType { get; } = "image/webp";

        /// <inheritdoc/>
        public override ImageFormat ImageFormat { get; } = WebP;

        /// <inheritdoc/>
        public override IQuantizer Quantizer { get; } = new OctreeQuantizer();

        /// <inheritdoc/>
        public override Image Load(Stream stream)
        {
            byte[] bytes = null;
            int length = (int)stream.Length;
            try
            {
                bytes = ArrayPool<byte>.Shared.Rent(length);
                stream.Read(bytes, 0, length);
                return Decode(bytes, length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
        }

        /// <inheritdoc/>
        public override void Save(Stream stream, Image image, BitDepth bitDepth, long quality)
        {
            // Only 24 and 32 bit supported.
            if (bitDepth != BitDepth.Bit32)
            {
                bitDepth = BitDepth.Bit24;
            }

            PixelFormat pixelFormat = FormatUtilities.GetPixelFormatForBitDepth(bitDepth);

            if (pixelFormat != image.PixelFormat)
            {
                using (Image copy = this.DeepClone(image, pixelFormat, FrameProcessingMode.All, true))
                {
                    Encode(stream, copy, bitDepth, pixelFormat, quality);
                }
            }
            else
            {
                Encode(stream, image, bitDepth, pixelFormat, quality);
            }
        }

        private static Bitmap Decode(byte[] webpData, int length)
        {
            // Get the image width and height
            var pinnedWebP = GCHandle.Alloc(webpData, GCHandleType.Pinned);
            IntPtr ptrData = pinnedWebP.AddrOfPinnedObject();
            uint dataSize = (uint)length;

            Bitmap bitmap = null;
            BitmapData bitmapData = null;
            IntPtr outputBuffer = IntPtr.Zero;

            if (NativeMethods.WebPGetInfo(ptrData, dataSize, out int width, out int height) != 1)
            {
                throw new ImageFormatException("WebP image header is corrupted.");
            }

            byte[] buffer = null;
            try
            {
                // Create a BitmapData and Lock all pixels to be written
                bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

                // Allocate memory for uncompress image
                int outputBufferSize = bitmapData.Stride * height;
                outputBuffer = Marshal.AllocHGlobal(outputBufferSize);

                // Uncompress the image
                outputBuffer = NativeMethods.WebPDecodeBGRAInto(ptrData, dataSize, outputBuffer, outputBufferSize, bitmapData.Stride);

                // Write image to bitmap using Marshal
                buffer = ArrayPool<byte>.Shared.Rent(outputBufferSize);
                Marshal.Copy(outputBuffer, buffer, 0, outputBufferSize);
                Marshal.Copy(buffer, 0, bitmapData.Scan0, outputBufferSize);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);

                // Unlock the pixels
                bitmap?.UnlockBits(bitmapData);

                // Free memory
                pinnedWebP.Free();
                Marshal.FreeHGlobal(outputBuffer);
            }

            return bitmap;
        }

        private static void Encode(Stream stream, Image image, BitDepth bitDepth, PixelFormat pixelFormat, long quality)
        {
            if (quality == 100L
                ? EncodeLosslessly((Bitmap)image, bitDepth, pixelFormat, out byte[] bytes)
                : EncodeLossly((Bitmap)image, bitDepth, pixelFormat, quality, out bytes))
            {
                using (var memoryStream = new MemoryStream(bytes))
                {
                    memoryStream.CopyTo(stream);
                }
            }
            else
            {
                throw new ImageFormatException("Unable to encode WebP image.");
            }
        }

        private static bool EncodeLossly(Bitmap bitmap, BitDepth bitDepth, PixelFormat pixelFormat, long quality, out byte[] webpData)
        {
            webpData = null;
            bool is32Bit = bitDepth == BitDepth.Bit32;
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, pixelFormat);
            IntPtr unmanagedData = IntPtr.Zero;
            bool encoded;

            try
            {
                // Attempt to lossy encode the image.
                int size = is32Bit
                    ? NativeMethods.WebPEncodeBGRA(bmpData.Scan0, bitmap.Width, bitmap.Height, bmpData.Stride, quality, out unmanagedData)
                    : NativeMethods.WebPEncodeBGR(bmpData.Scan0, bitmap.Width, bitmap.Height, bmpData.Stride, quality, out unmanagedData);

                // Copy image compress data to output array
                webpData = new byte[size];
                Marshal.Copy(unmanagedData, webpData, 0, size);
                encoded = true;
            }
            catch
            {
                encoded = false;
            }
            finally
            {
                // Unlock the pixels
                bitmap.UnlockBits(bmpData);

                // Free memory
                NativeMethods.WebPFree(unmanagedData);
            }

            return encoded;
        }

        private static bool EncodeLosslessly(Bitmap bitmap, BitDepth bitDepth, PixelFormat pixelFormat, out byte[] webpData)
        {
            webpData = null;
            bool is32Bit = bitDepth == BitDepth.Bit32;

            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, pixelFormat);
            IntPtr unmanagedData = IntPtr.Zero;
            bool encoded;

            try
            {
                // Attempt to losslessly encode the image.
                int size = is32Bit
                    ? NativeMethods.WebPEncodeLosslessBGRA(bmpData.Scan0, bitmap.Width, bitmap.Height, bmpData.Stride, out unmanagedData)
                    : NativeMethods.WebPEncodeLosslessBGR(bmpData.Scan0, bitmap.Width, bitmap.Height, bmpData.Stride, out unmanagedData);

                // Copy image compress data to output array
                webpData = new byte[size];
                Marshal.Copy(unmanagedData, webpData, 0, size);
                encoded = true;
            }
            catch
            {
                encoded = false;
            }
            finally
            {
                // Unlock the pixels
                bitmap.UnlockBits(bmpData);

                // Free memory
                NativeMethods.WebPFree(unmanagedData);
            }

            return encoded;
        }
    }
}
