// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ImageProcessor.Quantizers;

namespace ImageProcessor.Formats
{
    /// <summary>
    /// Provides the necessary information to support tiff images.
    /// </summary>
    public class TiffFormat : FormatBase
    {
        private ImageCodecInfo imageCodecInfo;

        /// <inheritdoc/>
        public override byte[][] FileHeaders { get; } = new[]
        {
            new byte[] { 0x49, 0x49, 0x2A, 0x0 },
            new byte[] { 0x4D, 0x4D, 0x0, 0x2A }
        };

        /// <inheritdoc/>
        public override string[] FileExtensions { get; } = new[]
        {
            "tif", "tiff"
        };

        /// <inheritdoc/>
        public override string MimeType => "image/tiff";

        /// <inheritdoc/>
        public override ImageFormat ImageFormat => ImageFormat.Tiff;

        /// <inheritdoc/>
        public override IQuantizer Quantizer { get; } = new OctreeQuantizer();

        /// <inheritdoc/>
        public override void Save(Stream stream, Image image, BitDepth bitDepth, long quality)
        {
            // Tiffs can be saved with different bit depths but hrows if we use 16 bits.
            if (bitDepth == BitDepth.Bit16)
            {
                bitDepth = BitDepth.Bit24;
            }

            using (EncoderParameters encoderParameters = GetEncoderParameters(bitDepth))
            {
                switch (bitDepth)
                {
                    case BitDepth.Bit4:
                    case BitDepth.Bit8:
                        // Save as 8 bit quantized image.
                        using (Bitmap quantized = this.Quantizer.Quantize(image))
                        {
                            this.CopyMetadata(image, quantized);
                            quantized.Save(stream, this.GetCodecInfo(), encoderParameters);
                        }

                        return;

                    case BitDepth.Bit24:
                    case BitDepth.Bit32:

                        PixelFormat pixelFormat = FormatUtilities.GetPixelFormatForBitDepth(bitDepth);

                        if (pixelFormat != image.PixelFormat)
                        {
                            using (Image copy = this.DeepClone(image, pixelFormat, FrameProcessingMode.All, true))
                            {
                                copy.Save(stream, this.GetCodecInfo(), encoderParameters);
                            }
                        }
                        else
                        {
                            image.Save(stream, this.GetCodecInfo(), encoderParameters);
                        }

                        break;
                    default:

                        // Encoding is handled by the encoding parameters.
                        image.Save(stream, this.GetCodecInfo(), encoderParameters);
                        break;
                }
            }
        }

        private static EncoderParameters GetEncoderParameters(BitDepth bitDepth)
        {
            long colorDepth = (long)bitDepth;

            // CompressionCCITT4 provides 1 bit diffusion.
            long compression = (long)(bitDepth == BitDepth.Bit1
                ? EncoderValue.CompressionCCITT4
                : EncoderValue.CompressionLZW);

            var encoderParameters = new EncoderParameters(2);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Compression, compression);
            encoderParameters.Param[1] = new EncoderParameter(Encoder.ColorDepth, colorDepth);

            return encoderParameters;
        }

        private ImageCodecInfo GetCodecInfo()
        {
            return this.imageCodecInfo ?? (this.imageCodecInfo = Array.Find(
                ImageCodecInfo.GetImageEncoders(),
                ici => ici.MimeType.Equals(this.MimeType, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
