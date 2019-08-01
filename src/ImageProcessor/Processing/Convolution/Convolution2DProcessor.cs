// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Applies convolution to an image using two 2 dimensional kernel arrays.
    /// </summary>
    public class Convolution2DProcessor : IGraphicsProcessor<KernelPair>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Convolution2DProcessor"/> class.
        /// </summary>
        /// <param name="kernels">The horizontal and vertical kernel operators.</param>
        public Convolution2DProcessor(KernelPair kernels) => this.Options = kernels;

        /// <inheritdoc/>
        public KernelPair Options { get; }

        /// <inheritdoc/>
        public Image ProcessImageFrame(ImageFactory factory, Image frame)
        {
            int width = frame.Width;
            int height = frame.Height;
            int maxWidth = width + 1;
            int maxHeight = height + 1;
            int bufferedWidth = width + 2;
            int bufferedHeight = height + 2;

            Bitmap result = FormatUtilities.CreateEmptyFrameFrom(frame);

            // We use a trick here to detect right to the edges of the image.
            // flip/tile the image with a pixel in excess in each direction to duplicate pixels.
            // Later on we draw pixels without that excess.
            var buffer = new Bitmap(bufferedWidth, bufferedHeight, frame.PixelFormat);
            buffer.SetResolution(frame.HorizontalResolution, frame.VerticalResolution);
            var rectangle = new Rectangle(0, 0, width, height);
            var bufferedRectangle = new Rectangle(0, 0, bufferedWidth, bufferedHeight);

            using (var graphics = Graphics.FromImage(buffer))
            using (var attributes = new ImageAttributes())
            using (var tb = new TextureBrush(frame, rectangle, attributes))
            {
                tb.WrapMode = WrapMode.TileFlipXY;
                tb.TranslateTransform(1, 1);

                graphics.FillRectangle(tb, bufferedRectangle);
            }

            double[,] kernelX = this.Options.KernelX;
            double[,] kernelY = this.Options.KernelY;

            int kernelLength = kernelX.GetLength(0);
            int radius = kernelLength >> 1;

            using (var fastBuffer = new FastBitmap(buffer))
            using (var fastResult = new FastBitmap(result))
            {
                // Loop through the pixels.
                Parallel.For(
                    0,
                    bufferedHeight,
                    y =>
                    {
                        for (int x = 0; x < bufferedWidth; x++)
                        {
                            double rX = 0;
                            double rY = 0;
                            double gX = 0;
                            double gY = 0;
                            double bX = 0;
                            double bY = 0;

                            // Apply each matrix multiplier to the color components for each pixel.
                            for (int fy = 0; fy < kernelLength; fy++)
                            {
                                int fyr = fy - radius;
                                int offsetY = y + fyr;

                                // Skip the current row
                                if (offsetY < 0)
                                {
                                    continue;
                                }

                                // Outwith the current bounds so break.
                                if (offsetY >= bufferedHeight)
                                {
                                    break;
                                }

                                for (int fx = 0; fx < kernelLength; fx++)
                                {
                                    int fxr = fx - radius;
                                    int offsetX = x + fxr;

                                    // Skip the column
                                    if (offsetX < 0)
                                    {
                                        continue;
                                    }

                                    if (offsetX < bufferedWidth)
                                    {
                                        Color currentColor = fastBuffer.GetPixel(offsetX, offsetY);
                                        double r = currentColor.R;
                                        double g = currentColor.G;
                                        double b = currentColor.B;
                                        double xMultiplier = kernelX[fy, fx];
                                        double yMultiplier = kernelY[fy, fx];

                                        rX += xMultiplier * r;
                                        rY += yMultiplier * r;

                                        gX += xMultiplier * g;
                                        gY += yMultiplier * g;

                                        bX += xMultiplier * b;
                                        bY += yMultiplier * b;
                                    }
                                }
                            }

                            if (y > 0 && x > 0 && y < maxHeight && x < maxWidth)
                            {
                                // Find the dot product and sanitize.
                                byte red = Math.Sqrt((rX * rX) + (rY * rY)).ToByte();
                                byte green = Math.Sqrt((gX * gX) + (gY * gY)).ToByte();
                                byte blue = Math.Sqrt((bX * bX) + (bY * bY)).ToByte();

                                var newColor = Color.FromArgb(red, green, blue);
                                fastResult.SetPixel(x - 1, y - 1, newColor);
                            }
                        }
                    });
            }

            buffer.Dispose();
            frame.Dispose();
            return result;
        }
    }

    /// <summary>
    /// A pair of convolution kernels.
    /// </summary>
    public class KernelPair
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KernelPair"/> class.
        /// </summary>
        /// <param name="kernelX">The horizontal kernel operator.</param>
        /// <param name="kernelY">The vertical kernel operator.</param>
        public KernelPair(double[,] kernelX, double[,] kernelY)
        {
            if (kernelX.GetLength(0) != kernelY.GetLength(0) || kernelX.GetLength(1) != kernelY.GetLength(1))
            {
                throw new ImageProcessingException("Convolution kernels must be the same size.");
            }

            this.KernelX = kernelX;
            this.KernelY = kernelY;
        }

        /// <summary>
        /// Gets the horizontal kernel operator.
        /// </summary>
        public double[,] KernelX { get; }

        /// <summary>
        /// Gets the vertical kernel operator.
        /// </summary>
        public double[,] KernelY { get; }
    }
}
