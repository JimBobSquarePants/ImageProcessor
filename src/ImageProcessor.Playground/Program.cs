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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    using ImageProcessor;
    using ImageProcessor.Imaging;

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
            const string InputImages = @"..\..\..\ImageProcessor.UnitTests\Images\";
            const string OutputImages = @"..\..\images\output";

            string root = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            string inPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(root), InputImages));
            string outPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(root), OutputImages));

            DirectoryInfo di = new DirectoryInfo(inPath);
            IEnumerable<FileInfo> files = GetFilesByExtensions(di, ".jpg", ".jpeg", ".jfif", ".gif", ".bmp", ".png", ".tif");

            foreach (FileInfo fileInfo in files)
            {
                // Start timing.
                byte[] photoBytes = File.ReadAllBytes(fileInfo.FullName);
                Console.WriteLine("Processing: " + fileInfo.Name);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                using (MemoryStream inStream = new MemoryStream(photoBytes))
                using (ImageFactory imageFactory = new ImageFactory(true, true))
                {
                    Size size = new Size(50, 0);

                    ResizeLayer layer = new ResizeLayer(size);

                    imageFactory.Load(inStream)
                        .Resize(layer)
                                //.Resolution(400, 400)
                                //.ReplaceColor(Color.LightGray, Color.Yellow, 10)
                                .Save(Path.GetFullPath(Path.Combine(outPath, fileInfo.Name)));

                    stopwatch.Stop();
                }

                // Report back.
                long peakWorkingSet64 = Process.GetCurrentProcess().PeakWorkingSet64;
                float mB = peakWorkingSet64 / (float)1024 / 1024;

                Console.WriteLine(@"Completed {0} in {1:s\.fff} secs {2}Peak memory usage was {3} bytes or {4} Mb.", fileInfo.Name, stopwatch.Elapsed, Environment.NewLine, peakWorkingSet64.ToString("#,#"), mB);
            }

            Console.ReadLine();
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
