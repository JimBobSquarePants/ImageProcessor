// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Enumerated crop modes to apply to cropped images.
    /// </summary>
    public enum CropMode
    {
        /// <summary>
        /// Crops the image using the standard rectangle model of x, y, width, height.
        /// </summary>
        Pixels,

        /// <summary>
        /// Crops the image using percents model left, top, right, bottom.
        /// </summary>
        Percentage
    }

    /// <summary>
    /// Crops an image to the given dimensions.
    /// </summary>
    public class Crop : IGraphicsProcessor<CropOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Crop"/> class.
        /// </summary>
        /// <param name="settings">The settings to crop by.</param>
        public Crop(CropOptions settings)
        {
            this.Options = settings;
        }

        /// <inheritdoc/>
        public CropOptions Options { get; }

        /// <inheritdoc/>
        public Image ProcessImageFrame(ImageFactory factory, Image frame)
        {
            CropOptions options = this.Options;

            RectangleF bounds;
            if (options.CropMode == CropMode.Percentage)
            {
                // Fix for whole 0-1 values.
                float percentLeft = options.Left > 1 ? options.Left / 100 : options.Left;
                float percentRight = options.Right > 1 ? options.Right / 100 : options.Right;
                float percentTop = options.Top > 1 ? options.Top / 100 : options.Top;
                float percentBottom = options.Bottom > 1 ? options.Bottom / 100 : options.Bottom;

                // Work out the percents.
                float left = percentLeft * frame.Width;
                float top = percentTop * frame.Height;
                float width = percentRight < 1 ? (1 - percentLeft - percentRight) * frame.Width : frame.Width;
                float height = percentBottom < 1 ? (1 - percentTop - percentBottom) * frame.Height : frame.Height;

                bounds = new RectangleF(left, top, width, height);
            }
            else
            {
                bounds = new RectangleF(options.Left, options.Top, options.Right, options.Bottom);
            }

            var rectangle = Rectangle.Intersect(Rectangle.Round(bounds), new Rectangle(0, 0, frame.Width, frame.Height));

            var result = new Bitmap(rectangle.Width, rectangle.Height, frame.PixelFormat);
            result.SetResolution(frame.HorizontalResolution, frame.VerticalResolution);

            using (var graphics = Graphics.FromImage(result))
            {
                graphics.DrawImage(
                    frame,
                    new Rectangle(0, 0, rectangle.Width, rectangle.Height),
                    rectangle.X,
                    rectangle.Y,
                    rectangle.Width,
                    rectangle.Height,
                    GraphicsUnit.Pixel);
            }

            frame.Dispose();
            return result;
        }
    }

    /// <summary>
    /// Contains settings for the crop processor.
    /// </summary>
    public class CropOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CropOptions"/> class.
        /// </summary>
        /// <param name="left">The left coordinate to crop.</param>
        /// <param name="top">The top coordinate to crop.</param>
        /// <param name="right">The right coordinate to crop.</param>
        /// <param name="bottom">The bottom coordinate to crop.</param>
        public CropOptions(
            float left,
            float top,
            float right,
            float bottom)
            : this(left, top, right, bottom, CropMode.Pixels)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CropOptions"/> class.
        /// </summary>
        /// <param name="left">The left coordinate to crop.</param>
        /// <param name="top">The top coordinate to crop.</param>
        /// <param name="right">The right coordinate to crop.</param>
        /// <param name="bottom">The bottom coordinate to crop.</param>
        /// <param name="cropMode">The <see cref="CropMode"/>.</param>
        /// <remarks>
        /// If the <see cref="CropMode"/> is set to <see cref="CropMode.Percentage"/> then the
        /// four coordinates represent the percentile to reduce from each edge.
        /// </remarks>
        public CropOptions(
            float left,
            float top,
            float right,
            float bottom,
            CropMode cropMode)
        {
            if (left < 0 || top < 0 || right < 0 || bottom < 0)
            {
                throw new ImageProcessingException("Crop coordinates cannot be less than 0.");
            }

            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
            this.CropMode = cropMode;
        }

        /// <summary>
        /// Gets the left coordinate to crop.
        /// </summary>
        public float Left { get; }

        /// <summary>
        /// Gets the top coordinate to crop.
        /// </summary>
        public float Top { get; }

        /// <summary>
        /// Gets the right coordinate to crop.
        /// </summary>
        public float Right { get; }

        /// <summary>
        /// Gets the bottom coordinate to crop.
        /// </summary>
        public float Bottom { get; }

        /// <summary>
        /// Gets the <see cref="CropMode"/>.
        /// </summary>
        public CropMode CropMode { get; }

        /// <inheritdoc/>
        public override string ToString() => $"[Left {this.Left}, Top {this.Top}, Right {this.Right}, Bottom {this.Bottom}]";
    }
}
