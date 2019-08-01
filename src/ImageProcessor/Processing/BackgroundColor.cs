// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Changes the background color of an image.
    /// </summary>
    public class BackgroundColor : IGraphicsProcessor<Color>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundColor"/> class.
        /// </summary>
        /// <param name="color">The color to set.</param>
        public BackgroundColor(Color color) => this.Options = color;

        /// <inheritdoc/>
        public Color Options { get; }

        /// <inheritdoc/>
        public Image ProcessImageFrame(ImageFactory factory, Image frame)
        {
            Bitmap result = FormatUtilities.CreateEmptyFrameFrom(frame);

            using (var graphics = Graphics.FromImage(result))
            {
                graphics.PageUnit = GraphicsUnit.Pixel;
                graphics.Clear(this.Options);

                graphics.DrawImageUnscaled(frame, 0, 0);
            }

            frame.Dispose();
            return result;
        }
    }
}
