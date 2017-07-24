// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostProcessorBootstrapper.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The postprocessor bootstrapper.
//   Many thanks to Azure Image Optimizer <see href="https://github.com/ligershark/AzureJobs"/>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Plugins.PostProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    using ImageProcessor.Configuration;

    /// <summary>
    /// The postprocessor bootstrapper.
    /// Many thanks to Azure Image Optimizer <see href="https://github.com/ligershark/AzureJobs"/>
    /// </summary>
    internal sealed class PostProcessorBootstrapper
    {
        /// <summary>
        /// The assembly version.
        /// </summary>
        private static readonly string AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// A new instance of the <see cref="T:ImageProcessor.Web.Config.ImageProcessorConfig"/> class.
        /// with lazy initialization.
        /// </summary>
        private static readonly Lazy<PostProcessorBootstrapper> Lazy =
                        new Lazy<PostProcessorBootstrapper>(() => new PostProcessorBootstrapper());

        /// <summary>
        /// Prevents a default instance of the <see cref="PostProcessorBootstrapper"/> class from being created.
        /// </summary>
        private PostProcessorBootstrapper()
        {
            if (!Lazy.IsValueCreated)
            {
                this.RegisterExecutables();
            }
        }

        /// <summary>
        /// Gets the current instance of the <see cref="PostProcessorBootstrapper"/> class.
        /// </summary>
        public static PostProcessorBootstrapper Instance => Lazy.Value;

        /// <summary>
        /// Gets the working directory path.
        /// </summary>
        public string WorkingPath { get; private set; }

        /// <summary>
        /// Gets or a value indicating whether the post processor has been installed.
        /// </summary>
        public bool IsInstalled { get; private set; }

        /// <summary>
        /// Gets the allowed time in milliseconds for postprocessing an image file.
        /// </summary>
        public int Timout { get; internal set; } = 5000;

        /// <summary>
        /// Registers the embedded executables.
        /// </summary>
        public void RegisterExecutables()
        {
            // None of the tools used here are called using dllimport so we don't go through the normal registration channel.
            string folder = ImageProcessorBootstrapper.Instance.NativeBinaryFactory.Is64BitEnvironment ? "x64" : "x86";
            Assembly assembly = Assembly.GetExecutingAssembly();

            if (assembly.Location == null)
            {
                ImageProcessorBootstrapper.Instance.Logger.Log(
                    typeof(PostProcessorBootstrapper),
                    "Unable to install postprocessor - No images will be post-processed. Unable to map location for assembly.");

                return;
            }

            this.WorkingPath = Path.GetFullPath(
                Path.Combine(new Uri(assembly.Location).LocalPath,
                    "..\\imageprocessor.postprocessor" + AssemblyVersion + "\\"));

            string path = Path.GetDirectoryName(this.WorkingPath);

            if (string.IsNullOrWhiteSpace(path))
            {
                ImageProcessorBootstrapper.Instance.Logger.Log(
                    typeof(PostProcessorBootstrapper),
                    "Unable to install postprocessor - No images will be post-processed. Unable to map working path for processors.");

                return;
            }

            // Create the folder for storing the executables.
            // Delete any previous instances to make sure we copy over the new files.
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }

                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                ImageProcessorBootstrapper.Instance.Logger.Log(typeof(PostProcessorBootstrapper),
                    $"{ex.Message}, {ex.StackTrace}. Inner: {ex.InnerException?.Message}, {ex.InnerException?.StackTrace}");
                ImageProcessorBootstrapper.Instance.Logger.Log(
                    typeof(PostProcessorBootstrapper),
                    "Unable to install postprocessor - No images will be post-processed. Unable to map working path for processors.");

                return;
            }

            // Get the resources and copy them across.
            Dictionary<string, string> resources = new Dictionary<string, string>
            {
                { "gifsicle.exe", "ImageProcessor.Web.Plugins.PostProcessor.Resources.Unmanaged." + folder + ".gifsicle.exe" },
                { "jpegtran.exe", "ImageProcessor.Web.Plugins.PostProcessor.Resources.Unmanaged.x86.jpegtran.exe" },
                { "cjpeg.exe", "ImageProcessor.Web.Plugins.PostProcessor.Resources.Unmanaged.x86.cjpeg.exe" },
                { "libjpeg-62.dll", "ImageProcessor.Web.Plugins.PostProcessor.Resources.Unmanaged.x86.libjpeg-62.dll" },
                { "pngquant.exe", "ImageProcessor.Web.Plugins.PostProcessor.Resources.Unmanaged.x86.pngquant.exe" },
                { "pngout.exe", "ImageProcessor.Web.Plugins.PostProcessor.Resources.Unmanaged.x86.pngout.exe" },
                { "TruePNG.exe", "ImageProcessor.Web.Plugins.PostProcessor.Resources.Unmanaged.x86.TruePNG.exe" },
                { "png.cmd", "ImageProcessor.Web.Plugins.PostProcessor.Resources.Unmanaged.x86.png.cmd" }
            };

            // Write the files out to the bin folder.
            foreach (KeyValuePair<string, string> resource in resources)
            {
                using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource.Value))
                {
                    if (resourceStream != null)
                    {
                        using (FileStream fileStream = File.OpenWrite(Path.Combine(this.WorkingPath, resource.Key)))
                        {
                            resourceStream.CopyTo(fileStream);
                        }
                    }
                }
            }

            this.IsInstalled = true;
        }
    }
}