namespace ImageProcessor.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using ImageProcessor.Web.Extensions;

    public class EnumConverter : QueryParamConverter
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
            if (value.IsNullOrEmptyString())
            {
                // Value types return default instance.
                return propertyType.GetInstance();
            }

            if (value is string)
            {
                string strValue = (string)value;
                if (strValue.IndexOf(',') != -1)
                {
                    long convertedValue = 0;
                    string[] values = this.GetStringArray(strValue, culture);

                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (string v in values)
                    {
                        // OR assignment. Stolen from ComponentModel EnumConverter.
                        convertedValue |= Convert.ToInt64((Enum)Enum.Parse(propertyType, v, true), culture);
                    }

                    return Enum.ToObject(propertyType, convertedValue);
                }

                return Enum.Parse(propertyType, strValue, true);
            }

            if (value is int)
            {
                // Should handle most cases.
                if (Enum.IsDefined(propertyType, value))
                {
                    return Enum.ToObject(propertyType, value);
                }
            }

            if (value != null)
            {
                var valueType = value.GetType();
                if (valueType.IsEnum)
                {
                    // This should work for most cases where enums base type is int.
                    return Enum.ToObject(propertyType, Convert.ToInt64(value, culture));
                }

                if (valueType.IsEnumerableOfType(typeof(string)))
                {
                    long convertedValue = 0;
                    var enumerable = ((IEnumerable<string>)value).ToList();

                    if (enumerable.Any())
                    {
                        // ReSharper disable once LoopCanBeConvertedToQuery
                        foreach (string v in enumerable)
                        {
                            convertedValue |= Convert.ToInt64((Enum)Enum.Parse(propertyType, v, true), culture);
                        }

                        return Enum.ToObject(propertyType, convertedValue);
                    }

                    return propertyType.GetInstance();
                }
            }

            Enum[] enums = value as Enum[];
            if (enums != null)
            {
                long convertedValue = 0;
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (Enum e in enums)
                {
                    convertedValue |= Convert.ToInt64(e, culture);
                }

                return Enum.ToObject(propertyType, convertedValue);
            }

            return base.ConvertFrom(culture, value, propertyType);
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
