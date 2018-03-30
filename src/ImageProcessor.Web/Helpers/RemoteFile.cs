// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteFile.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates methods used to download files from a website address.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using ImageProcessor.Configuration;
using ImageProcessor.Web.HttpModules;

namespace ImageProcessor.Web.Helpers
{
    /// <summary>
    /// Encapsulates methods used to download files from a website address.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The purpose of this class is so there's one core way of downloading remote files with url[s] that are from
    /// outside users. There's various areas in application where an attacker could supply an external url to the server
    /// and tie up resources.
    /// </para>
    /// For example, the ImageProcessingModule accepts off-server addresses as a path. An attacker could, for instance, pass the url
    /// to a file that's a few gigs in size, causing the server to get out-of-memory exceptions or some other errors. An attacker
    /// could also use this same method to use one application instance to hammer another site by, again, passing an off-server
    /// address of the victims site to the <see cref="ImageProcessingModule"/>.
    /// This class will not throw an exception if the Uri supplied points to a resource local to the running application instance.
    /// <para>
    /// There shouldn't be any security issues there, as the internal <see cref="HttpClient"/> instance is still calling it remotely.
    /// Any local files that shouldn't be accessed by this won't be allowed by the remote call.
    /// </para>
    /// </remarks>
    internal sealed class RemoteFile
    {
        private static readonly HttpClientHandler Handler = new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            Credentials = CredentialCache.DefaultNetworkCredentials
        };

        private static readonly HttpClient Client = new HttpClient(Handler);

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteFile"/> class.
        /// </summary>
        /// <param name="timeoutMilliseconds">The maximum time, in milliseconds, to wait before the request times out.</param>
        /// <param name="maxDownloadSize">The maximum download size, in bytes, that a remote file download attempt can be.</param>
        public RemoteFile(int timeoutMilliseconds, int maxDownloadSize)
            : this(timeoutMilliseconds, maxDownloadSize, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteFile"/> class.
        /// </summary>
        /// <param name="timeoutMilliseconds">The maximum time, in milliseconds, to wait before the request times out.</param>
        /// <param name="maxDownloadSize">The maximum download size, in bytes, that a remote file download attempt can be.</param>
        /// <param name="userAgent">The User-Agent header to be passed when requesting the remote file.</param>
        public RemoteFile(int timeoutMilliseconds, int maxDownloadSize, string userAgent)
        {
            if (timeoutMilliseconds >= 0)
            {
                this.Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
            }

            if (maxDownloadSize > 0)
            {
                this.MaxDownloadSize = maxDownloadSize;
            }

            this.UserAgent = userAgent;

            // We're reusing the same static HttpClient so we don't exhaust the number of sockets available under heavy loads.
            // We're always using the same timeout, and user-agent values per all instances so it's ok to set the values here per instance.
            Client.Timeout = this.Timeout;
            var key = "User-Agent";
            if (!string.IsNullOrWhiteSpace(this.UserAgent) && !Client.DefaultRequestHeaders.TryGetValues(key, out var _))
            {
                Client.DefaultRequestHeaders.Add(key, this.UserAgent);
            }
        }

        /// <summary>
        /// Gets or sets the timespan to wait before the request times out.
        /// </summary>
        public TimeSpan Timeout { get; } = TimeSpan.FromSeconds(100.0);

        /// <summary>
        /// Gets the maximum download size, in bytes, that a remote file download attempt can be.
        /// </summary>
        public int MaxDownloadSize { get; } = int.MaxValue;

        /// <summary>
        /// Gets the UserAgent header to be passed when requesting the remote file
        /// </summary>
        public string UserAgent { get; }

        /// <summary>
        /// Returns the <see cref="HttpResponseMessage"/> used to download this file.
        /// <remarks>
        /// <para>
        /// This method is meant for outside users who need specific access to the HttpResponseMessage this class
        /// generates. They're responsible for disposing of it.
        /// </para>
        /// </remarks>
        /// </summary>
        /// <returns>The <see cref="HttpResponseMessage"/> used to download this file.</returns>
        /// <returns>
        /// The <see cref="IEnumerable{T}"/>.
        /// </returns>
        internal async Task<HttpResponseMessage> GetResponseAsync(Uri uri)
        {
            void Log(HttpRequestException ex)
            {
                ImageProcessorBootstrapper.Instance.Logger.Log<RemoteFile>(ex.Message);
            }

            HttpResponseMessage response = null;
            HttpStatusCode statusCode = HttpStatusCode.OK;
            try
            {
                response = await Client.GetAsync(uri).ConfigureAwait(false);
                statusCode = response.StatusCode;
                response.EnsureSuccessStatusCode();

                long? contentLength = response.Content.Headers.ContentLength;
                if (contentLength.HasValue)
                {
                    if (contentLength > this.MaxDownloadSize)
                    {
                        response.Dispose();
                        string message = "An attempt to download a remote file has been halted because the file is larger than allowed.";
                        ImageProcessorBootstrapper.Instance.Logger.Log<RemoteFile>(message);
                        throw new SecurityException(message);
                    }
                }
                else
                {
                    response.Dispose();
                    throw new HttpException((int)HttpStatusCode.NotFound, $"No image exists at {uri}");
                }

                return response;
            }
            catch (HttpRequestException ex)
            {
                switch (statusCode)
                {
                    // No error, carry on.
                    case HttpStatusCode.NotModified:
                        break;
                    case HttpStatusCode.NotFound:

                        // We want 404's to be handled by IIS so that other handlers/modules can still run.
                        Log(ex);
                        response?.Dispose();
                        throw new HttpException((int)statusCode, $"No image exists at {uri}", ex);

                    default:
                        Log(ex);
                        break;
                }
            }

            return null;
        }
    }
}