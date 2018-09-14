// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SASCache.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South, Dirk Seefeld
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Describes a Shared Access Signature cache object for private cloud blobs (Azure)
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Plugins.AzureBlobCache
{
    using System;

    /// <summary>
    /// used for SAS caching
    /// </summary>
    internal sealed class SASCache
    {
        /// <summary>
        /// defines how long this item is valid in minutes as defined in cache.config 'SASValidityInMinutes'
        /// </summary>
        public double ValidityMinutes { get; set; }
        /// <summary>
        /// stores creation time of SAS
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// stores SAS query string
        /// </summary>
        public string SASQueryString { get; set; }
    }
}
