// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Detects edges within an image using Laplacian 3x3 operators.
    /// <see href="http://en.wikipedia.org/wiki/Discrete_Laplace_operator"/>.
    /// </summary>
    public class Laplacian3x3 : EdgeDetectionProcessor
    {
        private static readonly double[,] KernelXY = new double[,]
        {
            { -1, -1, -1 },
            { -1,  8, -1 },
            { -1, -1, -1 }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="Laplacian3x3"/> class.
        /// </summary>
        /// <param name="grayscale">Whether to convert the image to grascale before processing.</param>
        public Laplacian3x3(bool grayscale)
            : base(KernelXY, grayscale)
        {
        }
    }
}
