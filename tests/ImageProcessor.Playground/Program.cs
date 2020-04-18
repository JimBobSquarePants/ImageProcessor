// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
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

            using (Image image = new Bitmap(200, 200))
            {
                using (ImageFactory factory = new ImageFactory())
                {
                    factory.Load(image).BackgroundColor(Color.HotPink).Save(Path.GetFullPath(Path.Combine(outPath, "test.bmp")));
                }
            }

            for (int i = 0; i < 1; i++)
            {
                foreach (FileInfo fileInfo in files)
                {
                    // Start timing.
                    byte[] photoBytes = File.ReadAllBytes(fileInfo.FullName);
                    Console.WriteLine("Processing: " + fileInfo.Name);

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    using (MemoryStream inStream = new MemoryStream(photoBytes))
                    using (ImageFactory imageFactory = new ImageFactory(MetaDataMode.CopyrightAndGeolocation) { AnimationProcessMode = AnimationProcessMode.All })
                    {
                        try
                        {
                            imageFactory.Load(inStream)
                                        .Resize(new Size(426, 0))
                                        //.Crop(new Rectangle(0, 0, imageFactory.Image.Width / 2, imageFactory.Image.Height / 2))
                                        .Save(Path.GetFullPath(Path.Combine(outPath, fileInfo.Name)));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        stopwatch.Stop();
                    }

                    // Report back.
                    long peakWorkingSet64 = Process.GetCurrentProcess().PeakWorkingSet64;
                    float mB = peakWorkingSet64 / (float)1024 / 1024;

                    Console.WriteLine(@"Completed {0} in {1:s\.fff} secs {2}Peak memory usage was {3:#,#} bytes or {4} Mb.", fileInfo.Name, stopwatch.Elapsed, Environment.NewLine, peakWorkingSet64, mB);
                }
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
                throw new ArgumentNullException(nameof(extensions));
            }

            IEnumerable<FileInfo> files = dir.EnumerateFiles("*.*", SearchOption.AllDirectories);
            return files.Where(f => extensions.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all files in a given directory by name.
        /// </summary>
        /// <param name="dir">The directory to search within.</param>
        /// <param name="name">The file extensions.</param>
        /// <returns>
        /// The <see cref="IEnumerable{FileInfo}"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if no extensions are given.
        /// </exception>
        public static IEnumerable<FileInfo> GetFilesByName(DirectoryInfo dir, params string[] name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            IEnumerable<FileInfo> files = dir.EnumerateFiles("*.*", SearchOption.AllDirectories);
            return files.Where(f => name.Contains(f.Name, StringComparer.OrdinalIgnoreCase));
        }
    }
}
