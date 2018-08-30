// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComicMatrixFilter.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates methods with which to add a comic filter to an image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Filters.Photo
{
    using System.Drawing;
    using System.Drawing.Imaging;

    using ImageProcessor.Imaging.Filters.Artistic;
    using ImageProcessor.Imaging.Helpers;

    /// <summary>
    /// Encapsulates methods with which to add a comic filter to an image.
    /// </summary>
    internal class ComicMatrixFilter : MatrixFilterBase
    {
        /// <summary>
        /// Gets the <see cref="T:System.Drawing.Imaging.ColorMatrix"/> for this filter instance.
        /// </summary>
        public override ColorMatrix Matrix => ColorMatrixes.ComicLow;

        /// <summary>
        /// Processes the image.
        /// </summary>
        /// <param name="source">The current image to process</param>
        /// <param name="destination">The new Image to return</param>
        /// <returns>
        /// The processed <see cref="System.Drawing.Bitmap"/>.
        /// </returns>
        public override Bitmap TransformImage(Image source, Image destination)
        {
            // Bitmaps for comic pattern
            Bitmap highBitmap = null;
            Bitmap lowBitmap = null;
            Bitmap patternBitmap = null;
            Bitmap edgeBitmap = null;
            int width = source.Width;
            int height = source.Height;

            try
            {
                using (var attributes = new ImageAttributes())
                {
                    var rectangle = new Rectangle(0, 0, source.Width, source.Height);

                    attributes.SetColorMatrix(ColorMatrixes.ComicHigh);

                    // Draw the image with the high comic colormatrix.
                    highBitmap = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppPArgb);
                    highBitmap.SetResolution(source.HorizontalResolution, source.VerticalResolution);

                    // Apply a oil painting filter to the image.
                    highBitmap = new OilPaintingFilter(3, 5).ApplyFilter((Bitmap)source);

                    // Draw the edges.
                    edgeBitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                    edgeBitmap.SetResolution(source.HorizontalResolution, source.VerticalResolution);
                    edgeBitmap = Effects.Trace(source, edgeBitmap, 120);

                    using (var graphics = Graphics.FromImage(highBitmap))
                    {
                        graphics.DrawImage(highBitmap, rectangle, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
                    }

                    // Create a bitmap for overlaying.
                    lowBitmap = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppPArgb);
                    lowBitmap.SetResolution(source.HorizontalResolution, source.VerticalResolution);

                    // Set the color matrix
                    attributes.SetColorMatrix(this.Matrix);

                    // Draw the image with the losatch colormatrix.
                    using (var graphics = Graphics.FromImage(lowBitmap))
                    {
                        graphics.DrawImage(highBitmap, rectangle, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
                    }

                    // We need to create a new image now with a pattern mask to paint it
                    // onto the other image with.
                    patternBitmap = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppPArgb);
                    patternBitmap.SetResolution(source.HorizontalResolution, source.VerticalResolution);

                    // Create the pattern mask.
                    using (var graphics = Graphics.FromImage(patternBitmap))
                    {
                        graphics.Clear(Color.Transparent);

                        for (int y = 0; y < height; y += 8)
                        {
                            for (int x = 0; x < width; x += 4)
                            {
                                graphics.FillEllipse(Brushes.White, x, y, 3, 3);
                                graphics.FillEllipse(Brushes.White, x + 2, y + 4, 3, 3);
                            }
                        }
                    }

                    // Transfer the alpha channel from the mask to the low saturation image.
                    lowBitmap = Effects.ApplyMask(lowBitmap, patternBitmap);

                    using (var graphics = Graphics.FromImage(destination))
                    {
                        graphics.Clear(Color.Transparent);

                        // Overlay the image.
                        graphics.DrawImage(highBitmap, 0, 0);
                        graphics.DrawImage(lowBitmap, 0, 0);
                        graphics.DrawImage(edgeBitmap, 0, 0);

                        // Draw an edge around the image.
                        using (var blackPen = new Pen(Color.Black))
                        {
                            blackPen.Width = 4;
                            graphics.DrawRectangle(blackPen, rectangle);
                        }

                        // Dispose of the other images
                        highBitmap.Dispose();
                        lowBitmap.Dispose();
                        patternBitmap.Dispose();
                        edgeBitmap.Dispose();
                    }
                }

                // Reassign the image.
                source.Dispose();
                source = destination;
            }
            catch
            {
                destination?.Dispose();

                highBitmap?.Dispose();

                lowBitmap?.Dispose();

                patternBitmap?.Dispose();

                edgeBitmap?.Dispose();
            }

            return (Bitmap)source;
        }
    }
}