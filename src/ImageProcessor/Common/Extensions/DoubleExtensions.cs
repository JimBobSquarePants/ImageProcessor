// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace ImageProcessor
{
    /// <summary>
    /// Encapsulates a series of time saving extension methods to the <see cref="T:System.Double"/> class.
    /// </summary>
    public static class DoubleExtensions
    {
        /// <summary>
        /// Converts an <see cref="double"/> value into a valid <see cref="byte"/>.
        /// <remarks>
        /// If the value given is less than 0 or greater than 255, the value will be constrained into
        /// those restricted ranges.
        /// </remarks>
        /// </summary>
        /// <param name="value">The <see cref="double"/> to convert.</param>
        /// <returns>The <see cref="byte"/>.</returns>
        public static byte ToByte(this double value) => Convert.ToByte(NumberUtilities.Clamp(value, 0, 255));
    }
}
