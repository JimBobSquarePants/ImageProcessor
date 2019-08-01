// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using ImageProcessor.Configuration;
using ImageProcessor.Formats;

namespace ImageProcessor
{
    /// <summary>
    /// Extension methods for the <see cref="Bitmap"/> type.
    /// </summary>
    public static class ImageExtensions
    {
        /// <summary>
        /// Creates a deep copy of the source image.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <returns>The <see cref="Bitmap"/>.</returns>
        public static Bitmap DeepClone(this Image source) => DeepClone(source, source.PixelFormat);

        /// <summary>
        /// Creates a deep copy of the source image.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="targetFormat">The target pixel format.</param>
        /// <returns>The <see cref="Bitmap"/>.</returns>
        public static Bitmap DeepClone(this Image source, PixelFormat targetFormat)
            => DeepClone(source, targetFormat, FrameProcessingMode.All);

        /// <summary>
        /// Creates a deep copy of the source image.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="targetFormat">The target pixel format.</param>
        /// <param name="frameProcessingMode">The frame processing mode.</param>
        /// <returns>The <see cref="Bitmap"/>.</returns>
        public static Bitmap DeepClone(
            this Image source,
            PixelFormat targetFormat,
            FrameProcessingMode frameProcessingMode)
            => DeepClone(source, targetFormat, frameProcessingMode, true);

        /// <summary>
        /// Creates a deep copy of the source image.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="targetFormat">The target pixel format.</param>
        /// <param name="frameProcessingMode">The frame processing mode.</param>
        /// <param name="preserveMetaData">Whether to preserve metadata.</param>
        /// <returns>The <see cref="Bitmap"/>.</returns>
        public static Bitmap DeepClone(
            this Image source,
            PixelFormat targetFormat,
            FrameProcessingMode frameProcessingMode,
            bool preserveMetaData)
        {
            IImageFormat format = ImageProcessorBootstrapper.Instance.ImageFormats
                .FirstOrDefault(x => x.ImageFormat.Equals(source.RawFormat));

            if (format is null)
            {
                format = ImageProcessorBootstrapper.Instance.ImageFormats
                  .First(x => x is BitmapFormat);
            }

            return format.DeepClone(source, targetFormat, frameProcessingMode, preserveMetaData);
        }
    }
}
