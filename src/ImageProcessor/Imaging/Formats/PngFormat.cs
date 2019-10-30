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
        /// <summary>
        /// Gets the file headers.
        /// </summary>
        public override byte[][] FileHeaders => new[]
        {
            new byte[] { 137, 80, 78, 71 }
        };

        /// <summary>
        /// Gets the list of file extensions.
        /// </summary>
        public override string[] FileExtensions => new[]
        {
            "png"
        };

        /// <summary>
        /// Gets the standard identifier used on the Internet to indicate the type of data that a file contains.
        /// </summary>
        public override string MimeType => "image/png";

        /// <summary>
        /// Gets the <see cref="ImageFormat" />.
        /// </summary>
        public override ImageFormat ImageFormat => ImageFormat.Png;

        /// <summary>
        /// Gets or sets the quantizer for reducing the image palette.
        /// </summary>
        public IQuantizer Quantizer { get; set; } = new WuQuantizer();

        /// <inheritdoc/>
        public override Image Save(Stream stream, Image image, long bitDepth)
        {
            if (this.IsIndexed)
            {
                image = this.Quantizer.Quantize(image);

                return base.Save(stream, image, bitDepth);
            }

            PixelFormat pixelFormat = PixelFormat.Format32bppPArgb;
            switch (bitDepth)
            {
                case 24L:
                    pixelFormat = PixelFormat.Format24bppRgb;
                    break;
            }

            using (Image clone = image.Copy(pixelFormat))
            {
                clone.Save(stream, this.ImageFormat);
            }

            return image;
        }

        /// <inheritdoc/>
        public override Image Save(string path, Image image, long bitDepth)
        {
            if (this.IsIndexed)
            {
                image = this.Quantizer.Quantize(image);
                return base.Save(path, image, bitDepth);
            }

            PixelFormat pixelFormat = PixelFormat.Format32bppPArgb;
            switch (bitDepth)
            {
                case 24L:
                    pixelFormat = PixelFormat.Format24bppRgb;
                    break;
            }

            using (Image clone = image.Copy(pixelFormat))
            {
                clone.Save(path, this.ImageFormat);
            }

            return image;
        }
    }
}