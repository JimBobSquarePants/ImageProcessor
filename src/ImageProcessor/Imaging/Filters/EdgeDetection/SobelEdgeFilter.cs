// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SobelEdgeFilter.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The Sobel operator filter.
//   <see href="http://en.wikipedia.org/wiki/Sobel_operator" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Filters.EdgeDetection
{
    /// <summary>
    /// The Sobel operator filter.
    /// <see href="http://en.wikipedia.org/wiki/Sobel_operator"/>
    /// </summary>
    public class SobelEdgeFilter : I2DEdgeFilter
    {
        /// <summary>
        /// Gets the horizontal gradient operator.
        /// </summary>
        public double[,] HorizontalGradientOperator => new double[,]
        {
            { -1, 0, 1 },
            { -2, 0, 2 },
            { -1, 0, 1 }
        };

        /// <summary>
        /// Gets the vertical gradient operator.
        /// </summary>
        public double[,] VerticalGradientOperator => new double[,]
        {
            { 1, 2, 1 },
            { 0, 0, 0 },
            { -1, -2, -1 }
        };
    }
}
