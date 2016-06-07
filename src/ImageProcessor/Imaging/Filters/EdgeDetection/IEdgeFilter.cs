// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEdgeFilter.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Describes properties for creating edge detection filters.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Filters.EdgeDetection
{
    /// <summary>
    /// Describes properties for creating edge detection filters.
    /// </summary>
    public interface IEdgeFilter
    {
        /// <summary>
        /// Gets the horizontal gradient operator.
        /// </summary>
        double[,] HorizontalGradientOperator { get; }
    }
}
