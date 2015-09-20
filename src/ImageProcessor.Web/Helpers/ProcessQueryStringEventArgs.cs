// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessQueryStringEventArgs.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The process querystring event arguments.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Helpers
{
    using System;
    using System.Web;

    /// <summary>
    /// The process querystring event arguments.
    /// </summary>
    public class ProcessQueryStringEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the current request context.
        /// </summary>
        public HttpContext Context { get; set; }

        /// <summary>
        /// Gets or sets the querystring.
        /// </summary>
        public string Querystring { get; set; }

        /// <summary>
        /// Gets or sets the raw http request url.
        /// </summary>
        public string RawUrl { get; set; }
    }
}
