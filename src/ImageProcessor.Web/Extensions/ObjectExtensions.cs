// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectExtensions.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Extensions methods for <see cref="object" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Extensions
{
    /// <summary>
    /// Extensions methods for <see cref="object"/>.
    /// </summary>
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="object"/> is null or an empty <see cref="string"/>.
        /// </summary>
        /// <param name="value">The object to test against.</param>
        /// <returns>True; if the value is null or an empty string; otherwise; false.</returns>
        public static bool IsNullOrEmptyString(this object value)
        {
            return value == null || value as string == string.Empty;
        }
    }
}
