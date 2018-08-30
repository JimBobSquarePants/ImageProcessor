// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinaryThreshold.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Performs binary threshold filtering against a given greyscale image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Filters.Binarization
{
    using System.Drawing;
    using System.Threading.Tasks;

    /// <summary>
    /// Performs binary threshold filtering against a given greyscale image.
    /// </summary>
    public class BinaryThreshold
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryThreshold"/> class.
        /// </summary>
        /// <param name="threshold">
        /// The threshold.
        /// </param>
        public BinaryThreshold(byte threshold = 10) => this.Threshold = threshold;

        /// <summary>
        /// Gets or sets the threshold.
        /// </summary>
        public byte Threshold { get; set; }

        /// <summary>
        /// Processes the given bitmap to apply the threshold.
        /// </summary>
        /// <param name="source">
        /// The image to process.
        /// </param>
        /// <returns>
        /// A processed bitmap.
        /// </returns>
        public Bitmap ProcessFilter(Bitmap source)
        {
            int width = source.Width;
            int height = source.Height;

            using (var sourceBitmap = new FastBitmap(source))
            {
                Parallel.For(
                    0,
                    height,
                    y =>
                    {
                        for (int x = 0; x < width; x++)
                        {
                            // ReSharper disable AccessToDisposedClosure
                            Color color = sourceBitmap.GetPixel(x, y);
                            sourceBitmap.SetPixel(x, y, color.B >= this.Threshold ? Color.White : Color.Black);

                            // ReSharper restore AccessToDisposedClosure
                        }
                    });
            }

            return source;
        }
    }
}
