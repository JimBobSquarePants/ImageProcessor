// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostProcessorBootstrapper.cs" company="James South">
//   Copyright (c) James South.
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
        public static PostProcessorBootstrapper Instance
        {
            get
            {
                return Lazy.Value;
            }
        }

        /// <summary>
        /// Gets the working directory path.
        /// </summary>
        public string WorkingPath { get; private set; }

        /// <summary>
        /// Registers the embedded executables.
        /// </summary>
        public void RegisterExecutables()
        {
            // None of the tools used here are called using dllimport so we don't go through the normal registration channel. 
            string folder = ImageProcessorBootstrapper.Instance.NativeBinaryFactory.Is64BitEnvironment ? "x64" : "x86";
            Assembly assembly = Assembly.GetExecutingAssembly();
            this.WorkingPath = Path.GetFullPath(
                Path.Combine(new Uri(assembly.Location).LocalPath, "..\\imageprocessor.postprocessor" + AssemblyVersion + "\\"));

            // Create the folder for storing temporary images.
            // ReSharper disable once AssignNullToNotNullAttribute
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(this.WorkingPath));

            // Prevent the files being copied over more than once.
            if (directoryInfo.Exists)
            {
                return;
            }

            directoryInfo.Create();

            // Get the resources and copy them across.
            Dictionary<string, string> resources = new Dictionary<string, string>
            {
                { "gifsicle.exe", "ImageProcessor.Web.Plugins.PostProcessor.Resources.Unmanaged." + folder + ".gifsicle.exe" },
                { "jpegtran.exe", "ImageProcessor.Web.Plugins.PostProcessor.Resources.Unmanaged.x86.jpegtran.exe" },
                { "optipng.exe", "ImageProcessor.Web.Plugins.PostProcessor.Resources.Unmanaged.x86.pngquant.exe" },
                { "pngout.exe", "ImageProcessor.Web.Plugins.PostProcessor.Resources.Unmanaged.x86.pngout.exe" },
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
        }
    }
}