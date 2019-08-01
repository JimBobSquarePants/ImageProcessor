// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Applies a convolution kernel to an image.
    /// </summary>
    public class ConvolutionProcessor : IGraphicsProcessor<double[,]>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvolutionProcessor"/> class.
        /// </summary>
        /// <param name="kernel">The convolution kernel.</param>
        public ConvolutionProcessor(double[,] kernel)
        {
            if (kernel.GetLength(0) != kernel.GetLength(1))
            {
                throw new ImageProcessingException("Convolution kernel dimensions must be the same.");
            }

            this.Options = kernel;
        }

        /// <inheritdoc/>
        public double[,] Options { get; }

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

            double[,] kernelX = this.Options;
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
                            double gX = 0;
                            double bX = 0;

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

                                        rX += xMultiplier * r;
                                        gX += xMultiplier * g;
                                        bX += xMultiplier * b;
                                    }
                                }
                            }

                            if (y > 0 && x > 0 && y < maxHeight && x < maxWidth)
                            {
                                // Sanitize.
                                byte red = rX.ToByte();
                                byte green = gX.ToByte();
                                byte blue = bX.ToByte();
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
}
