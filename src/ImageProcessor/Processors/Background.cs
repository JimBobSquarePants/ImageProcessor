// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Overlay.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Adds an image background to the current image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;

    using ImageProcessor.Common.Exceptions;
    using ImageProcessor.Imaging;
    using ImageProcessor.Imaging.Helpers;

    /// <summary>
    /// Adds an image background to the current image.
    /// If the background is larger than the image it will be scaled to match the image.
    /// </summary>
    public class Background : IGraphicsProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Background"/> class.
        /// </summary>
        public Background() => this.Settings = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the dynamic parameter.
        /// </summary>
        public dynamic DynamicParameter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets any additional settings required by the processor.
        /// </summary>
        public Dictionary<string, string> Settings
        {
            get;
            set;
        }

        /// <summary>
        /// Processes the image.
        /// </summary>
        /// <param name="factory">
        /// The current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class containing
        /// the image to process.
        /// </param>
        /// <returns>
        /// The processed image from the current instance of the <see cref="T:ImageProcessor.ImageFactory"/> class.
        /// </returns>
        public Image ProcessImage(ImageFactory factory)
        {
            Image image = factory.Image;
            Bitmap background = null;
            Bitmap newImage = null;

            try
            {
                ImageLayer imageLayer = this.DynamicParameter;
                background = new Bitmap(imageLayer.Image);

                // Set the resolution of the background and the image to match.
                background.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                Size size = imageLayer.Size;
                int width = image.Width;
                int height = image.Height;
                int backgroundWidth = size != Size.Empty ? Math.Min(width, size.Width) : Math.Min(width, background.Width);
                int backgroundHeight = size != Size.Empty ? Math.Min(height, size.Height) : Math.Min(height, background.Height);

                Point? position = imageLayer.Position;
                int opacity = imageLayer.Opacity;

                if (image.Size != background.Size || image.Size != new Size(backgroundWidth, backgroundHeight))
                {
                    // Find the maximum possible dimensions and resize the image.
                    var layer = new ResizeLayer(new Size(backgroundWidth, backgroundHeight), ResizeMode.Max);
                    var resizer = new Resizer(layer) { AnimationProcessMode = factory.AnimationProcessMode };
                    background = resizer.ResizeImage(background, factory.FixGamma);
                    backgroundWidth = background.Width;
                    backgroundHeight = background.Height;
                }

                // Figure out bounds.
                var parent = new Rectangle(0, 0, width, height);
                var child = new Rectangle(0, 0, backgroundWidth, backgroundHeight);

                // Apply opacity.
                if (opacity < 100)
                {
                    background = Adjustments.Alpha(background, opacity);
                }

                newImage = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                newImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(newImage))
                {
                    GraphicsHelper.SetGraphicsOptions(graphics, true);

                    if (position != null)
                    {
                        // Draw the image in position catering for overflow.
                        graphics.DrawImage(background, new Point(Math.Min(position.Value.X, width - backgroundWidth), Math.Min(position.Value.Y, height - backgroundHeight)));
                    }
                    else
                    {
                        RectangleF centered = ImageMaths.CenteredRectangle(parent, child);
                        graphics.DrawImage(background, new PointF(centered.X, centered.Y));
                    }

                    // Draw passed in image onto graphics object.
                    graphics.DrawImage(image, 0, 0, width, height);
                }

                image.Dispose();
                image = newImage;
            }
            catch (Exception ex)
            {
                newImage?.Dispose();

                throw new ImageProcessingException("Error processing image with " + this.GetType().Name, ex);
            }
            finally
            {
                background?.Dispose();
            }

            return image;
        }
    }
}
