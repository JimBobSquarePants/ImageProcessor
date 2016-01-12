// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericArrayTypeConverter.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Converts the value of an string to and from a Array{T}.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Converts the value of an string to and from a Array{T}.
    /// </summary>
    /// <typeparam name="T">
    /// The type to convert from.
    /// </typeparam>
    public class GenericArrayTypeConverter<T> : GenericListTypeConverter<T>
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
            IList<T> list = result as IList<T>;
            return list != null ? list.ToArray() : result;
        }
    }
}
