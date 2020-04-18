// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Drawing;
using ImageProcessor.Processing;

namespace ImageProcessor
{
    /// <content>
    /// Contains processing methods.
    /// </content>
    public partial class ImageFactory : IDisposable
    {
        /// <summary>
        /// Changes the opacity of the current image.
        /// </summary>
        /// <param name="percentage">
        /// The percentage by which to alter the images opacity. Range 0..100.
        /// </param>
        /// <returns>The <see cref="ImageFactory"/>.</returns>
        public ImageFactory Alpha(float percentage)
        {
            this.CheckLoaded();

            var processor = new Alpha(percentage);
            this.ApplyProcessor(processor);

            return this;
        }

        /// <summary>
        /// Performs auto-rotation to ensure that EXIF defined rotation is reflected in
        /// the final image.
        /// </summary>
        /// <returns>The <see cref="ImageFactory"/>.</returns>
        public ImageFactory AutoRotate()
        {
            this.CheckLoaded();

            var processor = new AutoRotate();
            this.ApplyProcessor(processor);

            return this;
        }

        /// <summary>
        /// Changes the background color of the current image.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to paint the image with.</param>
        /// <returns>The <see cref="ImageFactory"/>.</returns>
        public ImageFactory BackgroundColor(Color color)
        {
            this.CheckLoaded();

            var processor = new BackgroundColor(color);
            this.ApplyProcessor(processor);

            return this;
        }

        /// <summary>
        /// Changes the brightness of the current image.
        /// </summary>
        /// <param name="percentage">
        /// The percentage by which to alter the images brightness. Range -100..100.
        /// </param>
        /// <returns>The <see cref="ImageFactory"/>.</returns>
        public ImageFactory Brightness(float percentage)
        {
            this.CheckLoaded();

            var processor = new Brightness(percentage);
            this.ApplyProcessor(processor);

            return this;
        }

        /// <summary>
        /// Changes the contrast of the current image.
        /// </summary>
        /// <param name="percentage">
        /// The percentage by which to alter the images contrast. Range -100..100.
        /// </param>
        /// <returns>The <see cref="ImageFactory"/>.</returns>
        public ImageFactory Contrast(float percentage)
        {
            this.CheckLoaded();

            var processor = new Contrast(percentage);
            this.ApplyProcessor(processor);

            return this;
        }

        /// <summary>
        /// Crops the current image to the given location and size.
        /// </summary>
        /// <param name="bounds">The rectangle containing the coordinates to crop the image to.</param>
        /// <returns>The <see cref="ImageFactory"/>.</returns>
        public ImageFactory Crop(Rectangle bounds)
        {
            var options = new CropOptions(bounds.X, bounds.Y, bounds.Width, bounds.Height, CropMode.Pixels);
            return this.Crop(options);
        }

        /// <summary>
        /// Crops the current image to the given location and size.
        /// </summary>
        /// <param name="options">The settings to crop by.</param>
        /// <returns>The <see cref="ImageFactory"/>.</returns>
        public ImageFactory Crop(CropOptions options)
        {
            this.CheckLoaded();

            var processor = new Crop(options);
            this.ApplyProcessor(processor);

            return this;
        }

        /// <summary>
        /// Detect edges within the current image converting the image to grayscale before processing.
        /// </summary>
        /// <param name="filter">The filter for detecting edges.</param>
        /// <returns>The <see cref="ImageFactory"/>.</returns>
        public ImageFactory DetectEdges(EdgeDetectionOperators filter)
        {
            return this.DetectEdges(filter, true);
        }

