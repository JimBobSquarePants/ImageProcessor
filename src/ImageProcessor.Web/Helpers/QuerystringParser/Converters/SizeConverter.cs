// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SizeConverter.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The size converter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Helpers
{
    using System;
    using System.Drawing;
    using System.Globalization;

    /// <summary>
    /// The size converter.
    /// </summary>
    public class SizeConverter : GenericArrayTypeConverter<int>
    {
        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture 
        /// information.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object"/> that represents the converted value.
        /// </returns>
        /// <param name="culture">
        /// The <see cref="T:System.Globalization.CultureInfo"/> to use as the current culture. 
        /// </param>
        /// <param name="value">The <see cref="T:System.Object"/> to convert. </param>
        /// <param name="propertyType">The property type that the converter will convert to.</param>
        /// <exception cref="T:System.NotSupportedException">The conversion cannot be performed.</exception>
        public override object ConvertFrom(CultureInfo culture, object value, Type propertyType)
        {
            object result = base.ConvertFrom(culture, value, propertyType);

            return result is int[] list && list.Length == 2 ? new Size(list[0], list[1]) : result;
        }
    }
}
