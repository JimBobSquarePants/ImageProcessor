// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostProcessingEventArgs.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The post processing event arguments.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Helpers
{
    using System;
    using System.IO;
    using System.Web;

    /// <summary>
    /// The post processing event arguments.
    /// </summary>
    public class PostProcessingEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the current request context.
        /// </summary>
        public HttpContext Context { get; set; }

        /// <summary>
        /// Gets or sets the image stream.
        /// </summary>
        public MemoryStream ImageStream { get; set; }

        /// <summary>
        /// Gets or sets the image extension.
        /// </summary>
        public string ImageExtension { get; set; }
    }
}
