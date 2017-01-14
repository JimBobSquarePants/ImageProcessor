// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageCacheBase.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The image cache base provides methods for implementing the <see cref="IImageCache" /> interface.
//   It is recommended that any implementations inherit from this class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Caching
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web;

    using ImageProcessor.Web.Configuration;

    /// <summary>
    /// The image cache base provides methods for implementing the <see cref="IImageCache"/> interface.
    /// It is recommended that any implementations inherit from this class.
    /// </summary>
    public abstract class ImageCacheBase : IImageCache
    {
        /// <summary>
        /// The request path for the image.
        /// </summary>
        protected readonly string RequestPath;

        /// <summary>
        /// The full path for the image.
        /// </summary>
        protected readonly string FullPath;

        /// <summary>
        /// The querystring containing processing instructions.
        /// </summary>
        protected readonly string Querystring;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageCacheBase"/> class.
        /// </summary>
        /// <param name="requestPath">
        /// The request path for the image.
        /// </param>
        /// <param name="fullPath">
        /// The full path for the image.
        /// </param>
        /// <param name="querystring">
        /// The querystring containing instructions.
        /// </param>
        protected ImageCacheBase(string requestPath, string fullPath, string querystring)
        {
            this.RequestPath = requestPath;
            this.FullPath = fullPath;
            this.Querystring = querystring;

            ImageProcessorConfiguration config = ImageProcessorConfiguration.Instance;
            this.Settings = this.AugmentSettingsCore(config.ImageCacheSettings);
            this.MaxDays = config.ImageCacheMaxDays;
            this.BrowserMaxDays = config.BrowserCacheMaxDays;
            this.TrimCache = config.TrimCache;
            this.FolderDepth = config.FolderDepth;
        }

        /// <summary>
        /// Gets or sets any additional settings required by the cache.
        /// </summary>
        public Dictionary<string, string> Settings { get; set; }

        /// <summary>
        /// Gets or sets the path to the cached image.
        /// </summary>
        public string CachedPath { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of days to store the image.
        /// </summary>
        public int MaxDays { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of days to cache the image in the browser.
        /// </summary>
        public int BrowserMaxDays { get; set; }

        /// <summary>
        /// Gets or sets the maximum number folder levels to nest the cached images.
        /// </summary>
        public int FolderDepth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to periodically trim the cache.
        /// </summary>
        public bool TrimCache { get; set; }

        /// <summary>
        /// Gets a value indicating whether the image is new or updated in an asynchronous manner.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public abstract Task<bool> IsNewOrUpdatedAsync();

        /// <summary>
        /// Adds the image to the cache in an asynchronous manner.
        /// </summary>
        /// <param name="stream">
        /// The stream containing the image data.
        /// </param>
        /// <param name="contentType">
        /// The content type of the image.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> representing an asynchronous operation.
        /// </returns>
        public abstract Task AddImageToCacheAsync(Stream stream, string contentType);

        /// <summary>
        /// Trims the cache of any expired items in an asynchronous manner.
        /// </summary>
        /// <returns>
        /// The asynchronous <see cref="Task"/> representing an asynchronous operation.
        /// </returns>
        public abstract Task TrimCacheAsync();

        /// <summary>
        /// Gets a string identifying the cached file name.
        /// </summary>
        /// <returns>
        /// The asynchronous <see cref="Task"/> returning the value.
        /// </returns>
        public virtual async Task<string> CreateCachedFileNameAsync()
        {
            return await Task.FromResult(CachedImageHelper.GetCachedImageFileName(this.FullPath, this.Querystring));
        }

        /// <summary>
        /// Rewrites the path to point to the cached image.
        /// </summary>
        /// <param name="context">
        /// The <see cref="HttpContext"/> encapsulating all information about the request.
        /// </param>
        public abstract void RewritePath(HttpContext context);

        /// <summary>
        /// Gets a value indicating whether the given images creation date is out with
        /// the prescribed limit.
        /// </summary>
        /// <param name="creationDate">The creation date.</param>
        /// <returns>
        /// The true if the date is out with the limit, otherwise; false.
        /// </returns>
        protected virtual bool IsExpired(DateTime creationDate)
        {
            return creationDate < DateTime.UtcNow.AddDays(-this.MaxDays);
        }

        /// <summary>
        /// Provides a means to augment the cache settings taken from the configuration in derived classes. 
        /// This allows for configuration of cache objects outside the normal configuration files, for example
        /// by using app settings in the Azure platform.
        /// </summary>
        /// <param name="settings">The current settings.</param>
        protected virtual void AugmentSettings(Dictionary<string, string> settings)
        {
        }

        /// <summary>
        /// Provides an entry point to augmentation of the <see cref="Settings"/> dictionary
        /// </summary>
        /// <param name="settings">Dictionary of settings</param>
        /// <returns>augmented dictionary of settings</returns>
        private Dictionary<string, string> AugmentSettingsCore(Dictionary<string, string> settings)
        {
            this.AugmentSettings(settings);
            return settings;
        }
    }
}