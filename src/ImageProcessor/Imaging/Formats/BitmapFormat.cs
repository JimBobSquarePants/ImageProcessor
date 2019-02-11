// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BitmapFormat.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides the necessary information to support bitmap images.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Formats
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Text;
    using ImageProcessor.Common.Extensions;

    /// <summary>
    /// Provides the necessary information to support bitmap images.
    /// </summary>
    public class BitmapFormat : FormatBase
    {
        /// <summary>
        /// Gets the file headers.
        /// </summary>
        public override byte[][] FileHeaders => new[]
        {
            Encoding.ASCII.GetBytes("BM")
        };

        /// <summary>
        /// Gets the list of file extensions.
        /// </summary>
        public override string[] FileExtensions => new[]
        {
            "bmp"
        };

        /// <summary>
        /// Gets the standard identifier used on the Internet to indicate the type of data that a file contains.
        /// </summary>
        public override string MimeType => "image/bmp";

        /// <summary>
        /// Gets the <see cref="ImageFormat" />.
        /// </summary>
        public override ImageFormat ImageFormat => ImageFormat.Bmp;

        /// <inheritdoc/>
        public override Image Save(Stream stream, Image image, long bitDepth)
        {
            PixelFormat pixelFormat = PixelFormat.Format32bppPArgb;
            switch (bitDepth)
            {
                case 24L:
                    pixelFormat = PixelFormat.Format24bppRgb;
                    break;

                case 8L:
                    pixelFormat = PixelFormat.Format8bppIndexed;
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
            // Bmps can be saved with different bit depths.
            PixelFormat pixelFormat = PixelFormat.Format32bppPArgb;
            switch (bitDepth)
            {
                case 24L:
                    pixelFormat = PixelFormat.Format24bppRgb;
                    break;

                case 8L:
                    pixelFormat = PixelFormat.Format8bppIndexed;
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