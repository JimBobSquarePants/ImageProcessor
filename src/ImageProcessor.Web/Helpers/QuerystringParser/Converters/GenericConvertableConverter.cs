// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericConvertableConverter.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The generic converter for simple types.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Helpers
{
    using System;
    using System.Globalization;

    /// <summary>
    /// The generic converter for simple types.
    /// </summary>
    /// <typeparam name="T">
    /// The type of object to convert.
    /// </typeparam>
    public class GenericConvertableConverter<T> : QueryParamConverter
        where T : IConvertible
    {
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
                Type t = typeof(T);
                Type u = Nullable.GetUnderlyingType(t);

                if (u != null)
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    return (value == null) ? default(T) : (T)Convert.ChangeType(value, u);
                }

                return (T)Convert.ChangeType(value, t);
            }

            return base.ConvertFrom(culture, value, propertyType);
        }
    }
}
