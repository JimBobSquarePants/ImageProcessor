// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ImageProcessor.Quantizers;

namespace ImageProcessor.Formats
{
    /// <summary>
    /// Provides the necessary information to support png images.
    /// </summary>
    public sealed class PngFormat : FormatBase
    {
        private static readonly byte[][] Identifier = new[]
        {
            new byte[] { 0x89, 0x50, 0x4E, 0x47 }
        };

        private static readonly string[] Extensions = new[]
        {
            "png"
        };

        /// <inheritdoc/>
        public override byte[][] FileHeaders => Identifier;

        /// <inheritdoc/>
        public override string[] FileExtensions => Extensions;

        /// <inheritdoc/>
        public override string MimeType => "image/png";

        /// <inheritdoc/>
        public override ImageFormat ImageFormat => ImageFormat.Png;

        /// <inheritdoc/>
        public override IQuantizer Quantizer => new WuQuantizer();

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
                        CopyMetadata(image, quantized);
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
