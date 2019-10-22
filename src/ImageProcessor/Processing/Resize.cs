// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Provides enumeration over how the image should be resized.
    /// </summary>
    public enum ResizeMode
    {
        /// <summary>
        /// Crops the resized image to fit the bounds of its container.
        /// </summary>
        Crop,

        /// <summary>
        /// Pads the resized image to fit the bounds of its container.
        /// If only one dimension is passed, will maintain the original aspect ratio.
        /// </summary>
        Pad,

        /// <summary>
        /// Pads the image to fit the bound of the container without resizing the
        /// original source.
        /// When downscaling, performs the same functionality as <see cref="Pad"/>
        /// </summary>
        BoxPad,

        /// <summary>
        /// Constrains the resized image to fit the bounds of its container maintaining
        /// the original aspect ratio.
        /// </summary>
        Max,

        /// <summary>
        /// Resizes the image until the shortest side reaches the set given dimension.
        /// Upscaling is disabled in this mode and the original image will be returned
        /// if attempted.
        /// </summary>
        Min,

        /// <summary>
        /// Stretches the resized image to fit the bounds of its container.
        /// </summary>
        Stretch
    }

    /// <summary>
    /// Enumerated anchor positions to apply to resized images.
    /// </summary>
    public enum AnchorPositionMode
    {
        /// <summary>
        /// Anchors the position of the image to the center of it's bounding container.
        /// </summary>
        Center,

        /// <summary>
        /// Anchors the position of the image to the top of it's bounding container.
        /// </summary>
        Top,

        /// <summary>
        /// Anchors the position of the image to the bottom of it's bounding container.
        /// </summary>
        Bottom,

        /// <summary>
        /// Anchors the position of the image to the left of it's bounding container.
        /// </summary>
        Left,

        /// <summary>
        /// Anchors the position of the image to the right of it's bounding container.
        /// </summary>
        Right,

        /// <summary>
        /// Anchors the position of the image to the top left side of it's bounding container.
        /// </summary>
        TopLeft,

        /// <summary>
        /// Anchors the position of the image to the top right side of it's bounding container.
        /// </summary>
        TopRight,

        /// <summary>
        /// Anchors the position of the image to the bottom right side of it's bounding container.
        /// </summary>
        BottomRight,

        /// <summary>
        /// Anchors the position of the image to the bottom left side of it's bounding container.
        /// </summary>
        BottomLeft
    }

    /// <summary>
    /// Resizes an image to the given dimensions.
    /// </summary>
    public class Resize : IGraphicsProcessor<ResizeOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Resize"/> class.
        /// </summary>
        /// <param name="options">The resize options.</param>
        public Resize(ResizeOptions options) => this.Options = options;

        /// <inheritdoc/>
        public ResizeOptions Options { get; }

        /// <inheritdoc/>
        public Image ProcessImageFrame(ImageFactory factory, Image frame)
        {
            Size sourceSize = frame.Size;
            int targetWidth = this.Options.Size.Width;
            int targetHeight = this.Options.Size.Height;

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

            (Size size, Rectangle rectangle) = ResizeHelper.CalculateTargetLocationAndBounds(sourceSize, this.Options, targetWidth, targetHeight);

            int width = size.Width;
            int height = size.Height;
            Rectangle targetRectangle = rectangle;

            var result = new Bitmap(width, height, frame.PixelFormat);
            result.SetResolution(frame.HorizontalResolution, frame.VerticalResolution);

            using (var graphics = Graphics.FromImage(result))
            using (var attributes = new ImageAttributes())
            {
                attributes.SetWrapMode(WrapMode.TileFlipXY);

                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.DrawImage(
                    frame,
                    targetRectangle,
                    0,
                    0,
                    sourceSize.Width,
                    sourceSize.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }

            frame.Dispose();
            return result;
        }
    }

    /// <summary>
    /// The resize options for resizing images against certain modes.
    /// </summary>
    public class ResizeOptions
    {
        /// <summary>
        /// Gets or sets the resize mode.
        /// </summary>
        public ResizeMode Mode { get; set; } = ResizeMode.Crop;

        /// <summary>
        /// Gets or sets the anchor position.
        /// </summary>
        public AnchorPositionMode Position { get; set; } = AnchorPositionMode.Center;

        /// <summary>
        /// Gets or sets the center coordinates.
        /// </summary>
        public PointF? CenterCoordinates { get; set; }

        /// <summary>
        /// Gets or sets the target size.
        /// </summary>
        public Size Size { get; set; }
    }
}
