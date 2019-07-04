// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PngFormat.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides the necessary information to support png images.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Formats
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using ImageProcessor.Common.Extensions;
    using ImageProcessor.Imaging.Quantizers;
    using ImageProcessor.Imaging.Quantizers.WuQuantizer;

    /// <summary>
    /// Provides the necessary information to support png images.
    /// </summary>
    public class PngFormat : FormatBase, IQuantizableImageFormat
    {
        /// <inheritdoc/>
        public override byte[][] FileHeaders => new[]
        {
            new byte[] { 137, 80, 78, 71 }
        };

        /// <inheritdoc/>
        public override string[] FileExtensions => new[]
        {
            "png"
        };

        /// <inheritdoc/>
        public override string MimeType => "image/png";

        /// <inheritdoc/>
        public override ImageFormat ImageFormat => ImageFormat.Png;

        /// <inheritdoc/>
        public IQuantizer Quantizer { get; set; } = new WuQuantizer();

        /// <inheritdoc/>
        public override Image Save(Stream stream, Image image, BitDepth bitDepth)
        {
            switch (bitDepth)
            {
                case BitDepth.Bit1:
                case BitDepth.Bit2:
                case BitDepth.Bit4:
                case BitDepth.Bit8:

                    // Save as 8 bit quantized image.
                    image = this.Quantizer.Quantize(image);
                    return base.Save(stream, image, bitDepth);

                default:

                    // Use 24 or 32 bit.
                    var pixelFormat = bitDepth != BitDepth.Bit32 ? PixelFormat.Format24bppRgb : PixelFormat.Format32bppPArgb;
                    if (pixelFormat != image.PixelFormat)
                    {
                        using (Image clone = image.Copy(PixelFormat.Format24bppRgb))
                        {
                            clone.Save(stream, this.ImageFormat);
                            return image;
                        }
                    }
                    else
                    {
                        return base.Save(stream, image, bitDepth);
                    }
            }
        }
    }
}