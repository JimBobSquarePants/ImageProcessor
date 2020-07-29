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
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Text;
    using ImageProcessor.Imaging.MetaData;

    /// <summary>
    /// Provides the necessary information to support jpeg images.
    /// </summary>
    public sealed class JpegFormat : FormatBase
    {
        /// <summary>
        /// Gets the file headers.
        /// </summary>
        public override byte[][] FileHeaders => new[]
        {
            new byte[] { 255, 216, 255 },
            Encoding.ASCII.GetBytes("ÿØÿà..JFIF"),
            Encoding.ASCII.GetBytes("ÿØÿà..EXIF")
        };

        /// <summary>
        /// Gets the list of file extensions.
        /// </summary>
        public override string[] FileExtensions => new[]
        {
            "jpeg", "jpg"
        };

        /// <summary>
        /// Gets the standard identifier used on the Internet to indicate the type of data that a file contains.
        /// </summary>
        public override string MimeType => "image/jpeg";

        /// <summary>
        /// Gets the <see cref="ImageFormat" />.
        /// </summary>
        public override ImageFormat ImageFormat => ImageFormat.Jpeg;

        /// <summary>
        /// Saves the current image to the specified output stream.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="T:System.IO.Stream"/> to save the image information to.
        /// </param>
        /// <param name="image">
        /// The <see cref="T:System.Drawing.Image"/> to save.
        /// </param>
        /// <param name="bitDepth">
        /// The color depth in number of bits per pixel to save the image with.
        /// </param>
        /// <returns>
        /// The <see cref="T:System.Drawing.Image"/>.
        /// </returns>
        public override Image Save(Stream stream, Image image, long bitDepth)
        {
            SantizeMetadata(image);

            // Jpegs can be saved with different settings to include a quality setting for the JPEG compression.
            // This improves output compression and quality.
            using (EncoderParameters encoderParameters = FormatUtilities.GetEncodingParameters(this.Quality))
            {
                ImageCodecInfo imageCodecInfo =
                    Array.Find(ImageCodecInfo.GetImageEncoders(), ici => ici.MimeType.Equals(this.MimeType, StringComparison.OrdinalIgnoreCase));

                if (imageCodecInfo != null)
                {
                    image.Save(stream, imageCodecInfo, encoderParameters);
                }
            }

            return image;
        }

        /// <summary>
        /// Saves the current image to the specified file path.
        /// </summary>
        /// <param name="path">The path to save the image to.</param>
        /// <param name="image">
        /// The <see cref="T:System.Drawing.Image"/> to save.
        /// </param>
        /// <param name="bitDepth">
        /// The color depth in number of bits per pixel to save the image with.
        /// </param>
        /// <returns>
        /// The <see cref="T:System.Drawing.Image"/>.
        /// </returns>
        public override Image Save(string path, Image image, long bitDepth)
        {
            SantizeMetadata(image);

            // Jpegs can be saved with different settings to include a quality setting for the JPEG compression.
            // This improves output compression and quality.
            using (EncoderParameters encoderParameters = FormatUtilities.GetEncodingParameters(this.Quality))
            {
                ImageCodecInfo imageCodecInfo =
                    Array.Find(ImageCodecInfo.GetImageEncoders(), ici => ici.MimeType.Equals(this.MimeType, StringComparison.OrdinalIgnoreCase));

                if (imageCodecInfo != null)
                {
                    image.Save(path, imageCodecInfo, encoderParameters);
                }
            }

            return image;
        }

        // System.Drawing's jpeg encoder throws when proprietary tags are included in the metadata
        // https://github.com/JimBobSquarePants/ImageProcessor/issues/811
        private static void SantizeMetadata(Image image)
        {
            foreach (int id in image.PropertyIdList)
            {
                if (Array.IndexOf(ExifPropertyTagConstants.Ids, id) == -1)
                {
                    image.RemovePropertyItem(id);
                }
            }
        }
    }
}