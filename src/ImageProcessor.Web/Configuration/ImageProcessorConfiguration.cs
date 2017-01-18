// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageProcessorConfiguration.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates methods to allow the retrieval of ImageProcessor settings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Configuration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using ImageProcessor.Configuration;
    using ImageProcessor.Web.Processors;
    using ImageProcessor.Web.Services;

    /// <summary>
    /// Encapsulates methods to allow the retrieval of ImageProcessor settings.
    /// </summary>
    public sealed class ImageProcessorConfiguration
    {
        #region Fields
        /// <summary>
        /// A new instance of the <see cref="T:ImageProcessor.Web.Config.ImageProcessorConfig"/> class.
        /// with lazy initialization.
        /// </summary>
        private static readonly Lazy<ImageProcessorConfiguration> Lazy =
                        new Lazy<ImageProcessorConfiguration>(() => new ImageProcessorConfiguration());

        /// <summary>
        /// A collection of the processing presets defined in the configuration. 
        /// for available plugins.
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> PresetSettings = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// The processing configuration section from the current application configuration. 
        /// </summary>
        private static ImageProcessingSection imageProcessingSection;

        /// <summary>
        /// The cache configuration section from the current application configuration. 
        /// </summary>
        private static ImageCacheSection imageCacheSection;

        /// <summary>
        /// The security configuration section from the current application configuration. 
        /// </summary>
        private static ImageSecuritySection imageSecuritySection;
        #endregion

        #region Constructors
        /// <summary>
        /// Prevents a default instance of the <see cref="ImageProcessorConfiguration"/> class from being created.
        /// </summary>
        private ImageProcessorConfiguration()
        {
            this.LoadGraphicsProcessors();
            this.LoadImageServices();
            this.LoadImageCache();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the current instance of the <see cref="ImageProcessorConfiguration"/> class.
        /// </summary>
        public static ImageProcessorConfiguration Instance => Lazy.Value;

        /// <summary>
        /// Gets the collection of available procesors to run.
        /// </summary>
        public ConcurrentDictionary<Type, Dictionary<string, string>> AvailableWebGraphicsProcessors { get; } = new ConcurrentDictionary<Type, Dictionary<string, string>>();

        /// <summary>
        /// Gets the list of available ImageServices.
        /// </summary>
        public IList<IImageService> ImageServices { get; private set; }

        /// <summary>
        /// Gets the current image cache.
        /// </summary>
        public Type ImageCache { get; private set; }

        /// <summary>
        /// Gets the image cache max days.
        /// </summary>
        public int ImageCacheMaxDays { get; private set; }

        /// <summary>
        /// Gets the value indicating if the disk cache will apply file change monitors that can be used to invalidate the cache
        /// </summary>
        public bool UseFileChangeMonitors { get; private set; }

        /// <summary>
        /// Gets the browser cache max days.
        /// </summary>
        public int BrowserCacheMaxDays { get; private set; }

        /// <summary>
        /// Gets or sets the maximum number folder levels to nest the cached images.
        /// </summary>
        public int FolderDepth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to periodically trim the cache.
        /// </summary>
        public bool TrimCache { get; set; }

        /// <summary>
        /// Gets the image cache settings.
        /// </summary>
        public Dictionary<string, string> ImageCacheSettings { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to preserve exif meta data.
        /// </summary>
        public bool PreserveExifMetaData => GetImageProcessingSection().PreserveExifMetaData;

        /// <summary>
        /// Gets a value indicating whether to allow known cachebusters.
        /// </summary>
        public bool AllowCacheBuster => GetImageProcessingSection().AllowCacheBuster;

        /// <summary>
        /// Gets a value indicating whether to convert images to a linear color space before
        /// processing.
        /// </summary>
        public bool FixGamma => GetImageProcessingSection().FixGamma;

        /// <summary>
        /// Gets a value indicating whether whether to intercept all image requests including ones
        /// without querystring parameters.
        /// </summary>
        public bool InterceptAllRequests => GetImageProcessingSection().InterceptAllRequests;

        #endregion

        #region Methods
        /// <summary>
        /// Returns the processing instructions matching the preset defined in the configuration.
        /// </summary>
        /// <param name="name">
        /// The name of the plugin to get the settings for.
        /// </param>
        /// <returns>
        /// The <see cref="T:Systems.String"/> the processing instructions.
        /// </returns>
        public string GetPresetSettings(string name)
        {
            return PresetSettings.GetOrAdd(
                   name,
                   n =>
                   {
                       ImageProcessingSection.PresetElement presetElement = GetImageProcessingSection()
                       .Presets
                       .Cast<ImageProcessingSection.PresetElement>()
                       .FirstOrDefault(x => x.Name == n);
                       return presetElement?.Value;
                   });
        }

        /// <summary>
        /// Retrieves the security configuration section from the current application configuration. 
        /// </summary>
        /// <returns>The security configuration section from the current application configuration. </returns>
        internal ImageSecuritySection GetImageSecuritySection()
        {
            return imageSecuritySection ?? (imageSecuritySection = ImageSecuritySection.GetConfiguration());
        }

        /// <summary>
        /// Retrieves the processing configuration section from the current application configuration. 
        /// </summary>
        /// <returns>The processing configuration section from the current application configuration. </returns>
        private static ImageProcessingSection GetImageProcessingSection()
        {
            return imageProcessingSection ?? (imageProcessingSection = ImageProcessingSection.GetConfiguration());
        }

        /// <summary>
        /// Retrieves the caching configuration section from the current application configuration. 
        /// </summary>
        /// <returns>The caching configuration section from the current application configuration. </returns>
        private static ImageCacheSection GetImageCacheSection()
        {
            return imageCacheSection ?? (imageCacheSection = ImageCacheSection.GetConfiguration());
        }

        #region GraphicesProcessors
        /// <summary>
        /// Creates and returns a new collection of <see cref="IWebGraphicsProcessor"/> 
        /// <remarks>
        /// Creating the processors should be fairly cheap and better for performance than
        /// locking around the procesors on each request. The System.Drawing.Graphics object still does a lock but that 
        /// isn't used for many procesors.
        /// </remarks>
        /// </summary>
        /// <returns>The <see cref="T:IWebGraphicsProcessor[]"/></returns>
        public IWebGraphicsProcessor[] CreateWebGraphicsProcessors()
        {
            List<IWebGraphicsProcessor> processors = new List<IWebGraphicsProcessor>();

            foreach (KeyValuePair<Type, Dictionary<string, string>> pair in this.AvailableWebGraphicsProcessors)
            {
                IWebGraphicsProcessor processor = (IWebGraphicsProcessor)Activator.CreateInstance(pair.Key);
                processor.Processor.Settings = pair.Value;
                processors.Add(processor);
            }
            return processors.ToArray();
        }

        /// <summary>
        /// Loads graphics processors from configuration.
        /// </summary>
        /// <exception cref="TypeLoadException">
        /// Thrown when an <see cref="IWebGraphicsProcessor"/> cannot be loaded.
        /// </exception>
        private void LoadGraphicsProcessors()
        {
            IEnumerable<ImageProcessingSection.PluginElement> pluginConfigs
                = GetImageProcessingSection().Plugins
                                             .Cast<ImageProcessingSection.PluginElement>()
                                             .Where(p => p.Enabled);

            foreach (ImageProcessingSection.PluginElement pluginConfig in pluginConfigs)
            {
                Type type = Type.GetType(pluginConfig.Type);

                if (type == null)
                {
                    string message = "Couldn't load IWebGraphicsProcessor: " + pluginConfig.Type;
                    ImageProcessorBootstrapper.Instance.Logger.Log<ImageProcessorConfiguration>(message);
                    throw new TypeLoadException(message);
                }

                Dictionary<string, string> settings = this.GetPluginSettings(type.Name);

                // No reason for this to fail.
                this.AvailableWebGraphicsProcessors.TryAdd(type, settings);
            }
        }

        /// <summary>
        /// Returns the <see cref="T:ImageProcessor.Web.Config.ImageProcessingSection.SettingElementCollection"/> for the given plugin.
        /// </summary>
        /// <param name="name">
        /// The name of the plugin to get the settings for.
        /// </param>
        /// <returns>
        /// The <see cref="T:ImageProcessor.Web.Config.ImageProcessingSection.SettingElementCollection"/> for the given plugin.
        /// </returns>
        private Dictionary<string, string> GetPluginSettings(string name)
        {
            ImageProcessingSection.PluginElement pluginElement = GetImageProcessingSection()
                .Plugins
                .Cast<ImageProcessingSection.PluginElement>()
                .FirstOrDefault(x => x.Name == name);

            Dictionary<string, string> settings;

            if (pluginElement != null)
            {
                settings = pluginElement.Settings
                    .Cast<SettingElement>()
                    .ToDictionary(setting => setting.Key, setting => setting.Value);
            }
            else
            {
                settings = new Dictionary<string, string>();
            }

            return settings;
        }
        #endregion

        #region ImageServices
        /// <summary>
        /// Loads image services from configuration.
        /// </summary>
        /// <exception cref="TypeLoadException">
        /// Thrown when an <see cref="IImageService"/> cannot be loaded.
        /// </exception>
        private void LoadImageServices()
        {
            ImageSecuritySection.ServiceElementCollection services = this.GetImageSecuritySection().ImageServices;
            this.ImageServices = new List<IImageService>();
            foreach (ImageSecuritySection.ServiceElement config in services)
            {
                Type type = Type.GetType(config.Type);

                if (type == null)
                {
                    string message = $"Couldn\'t load IImageService: {config.Type}";
                    ImageProcessorBootstrapper.Instance.Logger.Log<ImageProcessorConfiguration>(message);
                    throw new TypeLoadException(message);
                }

                IImageService imageService = Activator.CreateInstance(type) as IImageService;
                if (!string.IsNullOrWhiteSpace(config.Prefix))
                {
                    if (imageService != null)
                    {
                        imageService.Prefix = config.Prefix;
                    }
                }

                this.ImageServices.Add(imageService);
            }

            // Add the available settings.
            foreach (IImageService service in this.ImageServices)
            {
                string name = service.GetType().Name;
                Dictionary<string, string> settings = this.GetServiceSettings(name);
                if (settings.Any())
                {
                    service.Settings = settings;
                }
                else if (service.Settings == null)
                {
                    // I've noticed some developers are not initializing 
                    // the settings in their implentations.
                    service.Settings = new Dictionary<string, string>();
                }

                Uri[] whitelist = this.GetServiceWhitelist(name);

                if (whitelist.Any())
                {
                    service.WhiteList = this.GetServiceWhitelist(name);
                }
            }
        }

        /// <summary>
        /// Returns the <see cref="SettingElementCollection"/> for the given plugin.
        /// </summary>
        /// <param name="name">
        /// The name of the plugin to get the settings for.
        /// </param>
        /// <returns>
        /// The <see cref="SettingElementCollection"/> for the given plugin.
        /// </returns>
        private Dictionary<string, string> GetServiceSettings(string name)
        {
            ImageSecuritySection.ServiceElement serviceElement = this.GetImageSecuritySection()
                .ImageServices
                .Cast<ImageSecuritySection.ServiceElement>()
                .FirstOrDefault(x => x.Name == name);

            Dictionary<string, string> settings;

            if (serviceElement != null)
            {
                settings = serviceElement.Settings
                    .Cast<SettingElement>()
                    .ToDictionary(setting => setting.Key, setting => setting.Value);
            }
            else
            {
                settings = new Dictionary<string, string>();
            }

            return settings;
        }

        /// <summary>
        /// Gets the whitelist of <see cref="System.Uri"/> for the given service.
        /// </summary>
        /// <param name="name">
        /// The name of the service to return the whitelist for.
        /// </param>
        /// <returns>
        /// The <see cref="System.Uri"/> array containing the whitelist.
        /// </returns>
        private Uri[] GetServiceWhitelist(string name)
        {
            ImageSecuritySection.ServiceElement serviceElement = this.GetImageSecuritySection()
               .ImageServices
               .Cast<ImageSecuritySection.ServiceElement>()
               .FirstOrDefault(x => x.Name == name);

            Uri[] whitelist = { };
            if (serviceElement != null)
            {
                whitelist = serviceElement.WhiteList
                    .Cast<ImageSecuritySection.SafeUrl>()
                    .Select(s => s.Url).ToArray();
            }

            return whitelist;
        }
        #endregion

        #region ImageCaches
        /// <summary>
        /// Gets the currently assigned <see cref="IImageCache"/>.
        /// </summary>
        private void LoadImageCache()
        {
            if (this.ImageCache == null)
            {
                string currentCache = GetImageCacheSection().CurrentCache;
                ImageCacheSection.CacheElementCollection caches = imageCacheSection.ImageCaches;

                foreach (ImageCacheSection.CacheElement cache in caches)
                {
                    if (cache.Name == currentCache)
                    {
                        Type type = Type.GetType(cache.Type);

                        if (type == null)
                        {
                            string message = $"Couldn\'t load IImageCache: {cache.Type}";
                            ImageProcessorBootstrapper.Instance.Logger.Log<ImageProcessorConfiguration>(message);
                            throw new TypeLoadException(message);
                        }

                        this.ImageCache = type;
                        this.ImageCacheMaxDays = cache.MaxDays;
                        this.UseFileChangeMonitors = cache.UseFileChangeMonitors;
                        this.BrowserCacheMaxDays = cache.BrowserMaxDays;
                        this.TrimCache = cache.TrimCache;
                        this.FolderDepth = cache.FolderDepth;
                        this.ImageCacheSettings = cache.Settings
                                                       .Cast<SettingElement>()
                                                       .ToDictionary(setting => setting.Key, setting => setting.Value);
                        break;
                    }
                }
            }
        }
        #endregion
        #endregion
    }
}