// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HalftoneFilter.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The halftone filter applies a classical CMYK filter to the given image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Filters.Artistic
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Threading.Tasks;

    using ImageProcessor.Imaging.Colors;
    using ImageProcessor.Imaging.Helpers;

    /// <summary>
    /// The halftone filter applies a classical CMYK filter to the given image.
    /// </summary>
    public class HalftoneFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HalftoneFilter"/> class.
        /// </summary>
        public HalftoneFilter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HalftoneFilter"/> class.
        /// </summary>
        /// <param name="distance">
        /// The distance.
        /// </param>
        public HalftoneFilter(int distance) => this.Distance = distance;

        /// <summary>
        /// Gets or sets the angle of the cyan component in degrees.
        /// </summary>
        public float CyanAngle { get; set; } = 15f;

        /// <summary>
        /// Gets or sets the angle of the magenta component in degrees.
        /// </summary>
        public float MagentaAngle { get; set; } = 75f;

        /// <summary>
        /// Gets or sets the angle of the yellow component in degrees.
        /// </summary>
        public float YellowAngle { get; set; }

        /// <summary>
        /// Gets or sets the angle of the keyline black component in degrees.
        /// </summary>
        public float KeylineAngle { get; set; } = 45f;

        /// <summary>
        /// Gets or sets the distance between component points.
        /// </summary>
        public int Distance { get; set; } = 4;

        /// <summary>
        /// Applies the halftone filter. 
        /// </summary>
        /// <param name="source">
        /// The <see cref="Bitmap"/> to apply the filter to.
        /// </param>
        /// <returns>
        /// The <see cref="Bitmap"/> with the filter applied.
        /// </returns>
        public Bitmap ApplyFilter(Bitmap source)
        {
            // TODO: Make this class implement an interface?
            Bitmap padded = null;
            Bitmap cyan = null;
            Bitmap magenta = null;
            Bitmap yellow = null;
            Bitmap keyline = null;
            Bitmap newImage = null;

            try
            {
                int sourceWidth = source.Width;
                int sourceHeight = source.Height;
                int width = source.Width + this.Distance;
                int height = source.Height + this.Distance;

                // Draw a slightly larger image, flipping the top/left pixels to prevent
                // jagged edge of output.
                padded = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                padded.SetResolution(source.HorizontalResolution, source.VerticalResolution);
                using (var graphicsPadded = Graphics.FromImage(padded))
                {
                    graphicsPadded.Clear(Color.White);
                    var destinationRectangle = new Rectangle(0, 0, sourceWidth + this.Distance, source.Height + this.Distance);
                    using (var tb = new TextureBrush(source))
                    {
                        tb.WrapMode = WrapMode.TileFlipXY;
                        tb.TranslateTransform(this.Distance, this.Distance);
                        graphicsPadded.FillRectangle(tb, destinationRectangle);
                    }
                }

                // Calculate min and max widths/heights.
                Rectangle rotatedBounds = this.GetBoundingRectangle(width, height);
                int minY = -(rotatedBounds.Height + height);
                int maxY = rotatedBounds.Height + height;
                int minX = -(rotatedBounds.Width + width);
                int maxX = rotatedBounds.Width + width;
                Point center = Point.Empty;

                // Yellow oversaturates the output.
                int offset = this.Distance;
                float yellowMultiplier = this.Distance * 1.587f;
                float magentaMultiplier = this.Distance * 2.176f;
                float multiplier = this.Distance * 2.2f;
                float max = this.Distance * (float)Math.Sqrt(2);
                float magentaMax = this.Distance * (float)Math.Sqrt(1.4545);

                // Bump up the keyline max so that black looks black.
                float keylineMax = max * (float)Math.Sqrt(2);

                // Color sampled process colours from Wikipedia pages. 
                // Keyline brush is declared separately.
                Brush cyanBrush = new SolidBrush(Color.FromArgb(0, 183, 235));
                Brush magentaBrush = new SolidBrush(Color.FromArgb(255, 0, 144));
                Brush yellowBrush = new SolidBrush(Color.FromArgb(255, 239, 0));

                // Create our images.
                cyan = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                magenta = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                yellow = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                keyline = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                newImage = new Bitmap(sourceWidth, sourceHeight, PixelFormat.Format32bppPArgb);

                // Ensure the correct resolution is set.
                cyan.SetResolution(source.HorizontalResolution, source.VerticalResolution);
                magenta.SetResolution(source.HorizontalResolution, source.VerticalResolution);
                yellow.SetResolution(source.HorizontalResolution, source.VerticalResolution);
                keyline.SetResolution(source.HorizontalResolution, source.VerticalResolution);
                newImage.SetResolution(source.HorizontalResolution, source.VerticalResolution);

                // Check bounds against this.
                var rectangle = new Rectangle(0, 0, width, height);

                using (var graphicsCyan = Graphics.FromImage(cyan))
                using (var graphicsMagenta = Graphics.FromImage(magenta))
                using (var graphicsYellow = Graphics.FromImage(yellow))
                using (var graphicsKeyline = Graphics.FromImage(keyline))
                {
                    // Set the quality properties.
                    graphicsCyan.PixelOffsetMode = PixelOffsetMode.Half;
                    graphicsMagenta.PixelOffsetMode = PixelOffsetMode.Half;
                    graphicsYellow.PixelOffsetMode = PixelOffsetMode.Half;
                    graphicsKeyline.PixelOffsetMode = PixelOffsetMode.Half;

                    graphicsCyan.SmoothingMode = SmoothingMode.AntiAlias;
                    graphicsMagenta.SmoothingMode = SmoothingMode.AntiAlias;
                    graphicsYellow.SmoothingMode = SmoothingMode.AntiAlias;
                    graphicsKeyline.SmoothingMode = SmoothingMode.AntiAlias;

                    graphicsCyan.CompositingQuality = CompositingQuality.HighQuality;
                    graphicsMagenta.CompositingQuality = CompositingQuality.HighQuality;
                    graphicsYellow.CompositingQuality = CompositingQuality.HighQuality;
                    graphicsKeyline.CompositingQuality = CompositingQuality.HighQuality;

                    // Set up the canvas.
                    graphicsCyan.Clear(Color.White);
                    graphicsMagenta.Clear(Color.White);
                    graphicsYellow.Clear(Color.White);
                    graphicsKeyline.Clear(Color.White);

                    // This is too slow. The graphics object can't be called within a parallel 
                    // loop so we have to do it old school. :(
                    using (var sourceBitmap = new FastBitmap(padded))
                    {
                        for (int y = minY; y < maxY; y += offset)
                        {
                            for (int x = minX; x < maxX; x += offset)
                            {
                                Color color;
                                CmykColor cmykColor;
                                float brushWidth;

                                // Cyan
                                Point rotatedPoint = ImageMaths.RotatePoint(new Point(x, y), this.CyanAngle, center);
                                int angledX = rotatedPoint.X;
                                int angledY = rotatedPoint.Y;
                                if (rectangle.Contains(new Point(angledX, angledY)))
                                {
                                    color = sourceBitmap.GetPixel(angledX, angledY);
                                    cmykColor = color;
                                    brushWidth = Math.Min((cmykColor.C / 100f) * multiplier, max);
                                    graphicsCyan.FillEllipse(cyanBrush, angledX, angledY, brushWidth, brushWidth);
                                }

                                // Magenta
                                rotatedPoint = ImageMaths.RotatePoint(new Point(x, y), this.MagentaAngle, center);
                                angledX = rotatedPoint.X;
                                angledY = rotatedPoint.Y;
                                if (rectangle.Contains(new Point(angledX, angledY)))
                                {
                                    color = sourceBitmap.GetPixel(angledX, angledY);
                                    cmykColor = color;
                                    brushWidth = Math.Min((cmykColor.M / 100f) * magentaMultiplier, magentaMax);
                                    graphicsMagenta.FillEllipse(magentaBrush, angledX, angledY, brushWidth, brushWidth);
                                }

                                // Yellow
                                rotatedPoint = ImageMaths.RotatePoint(new Point(x, y), this.YellowAngle, center);
                                angledX = rotatedPoint.X;
                                angledY = rotatedPoint.Y;
                                if (rectangle.Contains(new Point(angledX, angledY)))
                                {
                                    color = sourceBitmap.GetPixel(angledX, angledY);
                                    cmykColor = color;
                                    brushWidth = Math.Min((cmykColor.Y / 100f) * yellowMultiplier, max);
                                    graphicsYellow.FillEllipse(yellowBrush, angledX, angledY, brushWidth, brushWidth);
                                }

                                // Keyline 
                                rotatedPoint = ImageMaths.RotatePoint(new Point(x, y), this.KeylineAngle, center);
                                angledX = rotatedPoint.X;
                                angledY = rotatedPoint.Y;
                                if (rectangle.Contains(new Point(angledX, angledY)))
                                {
                                    color = sourceBitmap.GetPixel(angledX, angledY);
                                    cmykColor = color;
                                    brushWidth = Math.Min((cmykColor.K / 100f) * multiplier, keylineMax);

                                    // Just using black is too dark. 
                                    Brush keylineBrush = new SolidBrush(CmykColor.FromCmykColor(0, 0, 0, cmykColor.K));
                                    graphicsKeyline.FillEllipse(keylineBrush, angledX, angledY, brushWidth, brushWidth);
                                }
                            }
                        }
                    }

                    // Set our white background.
                    using (var graphics = Graphics.FromImage(newImage))
                    {
                        graphics.Clear(Color.White);
                    }

                    // Blend the colors now to mimic adaptive blending.
                    using (var cyanBitmap = new FastBitmap(cyan))
                    using (var magentaBitmap = new FastBitmap(magenta))
                    using (var yellowBitmap = new FastBitmap(yellow))
                    using (var keylineBitmap = new FastBitmap(keyline))
                    using (var destinationBitmap = new FastBitmap(newImage))
                    {
                        Parallel.For(
                            offset,
                            height,
                            y =>
                            {
                                for (int x = offset; x < width; x++)
                                {
                                    // ReSharper disable AccessToDisposedClosure
                                    Color cyanPixel = cyanBitmap.GetPixel(x, y);
                                    Color magentaPixel = magentaBitmap.GetPixel(x, y);
                                    Color yellowPixel = yellowBitmap.GetPixel(x, y);
                                    Color keylinePixel = keylineBitmap.GetPixel(x, y);

                                    // Negate the offset.
                                    int xBack = x - offset;
                                    int yBack = y - offset;

                                    CmykColor blended = cyanPixel.AddAsCmykColor(magentaPixel, yellowPixel, keylinePixel);
                                    if (rectangle.Contains(new Point(xBack, yBack)))
                                    {
                                        destinationBitmap.SetPixel(xBack, yBack, blended);
                                    }
                                    // ReSharper restore AccessToDisposedClosure
                                }
                            });
                    }
                }

                padded.Dispose();
                cyan.Dispose();
                magenta.Dispose();
                yellow.Dispose();
                keyline.Dispose();
                source.Dispose();
                source = newImage;
            }
            catch
            {
                padded?.Dispose();
                cyan?.Dispose();
                magenta?.Dispose();
                yellow?.Dispose();
                keyline?.Dispose();
                newImage?.Dispose();
            }

            return source;
        }

        /// <summary>
        /// Gets the bounding rectangle of the image based on the rotating angles.
        /// </summary>
        /// <param name="width">
        /// The width of the image.
        /// </param>
        /// <param name="height">
        /// The height of the image.
        /// </param>
        /// <returns>
        /// The <see cref="Rectangle"/>.
        /// </returns>
        private Rectangle GetBoundingRectangle(int width, int height)
        {
            int maxWidth = 0;
            int maxHeight = 0;
            foreach (float angle in new List<float> { this.CyanAngle, this.MagentaAngle, this.YellowAngle, this.KeylineAngle })
            {
                Size rotatedSize = ImageMaths.GetBoundingRotatedRectangle(width, height, angle).Size;
                maxWidth = Math.Max(maxWidth, rotatedSize.Width);
                maxHeight = Math.Max(maxHeight, rotatedSize.Height);
            }

            return new Rectangle(0, 0, maxWidth, maxHeight);
        }
    }
}