        /// <summary>
        /// Detect edges within the current image.
        /// </summary>
        /// <param name="filter">The filter for detecting edges.</param>
        /// <param name="grayscale">Whether to convert the image to grascale before processing.</param>
        /// <returns>The <see cref="ImageFactory"/>.</returns>
        public ImageFactory DetectEdges(EdgeDetectionOperators filter, bool grayscale)
        {
            this.CheckLoaded();
            IGraphicsProcessor processor = null;
            switch (filter)
            {
                case EdgeDetectionOperators.Kayyali:
                    processor = new Kayyali(grayscale);
                    break;
                case EdgeDetectionOperators.Laplacian3x3:
                    processor = new Laplacian3x3(grayscale);
                    break;
                case EdgeDetectionOperators.Laplacian5x5:
                    processor = new Laplacian5x5(grayscale);
                    break;
                case EdgeDetectionOperators.LaplacianOfGaussian:
                    processor = new LaplacianOfGaussian(grayscale);
                    break;
                case EdgeDetectionOperators.Prewitt:
                    processor = new Prewitt(grayscale);
                    break;
                case EdgeDetectionOperators.RobertsCross:
                    processor = new RobertsCross(grayscale);
                    break;
                case EdgeDetectionOperators.Scharr:
                    processor = new Scharr(grayscale);
                    break;
                case EdgeDetectionOperators.Sobel:
                    processor = new Sobel(grayscale);
                    break;
            }

            this.ApplyProcessor(processor);

            return this;
        }

        /// <summary>
        /// Changes the brightness of the current image.
        /// </summary>
        /// <param name="degrees">The rotation angle in degrees to adjust the hue.</param>
        /// <returns>The <see cref="ImageFactory"/>.</returns>
        public ImageFactory Hue(float degrees)
        {
            this.CheckLoaded();

            var processor = new Hue(degrees);
            this.ApplyProcessor(processor);

            return this;
        }

        /// <summary>
        /// Pixelates the current image at the given location and size.
        /// </summary>
        /// <param name="size">The size of the pixels.</param>
        /// <returns>The <see cref="ImageFactory"/>.</returns>
        public ImageFactory Pixelate(int size)
        {
            var bounds = new Rectangle(Point.Empty, this.Image.Size);
            var options = new PixelateOptions(size, bounds);

            return this.Pixelate(options);
        }

        /// <summary>
        /// Pixelates the current image at the given location and size.
        /// </summary>
        /// <param name="options">The settings to crop by.</param>
        /// <returns>The <see cref="ImageFactory"/>.</returns>
        public ImageFactory Pixelate(PixelateOptions options)
        {
            this.CheckLoaded();

            var processor = new Pixelate(options);
            this.ApplyProcessor(processor);

            return this;
        }

        /// <summary>
        /// Resizes the current image to the given dimensions.
        /// </summary>
        /// <param name="size">The target width and height.</param>
        /// <returns>The <see cref="ImageFactory"/>.</returns>
        public ImageFactory Resize(Size size) => this.Resize(size.Width, size.Height);

        /// <summary>
        /// Resizes the current image to the given dimensions.
        /// </summary>
        /// <param name="width">The target width.</param>
        /// <param name="height">The target height.</param>
        /// <returns>The <see cref="ImageFactory"/>.</returns>
        public ImageFactory Resize(int width, int height) => this.Resize(width, height, ResizeMode.Crop);

        /// <summary>
        /// Resizes the current image to the given dimension using the mode.
        /// </summary>
        /// <param name="width">The target width.</param>
        /// <param name="height">The target height.</param>
        /// <param name="mode">Thye mode to use to calculate the result.</param>
        /// <returns>The <see cref="ImageFactory"/>.</returns>
        public ImageFactory Resize(int width, int height, ResizeMode mode)
        {
            var options = new ResizeOptions { Size = new Size(width, height), ResizeMode = mode };
            return this.Resize(options);
        }

        /// <summary>
        /// Resizes the current image to the given location and size.
        /// </summary>
        /// <param name="options">The settings to resize by.</param>
        /// <returns>The <see cref="ImageFactory"/>.</returns>
        public ImageFactory Resize(ResizeOptions options)
        {
            this.CheckLoaded();

            var processor = new Resize(options);
            this.ApplyProcessor(processor);

            return this;
        }

        /// <summary>
        /// Changes the saturation of the current image.
        /// </summary>
        /// <param name="percentage">
        /// The percentage by which to alter the images saturation. Range -100..100.
        /// </param>
        /// <returns>The <see cref="ImageFactory"/>.</returns>
        public ImageFactory Saturation(float percentage)
        {
            this.CheckLoaded();

            var processor = new Saturation(percentage);
            this.ApplyProcessor(processor);

            return this;
        }
    }
}
