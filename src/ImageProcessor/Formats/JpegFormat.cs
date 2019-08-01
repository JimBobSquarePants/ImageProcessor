// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Encoder = System.Drawing.Imaging.Encoder;

namespace ImageProcessor.Formats
{
    /// <summary>
    /// Provides the necessary information to support jpeg images.
    /// </summary>
    public sealed class JpegFormat : FormatBase
    {
        private ImageCodecInfo imageCodecInfo;

        /// <inheritdoc/>
        public override byte[][] FileHeaders { get; } = new[]
        {
            new byte[] { 0xFF, 0xD8, 0xFF },
            Encoding.ASCII.GetBytes("ÿØÿà..JFIF"),
            Encoding.ASCII.GetBytes("ÿØÿà..EXIF")
        };

        /// <inheritdoc/>
        public override string[] FileExtensions { get; } = new[]
        {
            "jpg", "jpeg", "jfif"
        };

        /// <inheritdoc/>
        public override string MimeType { get; } = "image/jpeg";

        /// <inheritdoc/>
        public override ImageFormat ImageFormat { get; } = ImageFormat.Jpeg;

        /// <inheritdoc/>
        public override void Save(Stream stream, Image image, BitDepth bitDepth, long quality)
        {
            // Jpegs can be saved with different settings to include a quality setting for the JPEG compression.
            // This improves output compression and quality.
            using (EncoderParameters encoderParameters = GetEncoderParameters(quality))
            {
                image.Save(stream, this.GetCodecInfo(), encoderParameters);
            }
        }

        private static EncoderParameters GetEncoderParameters(long quality)
        {
            return new EncoderParameters(1)
            {
                // Set the quality.
                Param = { [0] = new EncoderParameter(Encoder.Quality, quality) }
            };
        }

        private ImageCodecInfo GetCodecInfo()
        {
            return this.imageCodecInfo ?? (this.imageCodecInfo = Array.Find(
                ImageCodecInfo.GetImageEncoders(),
                ici => ici.MimeType.Equals(this.MimeType, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
