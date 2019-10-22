// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Detects edges within an image using Laplacian of Gaussian operators.
    /// <see href="http://fourier.eng.hmc.edu/e161/lectures/gradient/node9.html"/>.
    /// </summary>
    public class LaplacianOfGaussian : EdgeDetectionProcessor
    {
        private static readonly double[,] KernelXY = new double[,]
        {
            { 0, 0, -1,  0,  0 },
            { 0, -1, -2, -1,  0 },
            { -1, -2, 16, -2, -1 },
            { 0, -1, -2, -1,  0 },
            { 0, 0, -1,  0,  0 }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="LaplacianOfGaussian"/> class.
        /// </summary>
        /// <param name="grayscale">Whether to convert the image to grascale before processing.</param>
        public LaplacianOfGaussian(bool grayscale)
            : base(KernelXY, grayscale)
        {
        }
    }
}
