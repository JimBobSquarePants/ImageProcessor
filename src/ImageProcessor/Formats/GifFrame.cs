// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessor.Formats
{
    /// <summary>
    /// A single gif frame.
    /// </summary>
    public class GifFrame : IDisposable
    {
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="GifFrame"/> class.
        /// </summary>
        /// <param name="source">The source image to copy into the new frame.</param>
        /// <param name="delay">The time, in milliseconds, to wait before animating to the next frame.</param>
        public GifFrame(Image source, TimeSpan delay)
            : this(source, delay, 0, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GifFrame"/> class.
        /// </summary>
        /// <param name="source">The source image to copy into the new frame.</param>
        /// <param name="delay">The time, in milliseconds, to wait before animating to the next frame.</param>
        /// <param name="x">The frame left position.</param>
        /// <param name="y">The frame top position.</param>
        public GifFrame(Image source, TimeSpan delay, int x, int y)
        {
            this.Image = FormatUtilities.DeepCloneImageFrame(source, PixelFormat.Format32bppArgb);
            this.Delay = delay;
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Gets the image, stored in <see cref="PixelFormat.Format32bppArgb"/> format to allow processing
        /// using the <see cref="Graphics"/> canvas.
        /// </summary>
        public Image Image { get; private set; }

        /// <summary>
        /// Gets the delay in milliseconds.
        /// </summary>
        public TimeSpan Delay { get; }

        /// <summary>
        /// Gets the left position of the frame.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the top position of the frame.
        /// </summary>
        public int Y { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.Image?.Dispose();
            this.Image = null;
            this.isDisposed = true;
        }
    }
}
