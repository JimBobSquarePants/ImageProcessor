// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace ImageProcessor.Formats
{
    /// <summary>
    /// Provides the necessary information to support bitmap images.
    /// </summary>
    public class BitmapFormat : FormatBase
    {
        /// <inheritdoc/>
        public override byte[][] FileHeaders { get; } = new[]
        {
            Encoding.ASCII.GetBytes("BM")
        };

        /// <inheritdoc/>
        public override string[] FileExtensions { get; } = new[]
        {
            "bmp"
        };

        /// <inheritdoc/>
        public override string MimeType { get; } = "image/bmp";

        /// <inheritdoc/>
        public override ImageFormat ImageFormat { get; } = ImageFormat.Bmp;

        /// <inheritdoc/>
        public override void Save(Stream stream, Image image, BitDepth bitDepth, long quality)
        {
            switch (bitDepth)
            {
                case BitDepth.Bit1:
                case BitDepth.Bit4:
                case BitDepth.Bit8:

                    // Save as 8 bit quantized image.
                    // TODO: Consider allowing 1 and 4 bit quantization.
                    using (Bitmap quantized = this.Quantizer.Quantize(image))
                    {
                        this.CopyMetadata(image, quantized);
                        base.Save(stream, quantized, bitDepth, quality);
                    }

                    break;

                default:

                    PixelFormat pixelFormat = FormatUtilities.GetPixelFormatForBitDepth(bitDepth);

                    if (pixelFormat != image.PixelFormat)
                    {
                        using (Image copy = this.DeepClone(image, pixelFormat, FrameProcessingMode.All, true))
                        {
                            base.Save(stream, copy, bitDepth, quality);
                        }
                    }
                    else
                    {
                        base.Save(stream, image, bitDepth, quality);
                    }

                    break;
            }
        }
    }
}
