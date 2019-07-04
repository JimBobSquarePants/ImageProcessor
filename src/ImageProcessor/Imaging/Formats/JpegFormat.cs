// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JpegFormat.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides the necessary information to support jpeg images.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Formats
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Provides the necessary information to support jpeg images.
    /// </summary>
    public sealed class JpegFormat : FormatBase
    {
        private ImageCodecInfo imageCodecInfo;

        /// <inheritdoc/>
        public override byte[][] FileHeaders => new[]
        {
            new byte[] { 255, 216, 255 },
            Encoding.ASCII.GetBytes("ÿØÿà..JFIF"),
            Encoding.ASCII.GetBytes("ÿØÿà..EXIF")
        };

        /// <inheritdoc/>
        public override string[] FileExtensions => new[]
        {
            "jpeg", "jpg"
        };

        /// <inheritdoc/>
        public override string MimeType => "image/jpeg";

        /// <inheritdoc/>
        public override ImageFormat ImageFormat => ImageFormat.Jpeg;

        /// <inheritdoc/>
        public override Image Save(Stream stream, Image image, BitDepth bitDepth)
        {
            // Jpegs can be saved with different settings to include a quality setting for the JPEG compression.
            // This improves output compression and quality.
            using (EncoderParameters encoderParameters = FormatUtilities.GetEncodingParameters(this.Quality))
            {
                image.Save(stream, GetCodecInfo(), encoderParameters);
            }

            return image;
        }

        private ImageCodecInfo GetCodecInfo()
        {
            return this.imageCodecInfo ?? (this.imageCodecInfo = Array.Find(
                ImageCodecInfo.GetImageEncoders(),
                ici => ici.MimeType.Equals(this.MimeType, StringComparison.OrdinalIgnoreCase)));
        }
    }
}