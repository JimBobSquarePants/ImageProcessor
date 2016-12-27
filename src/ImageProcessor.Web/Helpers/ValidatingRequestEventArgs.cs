// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidatingRequestEventArgs.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The validating request event args
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Helpers
{
    using System.ComponentModel;
    using System.Web;

    /// <summary>
    /// The validating request event args
    /// </summary>
    /// <remarks>
    /// This can be used by event subscribers to cancel image processing based on the information contained in the
    /// request, or can be used to directly manipulate the querystring parameter that will be used to process the image.
    /// </remarks>
    public class ValidatingRequestEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatingRequestEventArgs"/> class.
        /// </summary>
        /// <param name="context">The current http context.</param>
        /// <param name="queryString">The query string.</param>
        public ValidatingRequestEventArgs(HttpContextBase context, string queryString)
        {
            this.Context = context;
            this.QueryString = queryString;
        }

        /// <summary>
        /// Gets the current http context.
        /// </summary>
        public HttpContextBase Context { get; private set; }

        /// <summary>
        /// Gets or sets the query string
        /// </summary>
        /// <remarks>
        /// Event subscribers can directly manipulate the querystring before it's used for image processing
        /// </remarks>
        public string QueryString { get; set; }
    }
}