// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Resizer.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides methods to resize images.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging
{
    using ImageProcessor.Common.Exceptions;
    using ImageProcessor.Common.Extensions;
    using ImageProcessor.Imaging.Formats;
    using ImageProcessor.Imaging.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;

    /// <summary>
    /// Provides methods to resize images.
    /// </summary>
    public class Resizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Resizer"/> class.
        /// </summary>
        /// <param name="size">
        /// The <see cref="Size"/> to resize the image to.
        /// </param>
        public Resizer(Size size) => this.ResizeLayer = new ResizeLayer(size);

        /// <summary>
        /// Initializes a new instance of the <see cref="Resizer"/> class.
        /// </summary>
        /// <param name="resizeLayer">
        /// The <see cref="ResizeLayer"/>.
        /// </param>
        public Resizer(ResizeLayer resizeLayer) => this.ResizeLayer = resizeLayer;

        /// <summary>
        /// Gets or sets the <see cref="ResizeLayer"/>.
        /// </summary>
        public ResizeLayer ResizeLayer { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISupportedImageFormat"/>.
        /// </summary>
        public ISupportedImageFormat ImageFormat { get; set; }

        /// <summary>
        /// Gets or sets process mode for frames in animated images.
        /// </summary>
        public AnimationProcessMode AnimationProcessMode { get; set; }

        /// <summary>
        /// Resizes the given image.
        /// </summary>
        /// <param name="source">The source <see cref="Image"/> to resize.</param>
        /// <param name="linear">Whether to resize the image using the linear color space.</param>
        /// <returns>
        /// The resized <see cref="Image"/>.
        /// </returns>
        public Bitmap ResizeImage(
            Image source,
            bool linear)
        {
            Bitmap newImage = null;

            try
            {
                Size sourceSize = source.Size;
                int targetWidth = this.ResizeLayer.Size.Width;
                int targetHeight = this.ResizeLayer.Size.Height;
                int maxWidth = this.ResizeLayer.MaxSize?.Width ?? int.MaxValue;
                int maxHeight = this.ResizeLayer.MaxSize?.Height ?? int.MaxValue;

                // Ensure size is populated across both dimensions.
                // These dimensions are used to calculate the final dimensions determined by the mode algorithm.
                // If only one of the incoming dimensions is 0, it will be modified here to maintain aspect ratio.
                // If it is not possible to keep aspect ratio, make sure at least the minimum is is kept.
                const int Min = 1;
                if (targetWidth == 0 && targetHeight > 0)
                {
                    targetWidth = (int)Math.Max(Min, Math.Round(sourceSize.Width * targetHeight / (float)sourceSize.Height));
                }

                if (targetHeight == 0 && targetWidth > 0)
                {
                    targetHeight = (int)Math.Max(Min, Math.Round(sourceSize.Height * targetWidth / (float)sourceSize.Width));
                }

                (Size size, Rectangle rectangle) = ResizeHelper.CalculateTargetLocationAndBounds(source.Size, this.ResizeLayer, targetWidth, targetHeight);

                int sourceWidth = source.Width;
                int sourceHeight = source.Height;
                int width = size.Width;
                int height = size.Height;
                bool upscale = this.ResizeLayer.Upscale;

                if (ResizeLayer.ResizeMode == ResizeMode.Min)
                {
                    // Ensure we can't upscale.
                    maxHeight = sourceHeight;
                    maxWidth = sourceWidth;
                    upscale = false;
                }

                maxWidth = maxWidth > 0 ? maxWidth : int.MaxValue;
                maxHeight = maxHeight > 0 ? maxHeight : int.MaxValue;
                List<Size> restrictedSizes = this.ResizeLayer.RestrictedSizes;

                // Restrict sizes
                if (restrictedSizes?.Count > 0)
                {
                    bool reject = true;
                    foreach (Size restrictedSize in restrictedSizes)
                    {
                        if (restrictedSize.Height == 0 || restrictedSize.Width == 0)
                        {
                            if (restrictedSize.Width == width || restrictedSize.Height == height)
                            {
                                reject = false;
                            }
                        }
                        else if (restrictedSize.Width == width && restrictedSize.Height == height)
                        {
                            reject = false;
                        }
                    }

                    if (reject)
                    {
                        return (Bitmap)source;
                    }
                }

                if (width > 0 && height > 0 && width <= maxWidth && height <= maxHeight)
                {
                    // Exit if upscaling is not allowed.
                    if ((width > sourceWidth || height > sourceHeight)
                        && !upscale
                        && this.ResizeLayer.ResizeMode != ResizeMode.Stretch)
                    {
                        return (Bitmap)source;
                    }

                    newImage = linear ? this.ResizeLinear(source, width, height, rectangle, this.AnimationProcessMode)
                                      : this.ResizeComposite(source, width, height, rectangle);

                    // Reassign the image.
                    source.Dispose();
                    source = newImage;
                }
            }
            catch (Exception ex)
            {
                newImage?.Dispose();

                throw new ImageProcessingException("Error processing image with " + this.GetType().Name, ex);
            }

            return (Bitmap)source;
        }

        /// <summary>
        /// Gets an image resized using the composite color space without any gamma correction adjustments.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="width">The target width to resize to.</param>
        /// <param name="height">The target height to resize to.</param>
        /// <param name="destination">The destination rectangle.</param>
        /// <returns>
        /// The <see cref="Bitmap"/>.
        /// </returns>
        protected virtual Bitmap ResizeComposite(Image source, int width, int height, Rectangle destination)
        {
            var resized = new Bitmap(width, height, source.PixelFormat);
            resized.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (var graphics = Graphics.FromImage(resized))
            {
                GraphicsHelper.SetGraphicsOptions(graphics);
                using (var attributes = new ImageAttributes())
                {
                    attributes.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(
                        source,
                        destination,
                        0,
                        0,
                        source.Width,
                        source.Height,
                        GraphicsUnit.Pixel,
                        attributes);
                }
            }

            return resized;
        }

        /// <summary>
        /// Gets an image resized using the linear color space with gamma correction adjustments.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="targetWidth">The width to resize to.</param>
        /// <param name="targetHeight">The height to resize to.</param>
        /// <param name="destination">The destination rectangle.</param>
        /// <returns>
        /// The <see cref="Bitmap"/>.
        /// </returns>
        protected virtual Bitmap ResizeLinear(Image source, int targetWidth, int targetHeight, Rectangle destination)
            => this.ResizeLinear(source, targetWidth, targetHeight, destination, this.AnimationProcessMode);

        /// <summary>
        /// Gets an image resized using the linear color space with gamma correction adjustments.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <param name="destination">The destination rectangle.</param>
        /// <param name="animationProcessMode">The process mode for frames in animated images.</param>
        /// <returns>
        /// The <see cref="Bitmap"/>.
        /// </returns>
        protected virtual Bitmap ResizeLinear(
            Image source,
            int width,
            int height,
            Rectangle destination,
            AnimationProcessMode animationProcessMode)
        {
            // Adjust the gamma value so that the image is in the linear color space.
            Bitmap linear = Adjustments.ToLinear(source.Copy(animationProcessMode));

            var resized = new Bitmap(width, height, source.PixelFormat);
            resized.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (var graphics = Graphics.FromImage(resized))
            {
                GraphicsHelper.SetGraphicsOptions(graphics);
                using (var attributes = new ImageAttributes())
                {
                    attributes.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(
                        linear,
                        destination,
                        0,
                        0,
                        source.Width,
                        source.Height,
                        GraphicsUnit.Pixel,
                        attributes);
                }
            }

            // Return to composite color space.
            resized = Adjustments.ToSRGB(resized);

            linear.Dispose();
            return resized;
        }
    }
}
