// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageCacheSection.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Represents an image cache section within a configuration file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Configuration
{
    using System.Configuration;
    using System.IO;
    using System.Xml;

    using ImageProcessor.Web.Helpers;

    /// <summary>
    /// Represents an image cache section within a configuration file.
    /// </summary>
    public sealed class ImageCacheSection : ConfigurationSection
    {
        /// <summary>
        /// Gets or sets the name of the current cache provider.
        /// </summary>
        /// <value>The name of the cache folder.</value>
        [ConfigurationProperty("currentCache", DefaultValue = "DiskCache", IsRequired = true)]
        public string CurrentCache
        {
            get
            {
                return (string)this["currentCache"];
            }

            set
            {
                this["currentCache"] = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="CacheElementCollection"/>
        /// </summary>
        /// <value>The <see cref="CacheElementCollection"/></value>
        [ConfigurationProperty("caches", IsRequired = true)]
        public CacheElementCollection ImageCaches
        {
            get
            {
                object o = this["caches"];
                return o as CacheElementCollection;
            }
        }

        /// <summary>
        /// Retrieves the cache configuration section from the current application configuration. 
        /// </summary>
        /// <returns>The cache configuration section from the current application configuration.</returns>
        public static ImageCacheSection GetConfiguration()
        {
            ImageCacheSection imageCacheSection = ConfigurationManager.GetSection("imageProcessor/caching") as ImageCacheSection;

            if (imageCacheSection != null)
            {
                return imageCacheSection;
            }

            string section = ResourceHelpers.ResourceAsString("ImageProcessor.Web.Configuration.Resources.cache.config.transform");

            using (XmlReader reader = new XmlTextReader(new StringReader(section)))
            {
                imageCacheSection = new ImageCacheSection();
                imageCacheSection.DeserializeSection(reader);
            }

            return imageCacheSection;
        }

        /// <summary>
        /// Represents a CacheElement configuration element within the configuration.
        /// </summary>
        public class CacheElement : ConfigurationElement
        {
            /// <summary>
            /// Gets or sets the name of the cache.
            /// </summary>
            /// <value>The name of the service.</value>
            [ConfigurationProperty("name", DefaultValue = "", IsRequired = true)]
            public string Name
            {
                get { return (string)this["name"]; }

                set { this["name"] = value; }
            }

            /// <summary>
            /// Gets or sets the type of the cache.
            /// </summary>
            /// <value>The full Type definition of the service</value>
            [ConfigurationProperty("type", DefaultValue = "", IsRequired = true)]
            public string Type
            {
                get { return (string)this["type"]; }

                set { this["type"] = value; }
            }

            /// <summary>
            /// Gets or sets the maximum number of days to store an image in the cache.
            /// </summary>
            /// <value>The maximum number of days to store an image in the cache.</value>
            /// <remarks>Defaults to 365 if not set.</remarks>
            [ConfigurationProperty("maxDays", DefaultValue = "365", IsRequired = true)]
            [IntegerValidator(ExcludeRange = false, MinValue = -1)]
            public int MaxDays
            {
                get
                {
                    return (int)this["maxDays"];
                }

                set
                {
                    this["maxDays"] = value;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the cache will apply file change monitors that can be used to invalidate the cache
            /// </summary>
            /// <value>True or False to enable or disable this setting</value>
            /// <remarks>
            /// Defaults to false, if this is set to true a file change monitor will be created for each cached file that Image Processor creates. 
            /// This could be useful if you want to be able to delete the cached image files in order to trigger the server cache invalidation, however
            /// if this setting is enabled and there are a lot of Image Processor cache files, this could end up causing some issues:
            /// * Performance penalties if hosting on a UNC share
            /// * ASP.Net app domain restarts if using FCNMode="Single" since the FCN buffer can overflow
            /// </remarks>
            [ConfigurationProperty("useFileChangeMonitors", DefaultValue = false, IsRequired = false)]            
            public bool UseFileChangeMonitors
            {
                get { return (bool)this["useFileChangeMonitors"]; }

                set { this["useFileChangeMonitors"] = value; }
            }

            /// <summary>
            /// Gets or sets the maximum number of days to store an image in the browser cache.
            /// </summary>
            /// <value>The maximum number of days to store an image in the browser cache.</value>
            /// <remarks>Defaults to 365 days if not set.</remarks>
            [ConfigurationProperty("browserMaxDays", DefaultValue = "0", IsRequired = false)]
            [IntegerValidator(ExcludeRange = false, MinValue = -1)]
            public int BrowserMaxDays
            {
                get
                {
                    int maxDays = (int)this["browserMaxDays"];
                    if (maxDays == 0)
                    {
                        maxDays = (int)this["maxDays"];
                    }

                    return maxDays;
                }

                set
                {
                    this["browserMaxDays"] = value;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether to periodically trim the cache.
            /// </summary>
            /// <remarks>
            /// Defaults to true.
            /// </remarks>
            [ConfigurationProperty("trimCache", DefaultValue = true, IsRequired = false)]
            public bool TrimCache
            {
                get { return (bool)this["trimCache"]; }

                set { this["trimCache"] = value; }
            }

            /// <summary>
            /// Gets or sets the maximum number folder levels to nest the cached images.
            /// </summary>
            /// <remarks>Defaults to 6 if not set.</remarks>
            [ConfigurationProperty("folderDepth", DefaultValue = "6", IsRequired = false)]
            [IntegerValidator(ExcludeRange = false, MinValue = 0)]
            public int FolderDepth
            {
                get
                {
                    return (int)this["folderDepth"];
                }

                set
                {
                    this["folderDepth"] = value;
                }
            }

            /// <summary>
            /// Gets the <see cref="SettingElementCollection"/>.
            /// </summary>
            /// <value>
            /// The <see cref="SettingElementCollection"/>.
            /// </value>
            [ConfigurationProperty("settings", IsRequired = false)]
            public SettingElementCollection Settings => this["settings"] as SettingElementCollection;
        }

        /// <summary>
        /// Represents a collection of <see cref="CacheElement"/> elements within the configuration.
        /// </summary>
        public class CacheElementCollection : ConfigurationElementCollection
        {
            /// <summary>
            /// Gets the type of the <see cref="ConfigurationElementCollection"/>.
            /// </summary>
            /// <value>
            /// The <see cref="ConfigurationElementCollectionType"/> of this collection.
            /// </value>
            public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;

            /// <summary>
            /// Gets the name used to identify this collection of elements in the configuration file when overridden in a derived class.
            /// </summary>
            /// <value>
            /// The name of the collection; otherwise, an empty string. The default is an empty string.
            /// </value>
            protected override string ElementName => "cache";

            /// <summary>
            /// Gets or sets the <see cref="CacheElement"/>
            /// at the specified index within the collection.
            /// </summary>
            /// <param name="index">The index at which to get the specified object.</param>
            /// <returns>
            /// The <see cref="CacheElement"/>
            /// at the specified index within the collection.
            /// </returns>
            public CacheElement this[int index]
            {
                get
                {
                    return (CacheElement)BaseGet(index);
                }

                set
                {
                    if (this.BaseGet(index) != null)
                    {
                        this.BaseRemoveAt(index);
                    }

                    this.BaseAdd(index, value);
                }
            }

            /// <summary>
            /// When overridden in a derived class, creates a new <see cref="ConfigurationElement"/>.
            /// </summary>
            /// <returns>
            /// A new <see cref="ConfigurationElement"/>.
            /// </returns>
            protected override ConfigurationElement CreateNewElement()
            {
                return new CacheElement();
            }

            /// <summary>
            /// Gets the element key for a specified configuration element when overridden in a derived class.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Object"/> that acts as the key for the specified <see cref="ConfigurationElement"/>.
            /// </returns>
            /// <param name="element">The <see cref="ConfigurationElement"/> to return the key for. </param>
            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((CacheElement)element).Name;
            }
        }
    }
}
