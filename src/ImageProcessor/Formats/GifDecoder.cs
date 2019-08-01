// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using ImageProcessor.Metadata;

namespace ImageProcessor.Formats
{
    /// <summary>
    /// Allows the decoding of gifs into individual frames.
    /// </summary>
    public class GifDecoder
    {
        private readonly Image image;
        private readonly byte[] times = new byte[4];

        /// <summary>
        /// Initializes a new instance of the <see cref="GifDecoder"/> class.
        /// </summary>
        /// <param name="image">
        /// The <see cref="Image"/> to decode.
        /// </param>
        public GifDecoder(Image image)
            : this(image, FrameProcessingMode.All)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GifDecoder"/> class.
        /// </summary>
        /// <param name="image">The image to decode.</param>
        /// <param name="frameProcessingMode">The frame processing mode.</param>
        public GifDecoder(Image image, FrameProcessingMode frameProcessingMode)
        {
            this.image = image;

            if (FormatUtilities.IsAnimated(image) && frameProcessingMode == FrameProcessingMode.All)
            {
                this.IsAnimated = true;
                this.FrameCount = image.GetFrameCount(FrameDimension.Time);
                const int LoopCount = (int)ExifPropertyTag.LoopCount;
                const int FrameDelay = (int)ExifPropertyTag.FrameDelay;

                // Loop info is stored at byte 20737. Default to infinite loop if not found.
                this.LoopCount = Array.IndexOf(image.PropertyIdList, LoopCount) != -1
                    ? BitConverter.ToInt16(image.GetPropertyItem(LoopCount).Value, 0)
                    : 0;

                // Get the times stored in the gif. Default to 0 if not found.
                if (Array.IndexOf(this.image.PropertyIdList, FrameDelay) != -1)
                {
                    this.times = this.image.GetPropertyItem(FrameDelay).Value;
                }
            }
            else
            {
                this.FrameCount = 1;
            }
        }

        /// <summary>
        /// Gets the input image.
        /// </summary>
        public Image Image { get; }

        /// <summary>
        /// Gets a value indicating whether the image is animated.
        /// </summary>
        public bool IsAnimated { get; }

        /// <summary>
        /// Gets the loop count.
        /// </summary>
        public int LoopCount { get; }

        /// <summary>
        /// Gets the frame count.
        /// </summary>
        public int FrameCount { get; }

        /// <summary>
        /// Gets the frame at the specified index.
        /// <remarks>
        /// Image frames are returned in <see cref="PixelFormat.Format32bppArgb"/> format to allow processing
        /// using the <see cref="Graphics"/> canvas.
        /// </remarks>
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The <see cref="GifFrame"/>.
        /// </returns>
        public GifFrame GetFrame(int index)
        {
            // Convert each 4-byte chunk into an integer.
            // GDI returns a single array with all delays, while Mono returns a different array for each frame.
            var delay = TimeSpan.FromMilliseconds(BitConverter.ToInt32(this.times, (4 * index) % this.times.Length) * 10);

            // Find the frame
            this.image.SelectActiveFrame(FrameDimension.Time, index);

            var frame = new GifFrame(this.image, delay);

            // Reset the image
            this.image.SelectActiveFrame(FrameDimension.Time, 0);

            return frame;
        }
    }
}
