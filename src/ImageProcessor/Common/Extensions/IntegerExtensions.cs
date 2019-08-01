// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

namespace ImageProcessor
{
    /// <summary>
    /// Encapsulates a series of time saving extension methods to the <see cref="T:System.Int32"/> class.
    /// </summary>
    public static class IntegerExtensions
    {
        /// <summary>
        /// Converts an <see cref="int"/> value into a valid <see cref="byte"/>.
        /// <remarks>
        /// If the value given is less than 0 or greater than 255, the value will be constrained into
        /// those restricted ranges.
        /// </remarks>
        /// </summary>
        /// <param name="value">The <see cref="T:System.Int32"/> to convert.</param>
        /// <returns>The <see cref="byte"/>.</returns>
        public static byte ToByte(this int value) => (byte)NumberUtilities.Clamp(value, 0, 255);
    }
}
