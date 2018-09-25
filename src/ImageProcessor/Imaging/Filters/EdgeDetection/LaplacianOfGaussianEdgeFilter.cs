// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LaplacianOfGaussianEdgeFilter.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The Laplacian of Gaussian operator filter.
//   <see href="http://fourier.eng.hmc.edu/e161/lectures/gradient/node9.html" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Filters.EdgeDetection
{
    /// <summary>
    /// The Laplacian of Gaussian operator filter.
    /// <see href="http://fourier.eng.hmc.edu/e161/lectures/gradient/node9.html"/>
    /// </summary>
    public class LaplacianOfGaussianEdgeFilter : IEdgeFilter
    {
        /// <summary>
        /// Gets the horizontal gradient operator.
        /// </summary>
        public double[,] HorizontalGradientOperator => new double[,]
        {
            { 0, 0, -1,  0,  0 },
            { 0, -1, -2, -1,  0 },
            { -1, -2, 16, -2, -1 },
            { 0, -1, -2, -1,  0 },
            { 0, 0, -1,  0,  0 }
        };
    }
}
