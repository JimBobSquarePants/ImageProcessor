// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ImageProcessor
{
    /// <summary>
    /// Encapsulates a series of time saving extension methods to the <see cref="Assembly"/> class.
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Gets a collection of loadable types from the given assembly.
        /// Adapted from <see href="http://stackoverflow.com/questions/7889228/how-to-prevent-reflectiontypeloadexception-when-calling-assembly-gettypes"/>.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to load the types from.</param>
        /// <returns>
        /// The loadable <see cref="IEnumerable{Type}"/>.
        /// </returns>
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null);
            }
        }

        /// <summary>
        /// Converts an assembly resource into a string.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to load the strings from.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="encoding">The character encoding to return the resource in.</param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetResourceAsString(this Assembly assembly, string resource, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;

            using (var ms = new MemoryStream())
            {
                using (Stream manifestResourceStream = assembly.GetManifestResourceStream(resource))
                {
                    manifestResourceStream?.CopyTo(ms);
                }

                return encoding.GetString(ms.GetBuffer()).Replace('\0', ' ').Trim();
            }
        }

        /// <summary>
        /// Returns the <see cref="FileInfo"/> identifying the file used to load the assembly.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to get the name from.</param>
        /// <returns>The <see cref="FileInfo"/>.</returns>
        public static FileInfo GetAssemblyFile(this Assembly assembly)
        {
            string codeBase = assembly.CodeBase;
            var uri = new Uri(codeBase);
            string path = uri.LocalPath;
            return new FileInfo(path);
        }

        /// <summary>
        /// Returns the <see cref="FileInfo"/> identifying the file used to load the assembly.
        /// </summary>
        /// <param name="assemblyName">The <see cref="AssemblyName"/> to get the name from.</param>
        /// <returns>The <see cref="FileInfo"/>.</returns>
        public static FileInfo GetAssemblyFile(this AssemblyName assemblyName)
        {
            string codeBase = assemblyName.CodeBase;
            var uri = new Uri(codeBase);
            string path = uri.LocalPath;
            return new FileInfo(path);
        }
    }
}
