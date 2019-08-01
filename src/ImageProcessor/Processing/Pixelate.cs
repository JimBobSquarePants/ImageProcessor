// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Pixelates an image.
    /// </summary>
    public class Pixelate : IGraphicsProcessor<PixelateOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Pixelate"/> class.
        /// </summary>
        /// <param name="options">The pixelate options.</param>
        public Pixelate(PixelateOptions options) => this.Options = options;

        /// <inheritdoc/>
        public PixelateOptions Options { get; }

        /// <inheritdoc/>
        public Image ProcessImageFrame(ImageFactory factory, Image frame)
        {
            int size = this.Options.Size;
            var rectangle = Rectangle.Intersect(this.Options.Rectangle, new Rectangle(Point.Empty, frame.Size));
            int startX = rectangle.X;
            int startY = rectangle.Y;
            int offset = size / 2;
            int maxX = rectangle.Right;
            int maxY = rectangle.Bottom;

            // Align start/end positions.
            int minX = Math.Max(0, startX);
            int minY = Math.Max(0, startY);

            // Reset offset if necessary.
            if (minX > 0)
            {
                startX = 0;
            }

            if (minY > 0)
            {
                startY = 0;
            }

            using (var fastBitmap = new FastBitmap(frame))
            {
                // Get the range on the y-plane to choose from.
                IEnumerable<int> range = EnumerableUtilities.SteppedRange(minY, maxY, size);

                Parallel.ForEach(
                    range,
                    y =>
                    {
                        int offsetY = y - startY;
                        int offsetPy = offset;

                        // Make sure that the offset is within the boundary of the image.
                        while (offsetY + offsetPy >= maxY)
                        {
                            offsetPy--;
                        }

                        for (int x = minX; x < maxX; x += size)
                        {
                            int offsetX = x - startX;
                            int offsetPx = offset;

                            while (x + offsetPx >= maxX)
                            {
                                offsetPx--;
                            }

                            // Get the pixel color in the centre of the soon to be pixelated area.
                            Color pixel = fastBitmap.GetPixel(offsetX + offsetPx, offsetY + offsetPy);

                            // For each pixel in the pixelate size, set it to the centre color.
                            for (int l = offsetY; l < offsetY + size && l < maxY; l++)
                            {
                                for (int k = offsetX; k < offsetX + size && k < maxX; k++)
                                {
                                    fastBitmap.SetPixel(k, l, pixel);
                                }
                            }
                        }
                    });
            }

            return frame;
        }
    }

    /// <summary>
    /// The pixelate options for pixelating images.
    /// </summary>
    public class PixelateOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PixelateOptions"/> class.
        /// </summary>
        /// <param name="size">The pixel size.</param>
        /// <param name="rectangle">The bounds within which to pixelate.</param>
        public PixelateOptions(int size, Rectangle rectangle)
        {
            if (size < 1)
            {
                throw new ImageProcessingException($"{nameof(size)} must be >= 1.");
            }

            if (rectangle == default)
            {
                throw new ImageProcessingException($"{nameof(rectangle)} must have value");
            }

            this.Size = size;
            this.Rectangle = rectangle;
        }

        /// <summary>
        /// Gets the size of the pixels.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Gets the rectangle bounds within which to pixelate.
        /// </summary>
        public Rectangle Rectangle { get; }
    }
}
