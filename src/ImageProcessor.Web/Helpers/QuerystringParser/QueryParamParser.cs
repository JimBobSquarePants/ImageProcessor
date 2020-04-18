// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryParamParser.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The query parameter parser that converts string values to different types.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Web;

    /// <summary>
    /// The query parameter parser that converts string values to different types.
    /// </summary>
    public sealed class QueryParamParser
    {
        /// <summary>
        /// A new instance of the <see cref="QueryParamParser" /> class.
        /// with lazy initialization.
        /// </summary>
        private static readonly Lazy<QueryParamParser> Lazy = new Lazy<QueryParamParser>(() => new QueryParamParser());

        /// <summary>
        /// Prevents a default instance of the <see cref="QueryParamParser" /> class from being created.
        /// </summary>
        private QueryParamParser()
        {
            this.AddColorConverters();
            this.AddFontFamilyConverters();
            this.AddPointConverters();
            this.AddSizeConverters();
            this.AddGenericConverters();
            this.AddListConverters();
            this.AddArrayConverters();
        }

        /// <summary>
        /// Gets the current <see cref="QueryParamParser" /> instance.
        /// </summary>
        /// <value>
        /// The <see cref="QueryParamParser" /> instance.
        /// </value>
        public static QueryParamParser Instance => Lazy.Value;

        /// <summary>
        /// Parses the given string value converting it to the given type.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type" /> to convert the string to.</typeparam>
        /// <param name="value">The <see cref="string" /> value to parse.</param>
        /// <param name="culture">The <see cref="CultureInfo" /> to use as the current culture.
        /// <remarks>If not set will parse using <see cref="CultureInfo.InvariantCulture" /></remarks></param>
        /// <returns>
        /// The <typeparamref name="T" />.
        /// </returns>
        public T ParseValue<T>(string value, CultureInfo culture = null)
        {
            if (culture == null)
            {
                culture = CultureInfo.InvariantCulture;
            }

            Type type = typeof(T);

            IQueryParamConverter converter = QueryTypeDescriptor.GetConverter(type);
            if (converter == null && Nullable.GetUnderlyingType(type) is Type underlyingType)
            {
                type = underlyingType;
                converter = QueryTypeDescriptor.GetConverter(type);
            }

            try
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return (T)converter.ConvertFrom(culture, HttpUtility.UrlDecode(value), type);
            }
            catch
            {
                // Return the default value
                return default(T);
            }
        }

        /// <summary>
        /// Adds a type converter to the parser.
        /// </summary>
        /// <param name="type">The <see cref="Type" /> to add a converter for.</param>
        /// <param name="converterType">The type of <see cref="IQueryParamConverter" /> to add.</param>
        public void AddTypeConverter(Type type, Type converterType) => QueryTypeDescriptor.AddConverter(type, converterType);

        /// <summary>
        /// Adds color converters.
        /// </summary>
        private void AddColorConverters() => this.AddTypeConverter(typeof(Color), typeof(ColorTypeConverter));

        /// <summary>
        /// Adds font family converters.
        /// </summary>
        private void AddFontFamilyConverters() => this.AddTypeConverter(typeof(FontFamily), typeof(FontFamilyConverter));

        /// <summary>
        /// Adds point converters.
        /// </summary>
        private void AddPointConverters()
        {
            this.AddTypeConverter(typeof(Point), typeof(PointConverter));
            this.AddTypeConverter(typeof(PointF), typeof(PointFConverter));
        }

        /// <summary>
        /// Adds size converters.
        /// </summary>
        private void AddSizeConverters() => this.AddTypeConverter(typeof(Size), typeof(SizeConverter));

        /// <summary>
        /// Add the generic converters.
        /// </summary>
        private void AddGenericConverters()
        {
            this.AddTypeConverter(typeof(sbyte), typeof(GenericConvertableConverter<sbyte>));
            this.AddTypeConverter(typeof(byte), typeof(GenericConvertableConverter<byte>));

            this.AddTypeConverter(typeof(short), typeof(GenericConvertableConverter<short>));
            this.AddTypeConverter(typeof(ushort), typeof(GenericConvertableConverter<ushort>));

            this.AddTypeConverter(typeof(int), typeof(GenericConvertableConverter<int>));
            this.AddTypeConverter(typeof(uint), typeof(GenericConvertableConverter<uint>));

            this.AddTypeConverter(typeof(long), typeof(GenericConvertableConverter<long>));
            this.AddTypeConverter(typeof(ulong), typeof(GenericConvertableConverter<ulong>));

            this.AddTypeConverter(typeof(decimal), typeof(GenericConvertableConverter<decimal>));
            this.AddTypeConverter(typeof(float), typeof(GenericConvertableConverter<float>));

            this.AddTypeConverter(typeof(double), typeof(GenericConvertableConverter<double>));
            this.AddTypeConverter(typeof(string), typeof(GenericConvertableConverter<string>));

            this.AddTypeConverter(typeof(bool), typeof(GenericConvertableConverter<bool>));
        }

        /// <summary>
        /// Adds a selection of default list type converters.
        /// </summary>
        private void AddListConverters()
        {
            this.AddTypeConverter(typeof(List<sbyte>), typeof(GenericListTypeConverter<sbyte>));
            this.AddTypeConverter(typeof(List<byte>), typeof(GenericListTypeConverter<byte>));

            this.AddTypeConverter(typeof(List<short>), typeof(GenericListTypeConverter<short>));
            this.AddTypeConverter(typeof(List<ushort>), typeof(GenericListTypeConverter<ushort>));

            this.AddTypeConverter(typeof(List<int>), typeof(GenericListTypeConverter<int>));
            this.AddTypeConverter(typeof(List<uint>), typeof(GenericListTypeConverter<uint>));

            this.AddTypeConverter(typeof(List<long>), typeof(GenericListTypeConverter<long>));
            this.AddTypeConverter(typeof(List<ulong>), typeof(GenericListTypeConverter<ulong>));

            this.AddTypeConverter(typeof(List<decimal>), typeof(GenericListTypeConverter<decimal>));
            this.AddTypeConverter(typeof(List<float>), typeof(GenericListTypeConverter<float>));
            this.AddTypeConverter(typeof(List<double>), typeof(GenericListTypeConverter<double>));

            this.AddTypeConverter(typeof(List<string>), typeof(GenericListTypeConverter<string>));

            this.AddTypeConverter(typeof(List<Color>), typeof(GenericListTypeConverter<Color>));
        }

        /// <summary>
        /// Adds a selection of default array type converters.
        /// </summary>
        private void AddArrayConverters()
        {
            this.AddTypeConverter(typeof(sbyte[]), typeof(GenericArrayTypeConverter<sbyte>));
            this.AddTypeConverter(typeof(byte[]), typeof(GenericArrayTypeConverter<byte>));

            this.AddTypeConverter(typeof(short[]), typeof(GenericArrayTypeConverter<short>));
            this.AddTypeConverter(typeof(ushort[]), typeof(GenericArrayTypeConverter<ushort>));

            this.AddTypeConverter(typeof(int[]), typeof(GenericArrayTypeConverter<int>));
            this.AddTypeConverter(typeof(uint[]), typeof(GenericArrayTypeConverter<uint>));

            this.AddTypeConverter(typeof(long[]), typeof(GenericArrayTypeConverter<long>));
            this.AddTypeConverter(typeof(ulong[]), typeof(GenericArrayTypeConverter<ulong>));

            this.AddTypeConverter(typeof(decimal[]), typeof(GenericArrayTypeConverter<decimal>));
            this.AddTypeConverter(typeof(float[]), typeof(GenericArrayTypeConverter<float>));
            this.AddTypeConverter(typeof(double[]), typeof(GenericArrayTypeConverter<double>));

            this.AddTypeConverter(typeof(string[]), typeof(GenericArrayTypeConverter<string>));

            this.AddTypeConverter(typeof(Color[]), typeof(GenericArrayTypeConverter<Color>));
        }
    }
}
