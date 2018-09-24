// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostProcessorApplicationEvents.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Defines the ApplicationEvents type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Web;

[assembly: PreApplicationStartMethod(typeof(ImageProcessor.Web.Plugins.PostProcessor.PostProcessorApplicationEvents), "Start")]

namespace ImageProcessor.Web.Plugins.PostProcessor
{
    using ImageProcessor.Web.Helpers;
    using ImageProcessor.Web.HttpModules;

    /// <summary>
    /// Binds the PostProcessor to process any image requests within the web application.
    /// Many thanks to Azure Image Optimizer <see href="https://github.com/ligershark/AzureJobs" />
    /// </summary>
    public static class PostProcessorApplicationEvents
    {
        /// <summary>
        /// The initial startup method.
        /// </summary>
        public static void Start()
        {
            ImageProcessingModule.OnPostProcessing += PostProcess;
        }

        /// <summary>
        /// Sets the timeout limit in milliseconds for the post processor.
        /// </summary>
        /// <param name="milliseconds">The timeout limit in milliseconds.</param>
        /// <remarks>
        /// The default timeout is 5000 milliseconds. Timeouts lower or equal to 0 are ignored (to prevent waiting indefinitely for the post processor processes to exit).
        /// </remarks>
        public static void SetPostProcessingTimeout(int milliseconds)
        {
            if (milliseconds > 0)
            {
                PostProcessorBootstrapper.Instance.Timout = milliseconds;
            }
        }

        /// <summary>
        /// Post-processes cached images.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="PostProcessingEventArgs">EventArgs</see> that contains the event data.</param>
        private static void PostProcess(object sender, PostProcessingEventArgs e)
        {
            e.ImageStream = PostProcessor.PostProcessImageAsync(e.Context, e.ImageStream, e.ImageExtension).Result;
        }
    }
}
