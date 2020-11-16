// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageProcessorConfiguration.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates methods to allow the retrieval of ImageProcessor settings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using ImageProcessor.Web.Caching;

namespace ImageProcessor.Web.Configuration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    using ImageProcessor.Configuration;
    using ImageProcessor.Web.Processors;
    using ImageProcessor.Web.Services;

    /// <summary>
    /// Encapsulates methods to allow the retrieval of ImageProcessor settings.
    /// </summary>
    public sealed class ImageProcessorConfiguration
    {
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

        /// <summary>
        /// Prevents a default instance of the <see cref="ImageProcessorConfiguration"/> class from being created.
        /// </summary>
        private ImageProcessorConfiguration()
        {
            this.LoadGraphicsProcessors();
            this.LoadImageServices();
            this.LoadImageCache();
        }

        /// <summary>
        /// Gets the current instance of the <see cref="ImageProcessorConfiguration"/> class.
        /// </summary>
        public static ImageProcessorConfiguration Instance => Lazy.Value;

        /// <summary>
        /// Gets the collection of available processors to run.
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
        /// Gets the maximum number of days to store an image in the cache.
        /// </summary>
        public int ImageCacheMaxDays { get; private set; }

        /// <summary>
        /// Gets the maximum number of minutes to store a cached image reference in memory.
        /// </summary>
        public int ImageCacheMaxMinutes { get; private set; }

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
        /// Gets the image cache rewrite path cache expiry.
        /// </summary>
        public TimeSpan ImageCacheRewritePathExpiry { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to preserve exif meta data.
        /// </summary>
        public bool PreserveExifMetaData => GetImageProcessingSection().PreserveExifMetaData;

        /// <summary>
        /// Gets the metadata mode to use when when processing images.
        /// </summary>
        public MetaDataMode MetaDataMode => GetImageProcessingSection().MetaDataMode;

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
        /// Gets a value indicating whether to intercept all image requests including ones
        /// without querystring parameters.
        /// </summary>
        public bool InterceptAllRequests => GetImageProcessingSection().InterceptAllRequests;

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
        internal ImageSecuritySection GetImageSecuritySection() => imageSecuritySection ?? (imageSecuritySection = ImageSecuritySection.GetConfiguration());

        /// <summary>
        /// Retrieves the processing configuration section from the current application configuration. 
        /// </summary>
        /// <returns>The processing configuration section from the current application configuration. </returns>
        private static ImageProcessingSection GetImageProcessingSection() => imageProcessingSection ?? (imageProcessingSection = ImageProcessingSection.GetConfiguration());

        /// <summary>
        /// Retrieves the caching configuration section from the current application configuration. 
        /// </summary>
        /// <returns>The caching configuration section from the current application configuration. </returns>
        private static ImageCacheSection GetImageCacheSection() => imageCacheSection ?? (imageCacheSection = ImageCacheSection.GetConfiguration());

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
            var processors = new List<IWebGraphicsProcessor>();

            foreach (KeyValuePair<Type, Dictionary<string, string>> pair in this.AvailableWebGraphicsProcessors)
            {
                var processor = (IWebGraphicsProcessor)Activator.CreateInstance(pair.Key);
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
                try
                {
                    var type = Type.GetType(pluginConfig.Type, true);

                    Dictionary<string, string> settings = this.GetPluginSettings(type.Name);

                    // No reason for this to fail.
                    this.AvailableWebGraphicsProcessors.TryAdd(type, settings);
                }
                catch (Exception ex)
                { 
                    string message = "Couldn't load IWebGraphicsProcessor: " + pluginConfig.Type;
                    ImageProcessorBootstrapper.Instance.Logger.Log<ImageProcessorConfiguration>(message);
                    throw new TypeLoadException(message, ex);
                }
            }
        }

        /// <summary>
        /// Returns the <see cref="T:ImageProcessor.Web.Config.ImageProcessingSection.SettingElementCollection"/> for the given plugin.
        /// </summary>
        /// <param name="name">
        /// The name of the plugin to get the settings for. Override settings by adding appsettings in web.config using the format 
        /// ImageProcessor.&lt;.plugin-name.&gt;.&lt;settingKey&gt; e.g. 'ImageProcessor.GaussianBlur.MaxSize'. 
        /// The key must exist in the config section for the appsetting to apply"
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

                // Override the settings found in config section with values in the app.config / deployment slot settings
                this.OverrideDefaultSettingsWithAppSettingsValue(settings, name);
            }
            else
            {
                settings = new Dictionary<string, string>();
            }

            return settings;
        }

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
                var type = Type.GetType(config.Type);

                if (type == null)
                {
                    string message = $"Couldn't load IImageService: {config.Type}";
                    ImageProcessorBootstrapper.Instance.Logger.Log<ImageProcessorConfiguration>(message);
                    throw new TypeLoadException(message);
                }

                var imageService = Activator.CreateInstance(type) as IImageService;

                if (imageService != null)
                {
                    string name = config.Name ?? imageService.GetType().Name;
                    imageService.Prefix = config.Prefix;
                    imageService.Settings = this.GetServiceSettings(name);
                    imageService.WhiteList = this.GetServiceWhitelist(name);
                }

                this.ImageServices.Add(imageService);
            }
        }

        /// <summary>
        /// Returns the <see cref="SettingElementCollection"/> for the given plugin. 
        /// Override the settings using appSettings using the following format "ImageProcessor.&lt;PluginName&gt;.&lt;settingKey&gt; e.g. 'ImageProcessor.CloudImageService.Host'. The key must exist in the config section for the appsetting to apply"
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

                // Override the config section settings with values found in the app.config / deployment slot settings
                this.OverrideDefaultSettingsWithAppSettingsValue(settings, name);
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
                        var type = Type.GetType(cache.Type);

                        if (type == null)
                        {
                            string message = $"Couldn't load IImageCache: {cache.Type}";
                            ImageProcessorBootstrapper.Instance.Logger.Log<ImageProcessorConfiguration>(message);
                            throw new TypeLoadException(message);
                        }

                        this.ImageCache = type;
                        this.ImageCacheMaxDays = cache.MaxDays;
                        this.ImageCacheMaxMinutes = cache.MaxMinutes;
                        this.UseFileChangeMonitors = cache.UseFileChangeMonitors;
                        this.BrowserCacheMaxDays = cache.BrowserMaxDays;
                        this.TrimCache = cache.TrimCache;
                        this.FolderDepth = cache.FolderDepth;
                        this.ImageCacheRewritePathExpiry = cache.CachedRewritePathExpiry;
                        this.ImageCacheSettings = cache.Settings
                                                       .Cast<SettingElement>()
                                                       .ToDictionary(setting => setting.Key, setting => setting.Value);

                        // Override the settings found with values found in the app.config / deployment slot settings
                        this.OverrideDefaultSettingsWithAppSettingsValue(this.ImageCacheSettings, currentCache);

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Override the default settings discovered in the config sections, with settings stored in appsettings of app.config or deployment slot settings (if available)
        /// This will allow the settings to be controlled per deployment slot within Microsoft Azure and similar services
        /// The setting must exist in the config section to be overwritten by the appconfig values
        /// </summary>
        /// <param name="defaultSettings">The list of settings discovered in config section which will be modified with settings found in appSettings</param>
        /// <param name="serviceOrPluginName">The name of the section, used to construct the appSetting key name</param>
        private void OverrideDefaultSettingsWithAppSettingsValue(Dictionary<string, string> defaultSettings, string serviceOrPluginName)
        {
            var copyOfSettingsForEnumeration = new Dictionary<string, string>(defaultSettings);

            // For each default setting found in the config section
            foreach (KeyValuePair<string, string> setting in copyOfSettingsForEnumeration)
            {
                // Check the app settings for a key in the specified format
                string appSettingKeyName = $"ImageProcessor.{serviceOrPluginName}.{setting.Key}";
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[appSettingKeyName]))
                {
                    // If the key is found in app settings use the app settings value rather than the value in the config section
                    defaultSettings[setting.Key] = ConfigurationManager.AppSettings[appSettingKeyName];
                }
            }
        }
    }
}
