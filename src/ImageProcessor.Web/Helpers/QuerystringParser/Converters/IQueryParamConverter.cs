// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IQueryParamConverter.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates properties and methods for converting to object to and from querystring parameters.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Helpers
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Encapsulates properties and methods for converting to object to and from querystring parameters.
    /// </summary>
    public interface IQueryParamConverter
    {
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of this converter, 
        /// using the specified context.
        /// </summary>
        /// <returns>
        /// true if this converter can perform the conversion; otherwise, false.
        /// </returns>
        /// <param name="sourceType">
        /// A <see cref="T:System.Type"/> that represents the type you want to convert from. 
        /// </param>
        bool CanConvertFrom(Type sourceType);

        /// <summary>
        /// Gets a value indicating whether this converter can convert an object to the given destination type.
        /// </summary>
        /// <param name="destinationType">The destination type.</param>
        /// <returns>
        /// true if this converter can perform the conversion; otherwise, false.
        /// </returns>
        bool CanConvertTo(Type destinationType);

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified culture 
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
        object ConvertFrom(CultureInfo culture, object value, Type propertyType);

        /// <summary>
        /// Converts the given value object to the specified type, using the specified culture 
        /// information.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object"/> that represents the converted value.
        /// </returns>
        /// <param name="culture">
        /// A <see cref="T:System.Globalization.CultureInfo"/>. If null is passed, the current culture is assumed. 
        /// </param>
        /// <param name="value">The <see cref="T:System.Object"/> to convert. </param>
        /// <param name="destinationType">
        /// The <see cref="T:System.Type"/> to convert the <paramref name="value"/> parameter to. 
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="destinationType"/> parameter is null. 
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. 
        /// </exception>
        object ConvertTo(CultureInfo culture, object value, Type destinationType);

        /// <summary>
        /// Converts the given string to the converter's native type using the invariant culture.
        /// </summary>
        /// <param name="text">The value to convert from.</param>
        /// <param name="propertyType">The property type that the converter will convert to.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> that represents the converted value.
        /// </returns>
        object ConvertFromInvariantString(string text, Type propertyType);
    }
}
