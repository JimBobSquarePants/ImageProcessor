// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Detects edges within an image using Scharr operators.
    /// <see href="http://en.wikipedia.org/wiki/Sobel_operator#Alternative_operators"/>.
    /// </summary>
    public class Scharr : EdgeDetection2DProcessor
    {
        private static readonly double[,] KernelX = new double[,]
        {
            { -3, 0, 3 },
            { -10, 0, 10 },
            { -3, 0, 3 }
        };

        private static readonly double[,] KernelY = new double[,]
        {
            { 3, 10, 3 },
            { 0, 0, 0 },
            { -3, -10, -3 }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="Scharr"/> class.
        /// </summary>
        /// <param name="grayscale">Whether to convert the image to grascale before processing.</param>
        public Scharr(bool grayscale)
            : base(new KernelPair(KernelX, KernelY), grayscale)
        {
        }
    }
}
