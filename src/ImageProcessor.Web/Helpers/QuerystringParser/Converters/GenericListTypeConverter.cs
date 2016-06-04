// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericListTypeConverter.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Converts the value of an string to and from a List{T}.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Converts the value of an string to and from a List{T}.
    /// </summary>
    /// <typeparam name="T">
    /// The type to convert from.
    /// </typeparam>
    public class GenericListTypeConverter<T> : QueryParamConverter
    {
        /// <summary>
        /// The type converter.
        /// </summary>
        private readonly IQueryParamConverter typeConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericListTypeConverter{T}"/> class.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no converter exists for the given type.
        /// </exception>
        public GenericListTypeConverter()
        {
            Type type = typeof(T);
            this.typeConverter = QueryTypeDescriptor.GetConverter(type);
            if (this.typeConverter == null)
            {
                throw new InvalidOperationException("No type converter exists for type " + type.FullName);
            }
        }

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
        public override bool CanConvertFrom(Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(sourceType);
        }

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
        public override object ConvertFrom(CultureInfo culture, object value, Type propertyType)
        {
            string input = value as string;
            if (input != null)
            {
                string[] items = this.GetStringArray(input, culture);

                List<T> result = new List<T>();

                Array.ForEach(
                    items,
                    s =>
                    {
                        object item = this.typeConverter.ConvertFromInvariantString(s, propertyType);
                        if (item != null)
                        {
                            result.Add((T)item);
                        }
                    });

                return result;
            }

            return base.ConvertFrom(culture, value, propertyType);
        }

        /// <summary>
        /// Converts the given value object to the specified type, using the specified context and culture 
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
        public override object ConvertTo(CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (culture == null)
                {
                    culture = CultureInfo.CurrentCulture;
                }

                string separator = culture.TextInfo.ListSeparator;
                return string.Join(separator, (IList<T>)value);
            }

            return base.ConvertTo(culture, value, destinationType);
        }

        /// <summary>
        /// Splits a string by comma to return an array of string values.
        /// </summary>
        /// <param name="input">
        /// The input string to split.
        /// </param>
        /// <param name="culture">
        /// A <see cref="T:System.Globalization.CultureInfo"/>. The current culture to split string by. 
        /// </param>
        /// <returns>
        /// The <see cref="string"/> array from the comma separated values.
        /// </returns>
        protected string[] GetStringArray(string input, CultureInfo culture)
        {
            if (culture == null)
            {
                culture = CultureInfo.CurrentCulture;
            }

            char separator = culture.TextInfo.ListSeparator[0];
            string[] result = input.Split(separator).Select(s => s.Trim()).ToArray();

            return result;
        }
    }
}
