// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryParamConverter.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Converts the value of an object into a different data type.
//   <remarks>The code here is adapted from the TypeConverter class
//   <see href="http://referencesource.microsoft.com/#System/compmod/system/componentmodel/TypeConverter.cs" /></remarks>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Helpers
{
    using System;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;

    /// <summary>
    /// Converts the value of an object into a different data type.
    /// <remarks>The code here is adapted from the TypeConverter class 
    /// <see href="http://referencesource.microsoft.com/#System/compmod/system/componentmodel/TypeConverter.cs"/></remarks>
    /// </summary>
    public class QueryParamConverter : IQueryParamConverter
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
        public virtual bool CanConvertFrom(Type sourceType)
        {
            if (sourceType == typeof(InstanceDescriptor))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a value indicating whether this converter can convert an object to the given destination type.
        /// </summary>
        /// <param name="destinationType">The destination type.</param>
        /// <returns>
        /// true if this converter can perform the conversion; otherwise, false.
        /// </returns>
        public virtual bool CanConvertTo(Type destinationType)
        {
            return destinationType == typeof(string);
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
        public virtual object ConvertFrom(CultureInfo culture, object value, Type propertyType)
        {
            InstanceDescriptor id = value as InstanceDescriptor;
            if (id != null)
            {
                return id.Invoke();
            }

            throw this.GetConvertFromException(value);
        }

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
        public virtual object ConvertTo(CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            if (destinationType == typeof(string))
            {
                if (value == null)
                {
                    return string.Empty;
                }

                // Pre-whidbey we just did a ToString() here.  To minimize the chance of a breaking change we
                // still send requests for the CurrentCulture to ToString() (which should return the same).
                if (culture != null && !culture.Equals(CultureInfo.CurrentCulture))
                {
                    // VSWhidbey 75433 - If the object is IFormattable, use this interface to convert to string
                    // so we use the specified culture rather than the CurrentCulture like object.ToString() does.
                    IFormattable formattable = value as IFormattable;
                    if (formattable != null)
                    {
                        return formattable.ToString(/* format = */ null, /* formatProvider = */ culture);
                    }
                }

                return value.ToString();
            }

            throw this.GetConvertToException(value, destinationType);
        }

        /// <summary>
        /// Converts the given string to the converter's native type using the invariant culture.
        /// </summary>
        /// <param name="text">The value to convert from.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> that represents the converted value.
        /// </returns>
        public object ConvertFromInvariantString(string text, Type propertyType)
        {
            return this.ConvertFrom(CultureInfo.InvariantCulture, text, propertyType);
        }

        /// <summary>
        /// Gets a suitable exception to throw when a conversion cannot be performed.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns><see cref="NotSupportedException"/></returns>
        protected Exception GetConvertFromException(object value)
        {
            string valueTypeName = value == null ? "null" : value.GetType().FullName;
            throw new NotSupportedException(string.Format("{0} cannot convert from {1}", this.GetType().Name, valueTypeName));
        }

        /// <summary>
        /// Gets a suitable exception to throw when a conversion cannot be performed.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="destinationType">The destination type to convert to.</param>
        /// <returns><see cref="NotSupportedException"/></returns>
        protected Exception GetConvertToException(object value, Type destinationType)
        {
            string valueTypeName = value == null ? "null" : value.GetType().FullName;
            throw new NotSupportedException(string.Format("{0} cannot convert from {1} to {2}", this.GetType().Name, valueTypeName, destinationType.FullName));
        }
    }
}
