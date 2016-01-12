// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryTypeDescriptor.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Hold the collection of <see cref="IQueryParamConverter" /> converters
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Helpers
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Hold the collection of <see cref="IQueryParamConverter"/> converters
    /// </summary>
    internal static class QueryTypeDescriptor
    {
        /// <summary>
        /// The converter cache.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Type> ConverterCache = new ConcurrentDictionary<Type, Type>();

        /// <summary>
        /// Returns an instance of the correct converter for the given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The <see cref="IQueryParamConverter"/>.
        /// </returns>
        public static IQueryParamConverter GetConverter(Type type)
        {
            if (type.IsEnum)
            {
                return (IQueryParamConverter)Activator.CreateInstance(typeof(EnumConverter));
            }

            if (ConverterCache.ContainsKey(type))
            {
                return (IQueryParamConverter)Activator.CreateInstance(ConverterCache[type]);
            }

            return null;
        }

        /// <summary>
        /// Adds the given converter for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="converter">The converter.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the converter does not implement <see cref="IQueryParamConverter"/>.
        /// </exception>
        public static void AddConverter(Type type, Type converter)
        {
            if (!typeof(IQueryParamConverter).IsAssignableFrom(converter))
            {
                throw new ArgumentException("converter does not implement IQueryParamConverter.");
            }

            if (ConverterCache.ContainsKey(type))
            {
                return;
            }

            ConverterCache[type] = converter;
        }
    }
}
