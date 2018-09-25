// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Interpolation.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides interpolation routines for resampling algorithms.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging
{
    using System;

    /// <summary>
    /// Provides interpolation routines for resampling algorithms.
    /// </summary>
    internal static class Interpolation
    {
        /// <summary>
        /// Returns a bicubic kernel for the given value.
        /// <remarks>
        /// The function implements bicubic kernel W(x) as described on
        /// <see href="https://en.wikipedia.org/wiki/Lanczos_resampling#Algorithm">Wikipedia</see>
        /// (coefficient <c>a</c> is set to <c>-0.5</c>).
        /// </remarks>
        /// </summary>
        /// <param name="x">X value.</param> 
        /// <returns>
        /// The <see cref="double"/> representing the bicubic coefficient.
        /// </returns>
        public static double BiCubicKernel(double x)
        {
            // The coefficient.
            const double a = -0.5;

            if (x < 0)
            {
                x = -x;
            }

            if (x <= 1)
            {
                return (((1.5 * x) - 2.5) * x * x) + 1;
            }
            else if (x < 2)
            {
                return (((((a * x) + 2.5) * x) - 4) * x) + 2;
            }

            return 0;
        }

        /// <summary>
        /// Returns a bicubic b-spline kernel for the given value.
        /// <remarks>
        /// The function implements bicubic kernel developed by Paul Bourke <see href="http://paulbourke.net"/> 
        /// described <see href="http://docs-hoffmann.de/bicubic03042002.pdf">here</see>
        /// </remarks>
        /// </summary>
        /// <param name="x">X value.</param> 
        /// <returns>
        /// The <see cref="double"/> representing the bicubic coefficient.
        /// </returns>
        public static double BiCubicBSplineKernel(double x)
        {
            double r = 0;
            double xplus2 = x + 2;
            double xplus1 = x + 1;
            double xminus1 = x - 1;

            if (xplus2 > 0)
            {
                r += xplus2 * xplus2 * xplus2;
            }

            if (xplus1 > 0)
            {
                r -= 4 * xplus1 * xplus1 * xplus1;
            }

            if (x > 0)
            {
                r += 6 * x * x * x;
            }

            if (xminus1 > 0)
            {
                r -= 4 * xminus1 * xminus1 * xminus1;
            }

            return r / 6.0;
        }

        /// <summary>
        /// Returns a Lanczos kernel for the given value.
        /// <remarks>
        /// The function implements Lanczos kernel as described on
        /// <see href="https://en.wikipedia.org/wiki/Lanczos_resampling#Algorithm">Wikipedia</see>
        /// </remarks>
        /// </summary>
        /// <param name="x">The value to return the kernel for.</param> 
        /// <returns>
        /// The <see cref="double"/> representing the bicubic coefficient.
        /// </returns>
        internal static double LanczosKernel3(double x)
        {
            if (x < 0)
            {
                x = -x;
            }

            if (x < 3)
            {
                return SinC(x) * SinC(x / 3f);
            }

            return 0;
        }

        /// <summary>
        /// Gets the result of a sine cardinal function for the given value.
        /// </summary>
        /// <param name="x">
        /// The value to calculate the result for.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        private static double SinC(double x)
        {
            const double Epsilon = .0001;

            if (Math.Abs(x) > Epsilon)
            {
                x *= Math.PI;
                return Clean(Math.Sin(x) / x);
            }

            return 1.0;
        }

        /// <summary>
        /// Ensures that any passed double is correctly rounded to zero
        /// </summary>
        /// <param name="x">The value to clean.</param>
        /// <returns>The <see cref="double"/>.</returns>
        private static double Clean(double x)
        {
            const double Epsilon = .0001;

            if (Math.Abs(x) < Epsilon)
            {
                return 0.0;
            }

            return x;
        }
    }
}
