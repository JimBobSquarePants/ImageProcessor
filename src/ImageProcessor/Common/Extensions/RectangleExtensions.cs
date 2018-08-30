// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RectangleExtensions.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Extensions to the rectangle structure.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Common.Extensions
{
    using System;
    using System.Drawing;

    /// <summary>
    /// Extensions to the rectangle structure.
    /// </summary>
    internal static class RectangleExtensions
    {
        /// <summary>
        /// Compares two rectangles for equality, considering an acceptance threshold.
        /// </summary>
        /// <param name="first">The first rectangle.</param>
        /// <param name="second">The second rectangle</param>
        /// <param name="threshold">The threshold.</param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsEqual(this Rectangle first, Rectangle second, int threshold)
        {
            return (Math.Abs(first.X - second.X) < threshold)
                   && (Math.Abs(first.Y - second.Y) < threshold)
                   && (Math.Abs(first.Width - second.Width) < threshold)
                   && (Math.Abs(first.Height - second.Height) < threshold);
        }
    }
}
