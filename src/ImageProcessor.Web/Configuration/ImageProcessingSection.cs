// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageProcessingSection.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Represents an image processing section within a configuration file.
//   Nested syntax adapted from <see href="http://tneustaedter.blogspot.co.uk/2011/09/how-to-create-one-or-more-nested.html" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Configuration
{
    using System.Configuration;
    using System.IO;
    using System.Xml;

    using ImageProcessor.Web.Helpers;

    /// <summary>
    /// Represents an image processing section within a configuration file.
    /// Nested syntax adapted from <see href="http://tneustaedter.blogspot.co.uk/2011/09/how-to-create-one-or-more-nested.html"/>
    /// </summary>
    public sealed class ImageProcessingSection : ConfigurationSection
    {
        /// <summary>
        /// Gets or sets a value indicating whether to preserve exif meta data.
        /// </summary>
        [ConfigurationProperty("preserveExifMetaData", IsRequired = false, DefaultValue = false)]
        public bool PreserveExifMetaData
        {
            get => (bool)this["preserveExifMetaData"];
            set => this["preserveExifMetaData"] = value;
        }

        /// <summary>
        /// Gets or sets the metadata mode to use when processing images.
        /// </summary>
        [ConfigurationProperty("metaDataMode", IsRequired = false, DefaultValue = MetaDataMode.All)]
        public MetaDataMode MetaDataMode
        {
            get => (MetaDataMode)this["metaDataMode"];
            set => this["metaDataMode"] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow known cache busters.
        /// </summary>
        [ConfigurationProperty("allowCacheBuster", IsRequired = false, DefaultValue = true)]
        public bool AllowCacheBuster
        {
            get => (bool)this["allowCacheBuster"];
            set => this["allowCacheBuster"] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to convert images to a linear color space before
        /// processing.
        /// </summary>
        [ConfigurationProperty("fixGamma", IsRequired = false, DefaultValue = false)]
        public bool FixGamma
        {
            get => (bool)this["fixGamma"];
            set => this["fixGamma"] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to intercept all image requests including ones
        /// without querystring parameters.
        /// </summary>
        [ConfigurationProperty("interceptAllRequests", IsRequired = false, DefaultValue = false)]
        public bool InterceptAllRequests
        {
            get => (bool)this["interceptAllRequests"];
            set => this["interceptAllRequests"] = value;
        }

        /// <summary>
        /// Gets the <see cref="T:ImageProcessor.Web.Config.ImageProcessingSection.PresetElementCollection"/>.
        /// </summary>
        /// <value>
        /// The <see cref="T:ImageProcessor.Web.Config.ImageProcessingSection.PresetElementCollection"/>.
        /// </value>
        [ConfigurationProperty("presets", IsRequired = true)]
        public PresetElementCollection Presets => this["presets"] as PresetElementCollection;

        /// <summary>
        /// Gets the <see cref="T:ImageProcessor.Web.Config.ImageProcessingSection.PluginElementCollection"/>.
        /// </summary>
        /// <value>
        /// The <see cref="T:ImageProcessor.Web.Config.ImageProcessingSection.PluginElementCollection"/>.
        /// </value>
        [ConfigurationProperty("plugins", IsRequired = true)]
        public PluginElementCollection Plugins => this["plugins"] as PluginElementCollection;

        /// <summary>
        /// Retrieves the processing configuration section from the current application configuration. 
        /// </summary>
        /// <returns>The processing configuration section from the current application configuration. </returns>
        public static ImageProcessingSection GetConfiguration()
        {
            if (ConfigurationManager.GetSection("imageProcessor/processing") is ImageProcessingSection imageProcessingSection)
            {
                return imageProcessingSection;
            }

            string section = ResourceHelpers.ResourceAsString("ImageProcessor.Web.Configuration.Resources.processing.config.transform");

            using (XmlReader reader = new XmlTextReader(new StringReader(section)))
            {
                imageProcessingSection = new ImageProcessingSection();
                imageProcessingSection.DeserializeSection(reader);
            }

            return imageProcessingSection;
        }

        /// <summary>
        /// Represents a PresetElement configuration element within the configuration.
        /// </summary>
        public class PresetElement : ConfigurationElement
        {
            /// <summary>
            /// Gets or sets the name of the preset.
            /// </summary>
            /// <value>The name of the plugin.</value>
            [ConfigurationProperty("name", DefaultValue = "", IsRequired = true)]
            public string Name
            {
                get => (string)this["name"];

                set => this["name"] = value;
            }

            /// <summary>
            /// Gets or sets the value of the preset.
            /// </summary>
            /// <value>The full Type definition of the plugin</value>
            [ConfigurationProperty("value", DefaultValue = "", IsRequired = true)]
            public string Value
            {
                get => (string)this["value"];

                set => this["value"] = value;
            }
        }

        /// <summary>
        /// Represents a PresetElementCollection collection configuration element within the configuration.
        /// </summary>
        public class PresetElementCollection : ConfigurationElementCollection
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
            protected override string ElementName => "preset";

            /// <summary>
            /// Gets or sets the <see cref="T:ImageProcessor.Web.Config.ImageProcessingSection.PresetElement"/>
            /// at the specified index within the collection.
            /// </summary>
            /// <param name="index">
            /// The index at which to get the specified object.
            /// </param>
            /// <returns>
            /// The <see cref="T:ImageProcessor.Web.Config.ImageProcessingSection.PresetElement"/>
            /// at the specified index within the collection.
            /// </returns>
            public PresetElement this[int index]
            {
                get => (PresetElement)this.BaseGet(index);

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
            /// Creates a new Preset configuration element.
            /// </summary>
            /// <returns>
            /// A new PluginConfig configuration element.
            /// </returns>
            protected override ConfigurationElement CreateNewElement() => new PresetElement();

            /// <summary>
            /// Gets the element key for a specified PluginElement configuration element.
            /// </summary>
            /// <param name="element">
            /// The <see cref="ConfigurationElement">ConfigurationElement</see> 
            /// to return the key for.
            /// </param>
            /// <returns>
            /// The element key for a specified PluginElement configuration element.
            /// </returns>
            protected override object GetElementKey(ConfigurationElement element) => ((PresetElement)element).Name;
        }

        /// <summary>
        /// Represents a PluginElement configuration element within the configuration.
        /// </summary>
        public class PluginElement : ConfigurationElement
        {
            /// <summary>
            /// Gets or sets the name of the plugin file.
            /// </summary>
            [ConfigurationProperty("name", DefaultValue = "", IsRequired = true)]
            public string Name
            {
                get => (string)this["name"];

                set => this["name"] = value;
            }

            /// <summary>
            /// Gets or sets the type of the plugin file.
            /// </summary>
            [ConfigurationProperty("type", DefaultValue = "", IsRequired = true)]
            public string Type
            {
                get => (string)this["type"];

                set => this["type"] = value;
            }

            /// <summary>
            /// Gets or sets a value indiating whether the plugin is enabled.
            /// </summary>
            [ConfigurationProperty("enabled", DefaultValue = "false", IsRequired = false)]
            public bool Enabled
            {
                get => (bool)this["enabled"];

                set => this["enabled"] = value;
            }

            /// <summary>
            /// Gets the <see cref="T:ImageProcessor.Web.Config.ImageProcessingSection.SettingElementCollection"/>.
            /// </summary>
            /// <value>
            /// The <see cref="T:ImageProcessor.Web.Config.ImageProcessingSection.SettingElementCollection"/>.
            /// </value>
            [ConfigurationProperty("settings", IsRequired = false)]
            public SettingElementCollection Settings => this["settings"] as SettingElementCollection;
        }

        /// <summary>
        /// Represents a PluginElementCollection collection configuration element within the configuration.
        /// </summary>
        public class PluginElementCollection : ConfigurationElementCollection
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
            protected override string ElementName => "plugin";

            /// <summary>
            /// Gets or sets the <see cref="T:ImageProcessor.Web.Config.ImageProcessingSection.PluginElement"/>
            /// at the specified index within the collection.
            /// </summary>
            /// <param name="index">
            /// The index at which to get the specified object.
            /// </param>
            /// <returns>
            /// The <see cref="T:ImageProcessor.Web.Config.ImageProcessingSection.PluginElement"/>
            /// at the specified index within the collection.
            /// </returns>
            public PluginElement this[int index]
            {
                get => (PluginElement)this.BaseGet(index);

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
            /// Creates a new Plugin configuration element.
            /// </summary>
            /// <returns>
            /// A new Plugin configuration element.
            /// </returns>
            protected override ConfigurationElement CreateNewElement() => new PluginElement();

            /// <summary>
            /// Gets the element key for a specified PluginElement configuration element.
            /// </summary>
            /// <param name="element">
            /// The <see cref="ConfigurationElement">ConfigurationElement</see> 
            /// to return the key for.
            /// </param>
            /// <returns>
            /// The element key for a specified PluginElement configuration element.
            /// </returns>
            protected override object GetElementKey(ConfigurationElement element) => ((PluginElement)element).Name;
        }
    }
}
