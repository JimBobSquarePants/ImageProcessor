// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.PlayGround
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    using ImageProcessor;

    /// <summary>
    /// The program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main routine.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "It works ok?")]
        public static void Main(string[] args)
        {
            var files = GetFilesByExtensions(new DirectoryInfo(Environment.CurrentDirectory).GetDirectories("Images").Single(), ".png");
            var factory = new ImageFactory();
            var imagePath = files.First().FullName;
            factory.Load(imagePath);
            var replaceColor = factory.ReplaceColor(Color.LightGray, Color.Yellow, 10);
            replaceColor.Save(Environment.CurrentDirectory + "/Test.png");
        }

        /// <summary>
        /// Gets all files in a given directory by extension.
        /// </summary>
        /// <param name="dir">The directory to search within.</param>
        /// <param name="extensions">The file extensions.</param>
        /// <returns>
        /// The <see cref="IEnumerable{FileInfo}"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if no extensions are given.
        /// </exception>
        public static IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException("extensions");
            }

            IEnumerable<FileInfo> files = dir.EnumerateFiles("*.*", SearchOption.AllDirectories);
            return files.Where(f => extensions.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));
        }
    }
}
