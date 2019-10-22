// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Detects edges within an image using Kayyali operators.
    /// <see href="https://edgedetection.webs.com/"/>.
    /// </summary>
    public class Kayyali : EdgeDetection2DProcessor
    {
        private static readonly double[,] KernelX = new double[,]
        {
            { 6, 0, -6 },
            { 0, 0, 0 },
            { -6, 0, 6 }
        };

        private static readonly double[,] KernelY = new double[,]
        {
            { -6, 0, 6 },
            { 0, 0, 0 },
            { 6, 0, -6 }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="Kayyali"/> class.
        /// </summary>
        /// <param name="grayscale">Whether to convert the image to grascale before processing.</param>
        public Kayyali(bool grayscale)
            : base(new KernelPair(KernelX, KernelY), grayscale)
        {
        }
    }
}
