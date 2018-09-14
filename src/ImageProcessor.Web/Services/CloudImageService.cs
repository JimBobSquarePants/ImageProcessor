// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CloudImageService.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   A generic cloud image service for retrieving images where the remote location has been rewritten as a
//   a virtual path. Commonly seen in content management systems like Umbraco.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ImageProcessor.Web.Caching;
using ImageProcessor.Web.Helpers;

namespace ImageProcessor.Web.Services
{
    /// <summary>
    /// A generic cloud image service for retrieving images where the remote location has been rewritten as a
    /// a virtual path. Commonly seen in content management systems like Umbraco.
    /// </summary>
    public class CloudImageService : IImageService
    {
        private static readonly HttpClient Client = new HttpClient(RemoteFile.Handler);

        private RemoteFile remoteFile;

        private Dictionary<string, string> settings = new Dictionary<string, string>
        {
            {"MaxBytes", "4194304"},
            {"Timeout", "30000"},
            {"Host", string.Empty}
        };

        /// <summary>
        /// Gets or sets the prefix for the given implementation.
        /// <remarks>
        /// This value is used as a prefix for any image requests that should use this service.
        /// </remarks>
        /// </summary>
        public string Prefix { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the image service requests files from
        /// the locally based file system.
        /// </summary>
        public bool IsFileLocalService => false;

        /// <summary>
        /// Gets or sets any additional settings required by the service.
        /// </summary>
        public Dictionary<string, string> Settings
        {
            get => this.settings;
            set
            {
                this.settings = value;
                this.InitRemoteFile();
            }
        }

        /// <summary>
        /// Gets or sets the white list of <see cref="System.Uri"/>.
        /// </summary>
        public Uri[] WhiteList { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current request passes sanitizing rules.
        /// </summary>
        /// <param name="path">
        /// The image path.
        /// </param>
        /// <returns>
        /// <c>True</c> if the request is valid; otherwise, <c>False</c>.
        /// </returns>
        public virtual bool IsValidRequest(string path) => ImageHelpers.IsValidImageExtension(path);

        /// <summary>
        /// Gets the image using the given identifier.
        /// </summary>
        /// <param name="id">The value identifying the image to fetch.</param>
        /// <returns>
        /// The <see cref="byte"/> array containing the image data.
        /// </returns>
        public virtual async Task<byte[]> GetImage(object id)
        {
            string host = this.Settings["Host"];
            string sasQueryString = this.Settings.ContainsKey("SASQueryString") ? this.Settings["SASQueryString"] : null;
            string container = this.Settings.ContainsKey("Container") ? this.Settings["Container"] : string.Empty;
            var baseUri = new Uri(host);

            string relativeResourceUrl = id.ToString();
            if (!string.IsNullOrEmpty(sasQueryString))
            {
                relativeResourceUrl += (relativeResourceUrl.Contains("?") ? "&" : "?") + sasQueryString.TrimStart('?');
            }

            if (!string.IsNullOrEmpty(container))
            {
                // TODO: Check me.
                container = $"{container.TrimEnd('/')}/";
                if (!relativeResourceUrl.StartsWith($"{container}/"))
                {
                    relativeResourceUrl = $"{container}{relativeResourceUrl.TrimStart('/')}";
                }
            }

            byte[] buffer;
            var uri = new Uri(baseUri, relativeResourceUrl);

            if (this.remoteFile == null)
            {
                this.InitRemoteFile();
            }

            HttpResponseMessage httpResponse = await this.remoteFile.GetResponseAsync(uri).ConfigureAwait(false);

            if (httpResponse == null)
            {
                return null;
            }

            using (MemoryStream memoryStream = MemoryStreamPool.Shared.GetStream())
            {
                using (HttpResponseMessage response = httpResponse)
                {
                    using (Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        await responseStream.CopyToAsync(memoryStream).ConfigureAwait(false);

                        // Reset the position of the stream to ensure we're reading the correct part.
                        memoryStream.Position = 0;
                        buffer = memoryStream.ToArray();
                    }
                }
            }

            return buffer;
        }

        private void InitRemoteFile()
        {
            int timeout = int.Parse(this.Settings["Timeout"]);
            int maxDownloadSize = int.Parse(this.Settings["MaxBytes"]);
            this.remoteFile = new RemoteFile(Client, timeout, maxDownloadSize);
        }
    }
}