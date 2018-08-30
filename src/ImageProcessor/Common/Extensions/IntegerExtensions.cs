// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IntegerExtensions.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates a series of time saving extension methods to the <see cref="T:System.Int32" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Common.Extensions
{
    using System;

    using ImageProcessor.Imaging.Helpers;

    /// <summary>
    /// Encapsulates a series of time saving extension methods to the <see cref="T:System.Int32"/> class.
    /// </summary>
    public static class IntegerExtensions
    {
        /// <summary>
        /// Converts an <see cref="T:System.Int32"/> value into a valid <see cref="T:System.Byte"/>.
        /// <remarks>
        /// If the value given is less than 0 or greater than 255, the value will be constrained into
        /// those restricted ranges.
        /// </remarks>
        /// </summary>
        /// <param name="value">
        /// The <see cref="T:System.Int32"/> to convert.
        /// </param>
        /// <returns>
        /// The <see cref="T:System.Byte"/>.
        /// </returns>
        public static byte ToByte(this int value) => Convert.ToByte(ImageMaths.Clamp(value, 0, 255));
    }
}
