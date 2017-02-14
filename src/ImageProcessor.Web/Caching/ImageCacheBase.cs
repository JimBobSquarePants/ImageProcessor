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
    using System.ComponentModel;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Hosting;

    using ImageProcessor.Web.Configuration;

    /// <summary>
    /// The image cache base provides methods for implementing the <see cref="IImageCacheExtended"/> interface.
    /// It is recommended that any implementations inherit from this class.
    /// </summary>
    public abstract class ImageCacheBase : IImageCacheExtended
    {
        private static readonly CacheTrimmer Trimmer = new CacheTrimmer();

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
        /// Call <see cref="M:DebounceTrimmerAsync"/> within your implementation to correctly debounce cache cleanup.
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use ScheduleCacheTrimmer instead")]
        protected virtual Task DebounceTrimmerAsync(Func<Task> trimmer)
        {
            // Wrap new method
            this.ScheduleCacheTrimmer(token => trimmer());
            return Task.FromResult(0);
        }

        /// <summary>
        /// Will schedule any cache trimming  to ensure that only one cleanup operation is running at any one time
        /// and that it is a quiet time to do so.
        /// </summary>
        /// <param name="trimmer">The cache trimming method.</param>
        protected void ScheduleCacheTrimmer(Func<CancellationToken, Task> trimmer)
        {
            Trimmer.ScheduleTrimCache(trimmer);
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

        /// <summary>
        /// This ensures that any cache trimming operation is executed on a background thread and that only one operation can ever occur at one time.
        /// The execution will occur on a sliding timeframe, so anytime ScheduleTrimCache is called, it will check if it's within the timeout, if not it
        /// will delay the timeout again but only until the maximum wait time is reached.
        /// </summary>
        private class CacheTrimmer : IRegisteredObject
        {
            /// <summary>
            /// The object to lock against
            /// </summary>
            private static readonly object Locker = new object();

            /// <summary>
            /// Whether the trimming task is running
            /// </summary>
            private static bool trim;

            /// <summary>
            /// The asynchronous trimmer task
            /// </summary>
            // private static Task task;

            /// <summary>
            /// The cancellation token source
            /// </summary>
            private static readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

            /// <summary>
            /// The timestamp
            /// </summary>
            private DateTime timestamp;

            /// <summary>
            /// The timer
            /// </summary>
            private Timer timer;

            /// <summary>
            /// Initializes a new instance of the <see cref="CacheTrimmer"/> class.
            /// </summary>
            public CacheTrimmer()
            {
                HostingEnvironment.RegisterObject(this);
            }

            /// <summary>
            /// The sliding delay time
            /// </summary>
            private const int WaitMilliseconds = 120000; //(2 min)

            /// <summary>
            /// The maximum time period that will elapse until we must trim (30 mins)
            /// </summary>
            private const int MaxWaitMilliseconds = 1800000;

            public void ScheduleTrimCache(Func<CancellationToken, Task> trimmer)
            {
                // Don't continue if already trimming or canceled
                if (trim || tokenSource.IsCancellationRequested)
                {
                    return;
                }

                lock (Locker)
                {
                    if (this.timer == null)
                    {
                        // It's the initial call to this at the beginning or after successful commit
                        this.timestamp = DateTime.Now;
                        this.timer = new Timer(_ => this.TimerRelease(trimmer));
                        this.timer.Change(WaitMilliseconds, 0);
                    }
                    else
                    {
                        // If we've been cancelled then be sure to cancel the timer
                        if (tokenSource.IsCancellationRequested)
                        {
                            // Stop the timer
                            this.timer.Change(Timeout.Infinite, Timeout.Infinite);
                            this.timer.Dispose();
                            this.timer = null;
                        }
                        else if (
                            // Must be less than the max and less than the delay
                            DateTime.Now - this.timestamp < TimeSpan.FromMilliseconds(MaxWaitMilliseconds) &&
                            DateTime.Now - this.timestamp < TimeSpan.FromMilliseconds(WaitMilliseconds))
                        {
                            // Delay
                            this.timer.Change(WaitMilliseconds, 0);
                        }
                        else
                        {
                            // Cannot delay! the callback will execute on the pending timeout
                        }
                    }
                }
            }

            /// <summary>
            /// Performs the trimming function.
            /// </summary>
            /// <param name="trimmer">The trimmer method.</param>
            /// <returns></returns>
            private static async Task PerformTrim(Func<CancellationToken, Task> trimmer)
            {
                if (tokenSource.IsCancellationRequested)
                {
                    return;
                }

                trim = true;
                await trimmer(tokenSource.Token);
                trim = false;
            }

            /// <summary>
            /// Releases the timer operation, running the cache trimmer.
            /// </summary>
            /// <param name="trimmer">The trimmer method.</param>
            private void TimerRelease(Func<CancellationToken, Task> trimmer)
            {
                lock (Locker)
                {
                    // Don't continue if already trimming or canceled
                    if (trim || tokenSource.IsCancellationRequested)
                    {
                        return;
                    }

                    // If the timer is not null then a trim has been scheduled
                    if (this.timer != null)
                    {
                        // Stop the timer
                        this.timer.Change(Timeout.Infinite, Timeout.Infinite);
                        this.timer.Dispose();
                        this.timer = null;

                        // Trim!
                        trim = true;

                        // We are already on a background then so we will block here
                        PerformTrim(trimmer).Wait();
                    }
                }
            }

            /// <inheritdoc />
            public void Stop(bool immediate)
            {
                //Stop the timer
                this.timer.Change(Timeout.Infinite, Timeout.Infinite);
                this.timer.Dispose();
                this.timer = null;

                if (!tokenSource.IsCancellationRequested)
                {
                    tokenSource.Cancel();
                }
            }
        }
    }
}