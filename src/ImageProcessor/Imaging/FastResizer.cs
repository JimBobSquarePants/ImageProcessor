

namespace ImageProcessor.Imaging
{
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Threading.Tasks;

    using ImageProcessor.Common.Extensions;
    using ImageProcessor.Imaging.Helpers;

    /// <summary>
    /// Resizes images out with <see cref="System.Drawing"/> using well known algorithms.
    /// This allows us to work in parallel and avoid locking issues caused by GDI+
    /// Based on work by the AForge.NET framework <see href="https://github.com/cureos/aforge"/>.
    /// </summary>
    public static class FastResizer
    {
        /// <summary>
        /// Resize an image using a bicubic interpolation algorithm.
        /// <remarks>
        /// The class implements image resizing filter using bicubic
        /// interpolation algorithm. It uses bicubic kernel W(x) as described on
        /// <see href="http://en.wikipedia.org/wiki/Bicubic_interpolation#Bicubic_convolution_algorithm">Wikipedia</see>
        /// (coefficient <b>a</b> is set to <b>-0.5</b>).
        /// </remarks>
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <param name="destinationRectangle">The position within the new image to place the pixels.</param>
        /// <param name="fixGamma">
        /// Whether to resize the image using the linear color space.
        /// </param>
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
                           // Y coordinates of source points.
                           double oy = ((y - startY) * heightFactor) - 0.5;
                           int oy1 = (int)oy;
                           double dy = oy - oy1;

                           // For each row.
                           for (int x = startX; x < endX; x++)
                           {
                               // X coordinates of source points.
                               double ox = ((x - startX) * widthFactor) - 0.5f;
                               int ox1 = (int)ox;
                               double dx = ox - ox1;

                               // Destination color components
                               double r = 0;
                               double g = 0;
                               double b = 0;
                               double a = 0;

                               for (int n = -1; n < 3; n++)
                               {
                                   // Get Y cooefficient
                                   double k1 = Interpolation.BiCubicKernel(dy - n);

                                   int oy2 = oy1 + n;
                                   if (oy2 < 0)
                                   {
                                       oy2 = 0;
                                   }

                                   if (oy2 > maxHeight)
                                   {
                                       oy2 = maxHeight;
                                   }

                                   for (int m = -1; m < 3; m++)
                                   {
                                       // Get X cooefficient
                                       double k2 = k1 * Interpolation.BiCubicKernel(m - dx);

                                       int ox2 = ox1 + m;
                                       if (ox2 < 0)
                                       {
                                           ox2 = 0;
                                       }

                                       if (ox2 > maxWidth)
                                       {
                                           ox2 = maxWidth;
                                       }

                                       Color sourceColor = sourceBitmap.GetPixel(ox2, oy2);

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

                               Color destinationColor = Color.FromArgb(a.ToByte(), r.ToByte(), g.ToByte(), b.ToByte());

                               if (fixGamma)
                               {
                                   destinationColor = PixelOperations.ToSRGB(destinationColor);
                               }

                               destinationBitmap.SetPixel(x, y, destinationColor);
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
        /// <param name="fixGamma">
        /// Whether to resize the image using the linear color space.
        /// </param>
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
                           // Y coordinates of source points
                           double oy = (y - startY) * heightFactor;
                           int oy1 = (int)oy;
                           int oy2 = (oy1 == maxHeight) ? oy1 : oy1 + 1;
                           double dy1 = oy - oy1;
                           double dy2 = 1.0 - dy1;

                           // Get temp pointers
                           int tp1 = oy1;
                           int tp2 = oy2;

                           // For every column.
                           for (int x = startX; x < endX; x++)
                           {
                               // X coordinates of source points
                               double ox = (x - startX) * widthFactor;
                               int ox1 = (int)ox;
                               int ox2 = (ox1 == maxWidth) ? ox1 : ox1 + 1;
                               double dx1 = ox - ox1;
                               double dx2 = 1.0 - dx1;

                               // Get four pixels to sample from.
                               Color sourceColor1 = sourceBitmap.GetPixel(ox1, tp1);
                               Color sourceColor2 = sourceBitmap.GetPixel(ox2, tp1);
                               Color sourceColor3 = sourceBitmap.GetPixel(ox1, tp2);
                               Color sourceColor4 = sourceBitmap.GetPixel(ox2, tp2);

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

                               Color destinationColor = Color.FromArgb(a.ToByte(), r.ToByte(), g.ToByte(), b.ToByte());

                               if (fixGamma)
                               {
                                   destinationColor = PixelOperations.ToSRGB(destinationColor);
                               }

                               destinationBitmap.SetPixel(x, y, destinationColor);
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
        /// <param name="fixGamma">
        /// Whether to resize the image using the linear color space.
        /// </param>
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
                           // Y coordinates of source points.
                           double oy = ((y - startY) * heightFactor) - 0.5;
                           int oy1 = (int)oy;
                           double dy = oy - oy1;

                           // For each row.
                           for (int x = startX; x < endX; x++)
                           {
                               // X coordinates of source points.
                               double ox = ((x - startX) * widthFactor) - 0.5f;
                               int ox1 = (int)ox;
                               double dx = ox - ox1;

                               // Destination color components
                               double r = 0;
                               double g = 0;
                               double b = 0;
                               double a = 0;

                               for (int n = -3; n < 6; n++)
                               {
                                   // Get Y cooefficient
                                   double k1 = Interpolation.LanczosKernel(dy - n);

                                   int oy2 = oy1 + n;
                                   if (oy2 < 0)
                                   {
                                       oy2 = 0;
                                   }

                                   if (oy2 > maxHeight)
                                   {
                                       oy2 = maxHeight;
                                   }

                                   for (int m = -3; m < 6; m++)
                                   {
                                       // Get X cooefficient
                                       double k2 = k1 * Interpolation.LanczosKernel(m - dx);

                                       int ox2 = ox1 + m;
                                       if (ox2 < 0)
                                       {
                                           ox2 = 0;
                                       }

                                       if (ox2 > maxWidth)
                                       {
                                           ox2 = maxWidth;
                                       }

                                       // ReSharper disable once AccessToDisposedClosure
                                       Color sourceColor = sourceBitmap.GetPixel(ox2, oy2);

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

                               Color destinationColor = Color.FromArgb(a.ToByte(), r.ToByte(), g.ToByte(), b.ToByte());

                               if (fixGamma)
                               {
                                   destinationColor = PixelOperations.ToSRGB(destinationColor);
                               }

                               // ReSharper disable once AccessToDisposedClosure
                               destinationBitmap.SetPixel(x, y, destinationColor);
                           }
                       });
                }
            }

            source.Dispose();
            return destination;
        }
    }
}
