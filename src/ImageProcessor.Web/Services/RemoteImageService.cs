﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteImageService.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The remote image service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ImageProcessor.Web.Caching;
using ImageProcessor.Web.Helpers;
using Polly;

namespace ImageProcessor.Web.Services
{
    /// <summary>
    /// The remote image service.
    /// </summary>
    public class RemoteImageService : IImageService
    {
        private static readonly HttpClient Client = new HttpClient(RemoteFile.Handler);

        private RemoteFile remoteFile;

        private Dictionary<string, string> settings = new Dictionary<string, string>
        {
            { "MaxBytes", "4194304" },
            { "Timeout", "30000" },
            { "Protocol", "http" },
            { "UserAgent", string.Empty },
            { "MaxRetryAttempts", "3" },
            { "PauseBetweenFailures", "3" }
        };

        /// <summary>
        /// Gets or sets the prefix for the given implementation.
        /// <remarks>
        /// This value is used as a prefix for any image requests that should use this service.
        /// </remarks>
        /// </summary>
        public string Prefix { get; set; } = "remote.axd";

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
        public Uri[] WhiteList { get; set; } = { };

        /// <summary>
        /// Gets a value indicating whether the current request passes sanitizing rules.
        /// </summary>
        /// <param name="path">
        /// The image path.
        /// </param>
        /// <returns>
        /// <c>True</c> if the request is valid; otherwise, <c>False</c>.
        /// </returns>
        public virtual bool IsValidRequest(string path)
        {
            // Check the url is from a whitelisted location.
            var url = new Uri(path);
            string upper = url.Host.ToUpperInvariant();

            // Check for root or sub domain.
            bool validUrl = false;
            foreach (Uri uri in this.WhiteList)
            {
                if (!uri.IsAbsoluteUri)
                {
                    var rebaseUri = new Uri("http://" + uri.ToString().TrimStart('.', '/'));
                    validUrl = upper.StartsWith(rebaseUri.Host.ToUpperInvariant()) || upper.EndsWith(rebaseUri.Host.ToUpperInvariant());
                }
                else
                {
                    validUrl = upper.StartsWith(uri.Host.ToUpperInvariant()) || upper.EndsWith(uri.Host.ToUpperInvariant());
                }

                if (validUrl)
                {
                    break;
                }
            }

            return validUrl;
        }

        /// <summary>
        /// Gets the image using the given identifier.
        /// </summary>
        /// <param name="id">The value identifying the image to fetch.</param>
        /// <returns>
        /// The <see cref="byte"/> array containing the image data.
        /// </returns>
        public virtual async Task<byte[]> GetImage(object id)
        {
            byte[] buffer = new byte[0];

            if (this.remoteFile == null)
            {
                this.InitRemoteFile();
            }

            var retryPolicy = Policy
                    .Handle<HttpRequestException>()
                    .WaitAndRetryAsync(int.Parse(this.Settings["MaxRetryAttempts"]), i => TimeSpan.FromSeconds(int.Parse(this.Settings["PauseBetweenFailures"])));

            await retryPolicy.ExecuteAsync(async () =>
            {
                HttpResponseMessage httpResponse = await this.remoteFile.GetResponseAsync(new Uri(id.ToString())).ConfigureAwait(false);

                if (httpResponse == null)
                {
                    buffer = null;
                }
                else
                {
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
                }
            }).ConfigureAwait(false);

            return buffer;
        }

        private void InitRemoteFile()
        {
            int timeout = int.Parse(this.Settings["Timeout"]);
            int maxDownloadSize = int.Parse(this.Settings["MaxBytes"]);

            this.Settings.TryGetValue("Useragent", out string userAgent);
            this.remoteFile = new RemoteFile(Client, timeout, maxDownloadSize, userAgent);
        }
    }
}