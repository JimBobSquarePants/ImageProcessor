// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Detects edges within an image using Prewitt operators.
    /// <see href="http://en.wikipedia.org/wiki/Prewitt_operator"/>.
    /// </summary>
    public class Prewitt : EdgeDetection2DProcessor
    {
        private static readonly double[,] KernelX = new double[,]
        {
            { -1, 0, 1 },
            { -1, 0, 1 },
            { -1, 0, 1 }
        };

        private static readonly double[,] KernelY = new double[,]
        {
            { 1, 1, 1 },
            { 0, 0, 0 },
            { -1, -1, -1 }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="Prewitt"/> class.
        /// </summary>
        /// <param name="grayscale">Whether to convert the image to grascale before processing.</param>
        public Prewitt(bool grayscale)
            : base(new KernelPair(KernelX, KernelY), grayscale)
        {
        }
    }
}
