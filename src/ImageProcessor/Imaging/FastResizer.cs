// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FastResizer.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Resizes images out with <see cref="System.Drawing" /> using well known algorithms.
//   This allows us to work in parallel and avoid locking issues caused by GDI+
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Threading.Tasks;

    using ImageProcessor.Common.Extensions;
    using ImageProcessor.Imaging.Helpers;

    /// <summary>
    /// Resizes images out with <see cref="System.Drawing"/> using well known algorithms.
    /// This allows us to work in parallel and avoid locking issues caused by GDI+
    /// </summary>
    internal static class FastResizer
    {
        /// <summary>
        /// Resize an image using a bicubic interpolation algorithm.
        /// <remarks>
        /// The class implements image resizing filter using bicubic
        /// interpolation algorithm. It uses bicubic kernel as described on
        /// <see href="http://en.wikipedia.org/wiki/Bicubic_interpolation#Bicubic_convolution_algorithm">Wikipedia</see>
        /// </remarks>
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <param name="destinationRectangle">The position within the new image to place the pixels.</param>
        /// <param name="fixGamma">Whether to resize the image using the linear color space.</param>
        /// <returns>
        /// The resized <see cref="Bitmap"/>.
        /// </returns>
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "FastBitmap is incorrectly warned against.")]
        public static Bitmap ResizeBicubic(Bitmap source, int width, int height, Rectangle destinationRectangle, bool fixGamma)
        {
            int sourceWidth = source.Width;
            int sourceHeight = source.Height;
            int startX = destinationRectangle.X;
            int startY = destinationRectangle.Y;
            int endX = destinationRectangle.Width + startX;
            int endY = destinationRectangle.Height + startY;

            // Scaling factors
            double widthFactor = sourceWidth / (double)destinationRectangle.Width;
            double heightFactor = sourceHeight / (double)destinationRectangle.Height;

            // Width and height decreased by 1
            int maxHeight = sourceHeight - 1;
            int maxWidth = sourceWidth - 1;

            Bitmap destination = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            destination.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (FastBitmap sourceBitmap = new FastBitmap(source))
            {
                using (FastBitmap destinationBitmap = new FastBitmap(destination))
                {
                    // For each column
                    Parallel.For(
                       startY,
                       endY,
                        y =>
                        {
                            if (y >= 0 && y < height)
                            {
                                // Y coordinates of source points.
                                double originY = ((y - startY) * heightFactor) - 0.5;
                                int originY1 = (int)originY;
                                double dy = originY - originY1;

                                // For each row.
                                for (int x = startX; x < endX; x++)
                                {
                                    if (x >= 0 && x < width)
                                    {
                                        // X coordinates of source points.
                                        double originX = ((x - startX) * widthFactor) - 0.5f;
                                        int originX1 = (int)originX;
                                        double dx = originX - originX1;

                                        // Destination color components
                                        double r = 0;
                                        double g = 0;
                                        double b = 0;
                                        double a = 0;

                                        for (int yy = -1; yy < 3; yy++)
                                        {
                                            // Get Y cooefficient
                                            double kernel1 = Interpolation.BiCubicKernel(dy - yy);

                                            int originY2 = originY1 + yy;
                                            if (originY2 < 0)
                                            {
                                                originY2 = 0;
                                            }

                                            if (originY2 > maxHeight)
                                            {
                                                originY2 = maxHeight;
                                            }

                                            for (int xx = -1; xx < 3; xx++)
                                            {
                                                // Get X cooefficient
                                                double kernel2 = kernel1 * Interpolation.BiCubicKernel(xx - dx);

                                                int originX2 = originX1 + xx;
                                                if (originX2 < 0)
                                                {
                                                    originX2 = 0;
                                                }

                                                if (originX2 > maxWidth)
                                                {
                                                    originX2 = maxWidth;
                                                }

                                                Color sourceColor = sourceBitmap.GetPixel(originX2, originY2);

                                                if (fixGamma)
                                                {
                                                    sourceColor = PixelOperations.ToLinear(sourceColor);
                                                }

                                                r += kernel2 * sourceColor.R;
                                                g += kernel2 * sourceColor.G;
                                                b += kernel2 * sourceColor.B;
                                                a += kernel2 * sourceColor.A;
                                            }
                                        }

                                        Color destinationColor = Color.FromArgb(
                                            a.ToByte(),
                                            r.ToByte(),
                                            g.ToByte(),
                                            b.ToByte());

                                        if (fixGamma)
                                        {
                                            destinationColor = PixelOperations.ToSRGB(destinationColor);
                                        }

                                        destinationBitmap.SetPixel(x, y, destinationColor);
                                    }
                                }
                            }
                        });
                }
            }

            source.Dispose();
            return destination;
        }

        /// <summary>
        /// Resize an image using a bicubic interpolation algorithm.
        /// The image is preprocessed using a multi-pass box blur to reduce moiré when processing image less than 150x150.
        /// <remarks>
        /// The function implements bicubic kernel developed by Paul Bourke <see cref="http://paulbourke.net"/> 
        /// described <see href="http://docs-hoffmann.de/bicubic03042002.pdf">here</see>
        /// </remarks>
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <param name="destinationRectangle">The position within the new image to place the pixels.</param>
        /// <param name="fixGamma">Whether to resize the image using the linear color space.</param>
        /// <returns>
        /// The resized <see cref="Bitmap"/>.
        /// </returns>
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "FastBitmap is incorrectly warned against.")]
        public static Bitmap ResizeBicubicHighQuality(Bitmap source, int width, int height, Rectangle destinationRectangle, bool fixGamma)
        {
            int sourceWidth = source.Width;
            int sourceHeight = source.Height;
            int startX = destinationRectangle.X;
            int startY = destinationRectangle.Y;
            int endX = destinationRectangle.Width + startX;
            int endY = destinationRectangle.Height + startY;

            // Scaling factors
            double widthFactor = sourceWidth / (double)destinationRectangle.Width;
            double heightFactor = sourceHeight / (double)destinationRectangle.Height;

            // Width and height decreased by 1
            int maxHeight = sourceHeight - 1;
            int maxWidth = sourceWidth - 1;

            Bitmap destination = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            destination.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            // The radius for pre blurring the images.
            // We only apply this for very small images.
            int radius = 0;
            if (width <= 150 && height <= 150)
            {
                radius = 4;
            }

            using (FastBitmap sourceBitmap = new FastBitmap(source))
            {
                using (FastBitmap destinationBitmap = new FastBitmap(destination))
                {
                    // For each column
                    Parallel.For(
                       startY,
                       endY,
                       y =>
                       {
                           if (y >= 0 && y < height)
                           {
                               // Y coordinates of source points.
                               double originY = ((y - startY) * heightFactor) - 0.5;
                               int originY1 = (int)originY;
                               double dy = originY - originY1;

                               // Houses colors for blurring.
                               Color[,] sourceColors = new Color[4, 4];

                               // For each row.
                               for (int x = startX; x < endX; x++)
                               {
                                   if (x >= 0 && x < width)
                                   {
                                       // X coordinates of source points.
                                       double originX = ((x - startX) * widthFactor) - 0.5f;
                                       int originX1 = (int)originX;
                                       double dx = originX - originX1;

                                       // Destination color components
                                       double r = 0;
                                       double g = 0;
                                       double b = 0;
                                       double a = 0;

                                       for (int yy = -1; yy < 3; yy++)
                                       {
                                           int originY2 = originY1 + yy;
                                           if (originY2 < 0)
                                           {
                                               originY2 = 0;
                                           }

                                           if (originY2 > maxHeight)
                                           {
                                               originY2 = maxHeight;
                                           }

                                           for (int xx = -1; xx < 3; xx++)
                                           {
                                               int originX2 = originX1 + xx;
                                               if (originX2 < 0)
                                               {
                                                   originX2 = 0;
                                               }

                                               if (originX2 > maxWidth)
                                               {
                                                   originX2 = maxWidth;
                                               }

                                               Color sourceColor = sourceBitmap.GetPixel(originX2, originY2);

                                               sourceColors[xx + 1, yy + 1] = sourceColor;
                                           }
                                       }

                                       // Blur the colors.
                                       if (radius > 0)
                                       {
                                           sourceColors = BoxBlur(sourceColors, radius, fixGamma);
                                       }

                                       // Do the resize.
                                       for (int yy = -1; yy < 3; yy++)
                                       {
                                           // Get Y cooefficient
                                           double kernel1 = Interpolation.BiCubicBSplineKernel(dy - yy);

                                           for (int xx = -1; xx < 3; xx++)
                                           {
                                               // Get X cooefficient
                                               double kernel2 = kernel1 * Interpolation.BiCubicBSplineKernel(xx - dx);

                                               Color sourceColor = sourceColors[xx + 1, yy + 1];

                                               if (fixGamma)
                                               {
                                                   sourceColor = PixelOperations.ToLinear(sourceColor);
                                               }

                                               r += kernel2 * sourceColor.R;
                                               g += kernel2 * sourceColor.G;
                                               b += kernel2 * sourceColor.B;
                                               a += kernel2 * sourceColor.A;
                                           }
                                       }

                                       Color destinationColor = Color.FromArgb(
                                           a.ToByte(),
                                           r.ToByte(),
                                           g.ToByte(),
                                           b.ToByte());

                                       if (fixGamma)
                                       {
                                           destinationColor = PixelOperations.ToSRGB(destinationColor);
                                       }

                                       destinationBitmap.SetPixel(x, y, destinationColor);
                                   }
                               }
                           }
                       });
                }
            }

            source.Dispose();
            return destination;
        }

        /// <summary>
        /// Resize an image using a bilinear interpolation algorithm.
        /// <remarks>
        /// The class implements image resizing filter using bilinear
        /// interpolation algorithms described on
        /// <see href="https://en.wikipedia.org/wiki/Bilinear_interpolation#Algorithm">Wikipedia</see>.
        /// </remarks>
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <param name="destinationRectangle">The position within the new image to place the pixels.</param>
        /// <param name="fixGamma">Whether to resize the image using the linear color space.</param>
        /// <returns>
        /// The resized <see cref="Bitmap"/>.
        /// </returns>
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "FastBitmap is incorrectly warned against.")]
        public static Bitmap ResizeBilinear(Bitmap source, int width, int height, Rectangle destinationRectangle, bool fixGamma)
        {
            int sourceWidth = source.Width;
            int sourceHeight = source.Height;
            int startX = destinationRectangle.X;
            int startY = destinationRectangle.Y;
            int endX = destinationRectangle.Width + startX;
            int endY = destinationRectangle.Height + startY;

            // Scaling factors
            double widthFactor = sourceWidth / (double)destinationRectangle.Width;
            double heightFactor = sourceHeight / (double)destinationRectangle.Height;

            // Width and height decreased by 1
            int maxHeight = sourceHeight - 1;
            int maxWidth = sourceWidth - 1;

            Bitmap destination = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            destination.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (FastBitmap sourceBitmap = new FastBitmap(source))
            {
                using (FastBitmap destinationBitmap = new FastBitmap(destination))
                {
                    // For each column
                    Parallel.For(
                        startY,
                        endY,
                        y =>
                        {
                            if (y >= 0 && y < height)
                            {
                                // Y coordinates of source points
                                double originY = (y - startY) * heightFactor;
                                int originY1 = (int)originY;
                                int originY2 = (originY1 == maxHeight) ? originY1 : originY1 + 1;
                                double dy1 = originY - originY1;
                                double dy2 = 1.0 - dy1;

                                // Get temp pointers
                                int temp1 = originY1;
                                int temp2 = originY2;

                                // For every column.
                                for (int x = startX; x < endX; x++)
                                {
                                    if (x >= 0 && x < width)
                                    {
                                        // X coordinates of source points
                                        double originX = (x - startX) * widthFactor;
                                        int originX1 = (int)originX;
                                        int originX2 = (originX1 == maxWidth) ? originX1 : originX1 + 1;
                                        double dx1 = originX - originX1;
                                        double dx2 = 1.0 - dx1;

                                        // Get four pixels to sample from.
                                        Color sourceColor1 = sourceBitmap.GetPixel(originX1, temp1);
                                        Color sourceColor2 = sourceBitmap.GetPixel(originX2, temp1);
                                        Color sourceColor3 = sourceBitmap.GetPixel(originX1, temp2);
                                        Color sourceColor4 = sourceBitmap.GetPixel(originX2, temp2);

                                        if (fixGamma)
                                        {
                                            sourceColor1 = PixelOperations.ToLinear(sourceColor1);
                                            sourceColor2 = PixelOperations.ToLinear(sourceColor2);
                                            sourceColor3 = PixelOperations.ToLinear(sourceColor3);
                                            sourceColor4 = PixelOperations.ToLinear(sourceColor4);
                                        }

                                        // Get four points in red channel.
                                        int p1 = sourceColor1.R;
                                        int p2 = sourceColor2.R;
                                        int p3 = sourceColor3.R;
                                        int p4 = sourceColor4.R;

                                        int r = (int)((dy2 * ((dx2 * p1) + (dx1 * p2))) + (dy1 * ((dx2 * p3) + (dx1 * p4))));

                                        // Get four points in green channel.
                                        p1 = sourceColor1.G;
                                        p2 = sourceColor2.G;
                                        p3 = sourceColor3.G;
                                        p4 = sourceColor4.G;

                                        int g = (int)((dy2 * ((dx2 * p1) + (dx1 * p2))) + (dy1 * ((dx2 * p3) + (dx1 * p4))));

                                        // Get four points in blue channel
                                        p1 = sourceColor1.B;
                                        p2 = sourceColor2.B;
                                        p3 = sourceColor3.B;
                                        p4 = sourceColor4.B;

                                        int b = (int)((dy2 * ((dx2 * p1) + (dx1 * p2))) + (dy1 * ((dx2 * p3) + (dx1 * p4))));

                                        // Get four points in alpha channel
                                        p1 = sourceColor1.A;
                                        p2 = sourceColor2.A;
                                        p3 = sourceColor3.A;
                                        p4 = sourceColor4.A;

                                        int a = (int)((dy2 * ((dx2 * p1) + (dx1 * p2))) + (dy1 * ((dx2 * p3) + (dx1 * p4))));

                                        Color destinationColor = Color.FromArgb(
                                            a.ToByte(),
                                            r.ToByte(),
                                            g.ToByte(),
                                            b.ToByte());

                                        if (fixGamma)
                                        {
                                            destinationColor = PixelOperations.ToSRGB(destinationColor);
                                        }

                                        destinationBitmap.SetPixel(x, y, destinationColor);
                                    }
                                }
                            }
                        });
                }
            }

            source.Dispose();
            return destination;
        }

        /// <summary>
        /// Resize an image using a nearest neighbor algorithm.
        /// <remarks>
        /// The class implements image resizing filter using the nearest neighbor algorithms described on
        /// <see href="https://en.wikipedia.org/wiki/Nearest-neighbor_interpolation">Wikipedia</see>.
        /// </remarks>
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <param name="destinationRectangle">The position within the new image to place the pixels.</param>
        /// <returns>
        /// The resized <see cref="Bitmap"/>.
        /// </returns>
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "FastBitmap is incorrectly warned against.")]
        public static Bitmap ResizeNearestNeighbor(Bitmap source, int width, int height, Rectangle destinationRectangle)
        {
            int sourceWidth = source.Width;
            int sourceHeight = source.Height;
            int startX = destinationRectangle.X;
            int startY = destinationRectangle.Y;
            int endX = destinationRectangle.Width + startX;
            int endY = destinationRectangle.Height + startY;

            // Scaling factors
            double widthFactor = sourceWidth / (double)destinationRectangle.Width;
            double heightFactor = sourceHeight / (double)destinationRectangle.Height;

            Bitmap destination = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            destination.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (FastBitmap sourceBitmap = new FastBitmap(source))
            {
                using (FastBitmap destinationBitmap = new FastBitmap(destination))
                {
                    // For each column
                    Parallel.For(
                       startY,
                       endY,
                        y =>
                        {
                            if (y >= 0 && y < height)
                            {
                                // Y coordinates of source points
                                int originY = (int)((y - startY) * heightFactor);

                                for (int x = startX; x < endX; x++)
                                {
                                    if (x >= 0 && x < width)
                                    {
                                        // X coordinates of source points
                                        int originX = (int)((x - startX) * widthFactor);

                                        destinationBitmap.SetPixel(x, y, sourceBitmap.GetPixel(originX, originY));
                                    }
                                }
                            }
                        });
                }
            }

            source.Dispose();
            return destination;
        }

        /// <summary>
        /// Resize an image using a Lanczos interpolation algorithm.
        /// <remarks>
        /// The class implements image resizing filter using Lanczos
        /// interpolation algorithms described on
        /// <see href="https://en.wikipedia.org/wiki/Lanczos_resampling#Algorithm">Wikipedia</see>.
        /// Hidden just now as I'm not sure I have it correct.
        /// </remarks>
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <param name="destinationRectangle">The position within the new image to place the pixels.</param>
        /// <param name="fixGamma">Whether to resize the image using the linear color space.</param>
        /// <returns>
        /// The resized <see cref="Bitmap"/>.
        /// </returns>
        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "FastBitmap is incorrectly warned against.")]
        internal static Bitmap ResizeLanczos(Bitmap source, int width, int height, Rectangle destinationRectangle, bool fixGamma = true)
        {
            int sourceWidth = source.Width;
            int sourceHeight = source.Height;
            int startX = destinationRectangle.X;
            int startY = destinationRectangle.Y;
            int endX = destinationRectangle.Width + startX;
            int endY = destinationRectangle.Height + startY;

            Bitmap destination = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            destination.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (FastBitmap sourceBitmap = new FastBitmap(source))
            {
                using (FastBitmap destinationBitmap = new FastBitmap(destination))
                {
                    // Scaling factors
                    double widthFactor = sourceWidth / (double)destinationRectangle.Width;
                    double heightFactor = sourceHeight / (double)destinationRectangle.Height;

                    // Width and height decreased by 1
                    int maxHeight = sourceHeight - 1;
                    int maxWidth = sourceWidth - 1;

                    // For each column
                    Parallel.For(
                       startY,
                       endY,
                        y =>
                        {
                            if (y >= 0 && y < height)
                            {
                                // Y coordinates of source points.
                                double originY = ((y - startY) * heightFactor) - 0.5;
                                int originY1 = (int)originY;
                                double dy = originY - originY1;

                                // For each row.
                                for (int x = startX; x < endX; x++)
                                {
                                    if (x >= 0 && x < width)
                                    {
                                        // X coordinates of source points.
                                        double originX = ((x - startX) * widthFactor) - 0.5f;
                                        int originX1 = (int)originX;
                                        double dx = originX - originX1;

                                        // Destination color components
                                        double r = 0;
                                        double g = 0;
                                        double b = 0;
                                        double a = 0;

                                        for (int n = -3; n < 6; n++)
                                        {
                                            // Get Y cooefficient
                                            double k1 = Interpolation.LanczosKernel3(dy - n);

                                            int originY2 = originY1 + n;
                                            if (originY2 < 0)
                                            {
                                                originY2 = 0;
                                            }

                                            if (originY2 > maxHeight)
                                            {
                                                originY2 = maxHeight;
                                            }

                                            for (int m = -3; m < 6; m++)
                                            {
                                                // Get X cooefficient
                                                double k2 = k1 * Interpolation.LanczosKernel3(m - dx);

                                                int originX2 = originX1 + m;
                                                if (originX2 < 0)
                                                {
                                                    originX2 = 0;
                                                }

                                                if (originX2 > maxWidth)
                                                {
                                                    originX2 = maxWidth;
                                                }

                                                // ReSharper disable once AccessToDisposedClosure
                                                Color sourceColor = sourceBitmap.GetPixel(originX2, originY2);

                                                if (fixGamma)
                                                {
                                                    sourceColor = PixelOperations.ToLinear(sourceColor);
                                                }

                                                r += k2 * sourceColor.R;
                                                g += k2 * sourceColor.G;
                                                b += k2 * sourceColor.B;
                                                a += k2 * sourceColor.A;
                                            }
                                        }

                                        Color destinationColor = Color.FromArgb(
                                            a.ToByte(),
                                            r.ToByte(),
                                            g.ToByte(),
                                            b.ToByte());

                                        if (fixGamma)
                                        {
                                            destinationColor = PixelOperations.ToSRGB(destinationColor);
                                        }

                                        // ReSharper disable once AccessToDisposedClosure
                                        destinationBitmap.SetPixel(x, y, destinationColor);
                                    }
                                }
                            }
                        });
                }
            }

            source.Dispose();
            return destination;
        }

        /// <summary>
        /// Performs a two dimensional fast box blur on the collection of colors.
        /// <see href="http://blog.ivank.net/fastest-gaussian-blur.html"/>
        /// </summary>
        /// <param name="sourceColors">The source colors.</param>
        /// <param name="radius">The radius to which to blur.</param>
        /// <param name="fixGamma">Whether to blur the colors using the linear color space.</param>
        /// <returns>
        /// The <see cref="T:Color[,]"/>.
        /// </returns>
        private static Color[,] BoxBlur(Color[,] sourceColors, int radius, bool fixGamma)
        {
            sourceColors = BoxBlurHorizontal(sourceColors, radius, fixGamma);
            sourceColors = BoxBlurVertical(sourceColors, radius, fixGamma);
            return sourceColors;
        }

        /// <summary>
        /// Performs a horizontal fast box blur on the collection of colors.
        /// <see href="http://blog.ivank.net/fastest-gaussian-blur.html"/>
        /// </summary>
        /// <param name="sourceColors">The source colors.</param>
        /// <param name="radius">The radius to which to blur.</param>
        /// <param name="fixGamma">Whether to blur the colors using the linear color space.</param>
        /// <returns>
        /// The <see cref="T:Color[,]"/>.
        /// </returns>
        private static Color[,] BoxBlurHorizontal(Color[,] sourceColors, int radius, bool fixGamma)
        {
            int width = sourceColors.GetLength(0);
            int height = sourceColors.GetLength(1);

            Color[,] destination = new Color[width, height];

            // For each column
            for (int y = 0; y < height; y++)
            {
                // For each row
                for (int x = 0; x < width; x++)
                {
                    int fx = Math.Max(0, x - radius);
                    int tx = Math.Min(width, x + radius + 1);
                    int red = 0;
                    int green = 0;
                    int blue = 0;
                    int alpha = 0;

                    for (int xx = fx; xx < tx; xx++)
                    {
                        Color sourceColor = sourceColors[y, xx];

                        if (fixGamma)
                        {
                            sourceColor = PixelOperations.ToLinear(sourceColor);
                        }

                        red += sourceColor.R;
                        green += sourceColor.G;
                        blue += sourceColor.B;
                        alpha += sourceColor.A;
                    }

                    int divider = tx - fx;

                    red /= divider;
                    green /= divider;
                    blue /= divider;
                    alpha /= divider;

                    Color destinationColor = Color.FromArgb(alpha, red, green, blue);

                    if (fixGamma)
                    {
                        destinationColor = PixelOperations.ToSRGB(destinationColor);
                    }

                    destination[x, y] = destinationColor;
                }
            }

            return destination;
        }

        /// <summary>
        /// Performs a vertical fast box blur on the collection of colors.
        /// <see href="http://blog.ivank.net/fastest-gaussian-blur.html"/>
        /// </summary>
        /// <param name="sourceColors">The source colors.</param>
        /// <param name="radius">The radius to which to blur.</param>
        /// <param name="fixGamma">Whether to blur the colors using the linear color space.</param>
        /// <returns>
        /// The <see cref="T:Color[,]"/>.
        /// </returns>
        private static Color[,] BoxBlurVertical(Color[,] sourceColors, int radius, bool fixGamma)
        {
            int width = sourceColors.GetLength(0);
            int height = sourceColors.GetLength(1);

            Color[,] destination = new Color[width, height];

            // For each column
            for (int y = 0; y < height; y++)
            {
                // For each row
                for (int x = 0; x < width; x++)
                {
                    int fy = Math.Max(0, y - radius);
                    int ty = Math.Min(width, y + radius + 1);
                    int red = 0;
                    int green = 0;
                    int blue = 0;
                    int alpha = 0;

                    for (int yy = fy; yy < ty; yy++)
                    {
                        Color sourceColor = sourceColors[x, yy];

                        if (fixGamma)
                        {
                            sourceColor = PixelOperations.ToLinear(sourceColor);
                        }

                        red += sourceColor.R;
                        green += sourceColor.G;
                        blue += sourceColor.B;
                        alpha += sourceColor.A;
                    }

                    int divider = ty - fy;

                    red /= divider;
                    green /= divider;
                    blue /= divider;
                    alpha /= divider;

                    Color destinationColor = Color.FromArgb(alpha, red, green, blue);

                    if (fixGamma)
                    {
                        destinationColor = PixelOperations.ToSRGB(destinationColor);
                    }

                    destination[x, y] = destinationColor;
                }
            }

            return destination;
        }
    }
}
