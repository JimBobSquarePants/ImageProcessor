// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteFile.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates methods used to download files from a website address.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Helpers
{
    using System;

    /// <summary>
    /// Helper class for decoding passed urls
    /// </summary>
    public class UrlDecoder
    {
        /// <summary>
        /// A new instance of the <see cref="UrlDecoder"/> class.
        /// with lazy initialization.
        /// </summary>
        private static readonly Lazy<UrlDecoder> Lazy = new Lazy<UrlDecoder>(() => new UrlDecoder());

        /// <summary>
        /// Gets the current <see cref="UrlDecoder"/> instance.
        /// </summary>
        public static UrlDecoder Instance => Lazy.Value;

        /// <summary>
        /// Url decodes the passed url
        /// </summary>
        /// <param name="url">Url parameter to decode</param>
        /// <returns></returns>
        public string DecodeUrl(string url)
        {
            //Use Uri.UnescapeDataString instead of HttpUtility.UrlDecode to maintain plus-characters (+)
            return Uri.UnescapeDataString(url);
        }
    }
}
