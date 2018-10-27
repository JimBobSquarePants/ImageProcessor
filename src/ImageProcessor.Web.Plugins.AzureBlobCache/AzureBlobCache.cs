// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AzureBlobCache.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides an <see cref="IImageCache" /> implementation that uses Azure blob storage.
//   The cache is self healing and cleaning.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Plugins.AzureBlobCache
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;

    using ImageProcessor.Configuration;
    using ImageProcessor.Web.Caching;
    using ImageProcessor.Web.HttpModules;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Provides an <see cref="IImageCache"/> implementation that uses Azure blob storage.
    /// The cache is self healing and cleaning.
    /// </summary>
    public class AzureBlobCache : ImageCacheBase
    {
        private static readonly object SyncLock = new object();

        /// <summary>
        /// The regular expression for parsing a remote uri.
        /// </summary>
        private static readonly Regex RemoteRegex = new Regex("^http(s)?://", RegexOptions.Compiled);

        /// <summary>
        /// The assembly version.
        /// </summary>
        private static readonly string AssemblyVersion =
            typeof(ImageProcessingModule).Assembly.GetName().Version.ToString();

        /// <summary>
        /// The cloud blob client, thread-safe so can be re-used
        /// </summary>
        private static CloudBlobClient cloudCachedBlobClient;

        /// <summary>
        /// The cloud cached blob container.
        /// </summary>
        private static CloudBlobContainer cloudCachedBlobContainer;

        /// <summary>
        /// The cloud source blob container.
        /// </summary>
        private static CloudBlobContainer cloudSourceBlobContainer;

        /// <summary>
        /// Cache item for Azure private blobs 'Shared Access Signature' (SAS)
        /// </summary>
        private static SASCache sasCache;

        /// <summary>
        /// The cached root url for a content delivery network.
        /// </summary>
        private static string cachedCdnRoot;

        /// <summary>
        /// Determines if the CDN request is redirected or rewritten
        /// </summary>
        private static bool streamCachedImage;

        /// <summary>
        /// The timeout length for requesting the CDN url.
        /// </summary>
        private static int timeout = 1000;

        /// <summary>
        /// Whether to use the cached container name in the Url.
        /// </summary>
        private static bool useCachedContainerInUrl;

        /// <summary>
        /// The cached rewrite path.
        /// </summary>
        private string cachedRewritePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobCache"/> class.
        /// </summary>
        /// <param name="requestPath">
        /// The request path for the image.
        /// </param>
        /// <param name="fullPath">
        /// The full path for the image.
        /// </param>
        /// <param name="querystring">
        /// The query string containing instructions.
        /// </param>
        public AzureBlobCache(string requestPath, string fullPath, string querystring)
            : base(requestPath, fullPath, querystring)
        {
            if (!(cloudCachedBlobContainer is null))
            {
                return;
            }

            lock (SyncLock)
            {
                // Retrieve storage accounts from connection string.
                var cloudCachedStorageAccount = CloudStorageAccount.Parse(this.Settings["CachedStorageAccount"]);

                // Create the blob clients.
                cloudCachedBlobClient = cloudCachedStorageAccount.CreateCloudBlobClient();

                // Set cloud cache container.
                BlobContainerPublicAccessType accessType = this.Settings.ContainsKey("AccessType")
                ? (BlobContainerPublicAccessType)Enum.Parse(typeof(BlobContainerPublicAccessType), this.Settings["AccessType"])
                : BlobContainerPublicAccessType.Blob;

                cloudCachedBlobContainer = CreateContainer(cloudCachedBlobClient, this.Settings["CachedBlobContainer"], accessType);

                // Set cloud source container.
                string sourceAccount = this.Settings.ContainsKey("SourceStorageAccount")
                                           ? this.Settings["SourceStorageAccount"]
                                           : string.Empty;

                // Repeat for source if it exists
                if (!string.IsNullOrWhiteSpace(sourceAccount))
                {
                    var cloudSourceStorageAccount = CloudStorageAccount.Parse(this.Settings["SourceStorageAccount"]);
                    CloudBlobClient cloudSourceBlobClient = cloudSourceStorageAccount.CreateCloudBlobClient();
                    cloudSourceBlobContainer = cloudSourceBlobClient.GetContainerReference(this.Settings["SourceBlobContainer"]);
                }

                // This setting was added to facilitate streaming of the blob resource directly instead of a redirect. This is beneficial for CDN purposes
                // but caution should be taken if not used with a CDN as it will add quite a bit of overhead to the site.
                // See: https://github.com/JimBobSquarePants/ImageProcessor/issues/161
                // If it's private, we also stream the response rather than redirect.
                streamCachedImage = accessType.Equals(BlobContainerPublicAccessType.Off) || (this.Settings.ContainsKey("StreamCachedImage") && string.Equals(this.Settings["StreamCachedImage"], "true", StringComparison.OrdinalIgnoreCase));

                cachedCdnRoot = this.Settings.ContainsKey("CachedCDNRoot")
                     ? this.Settings["CachedCDNRoot"]
                     : cloudCachedBlobContainer.Uri.ToString().TrimEnd(cloudCachedBlobContainer.Name.ToCharArray());

                if (this.Settings.ContainsKey("CachedCDNTimeout"))
                {
                    int.TryParse(this.Settings["CachedCDNTimeout"], out int t);
                    timeout = t;
                }

                // Do we insert the cache container? This seems to break some setups.
                useCachedContainerInUrl = this.Settings.ContainsKey("UseCachedContainerInUrl")
                        && !string.Equals(this.Settings["UseCachedContainerInUrl"], "false", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the image is new or updated in an asynchronous manner.
        /// </summary>
        /// <returns>
        /// The asynchronous <see cref="Task"/> returning the value.
        /// </returns>
        public override async Task<bool> IsNewOrUpdatedAsync()
        {
            // TODO: Before this check is performed it should be throttled. For example, only perform this check
            // if the last time it was checked is greater than 5 seconds. This would be much better for perf
            // if there is a high throughput of image requests.
            string cachedFileName = await this.CreateCachedFileNameAsync().ConfigureAwait(false);
            this.CachedPath = CachedImageHelper.GetCachedPath(cloudCachedBlobContainer.Uri.ToString(), cachedFileName, true, this.FolderDepth);

            this.cachedRewritePath = CachedImageHelper.GetCachedPath(useCachedContainerInUrl
                ? Path.Combine(cachedCdnRoot, cloudCachedBlobContainer.Name)
                : cachedCdnRoot, cachedFileName, true, this.FolderDepth);

            bool isUpdated = false;
            CachedImage cachedImage = CacheIndexer.Get(this.CachedPath);

            if (new Uri(this.CachedPath).IsFile && File.Exists(this.CachedPath))
            {
                cachedImage = new CachedImage
                {
                    Key = Path.GetFileNameWithoutExtension(this.CachedPath),
                    Path = this.CachedPath,
                    CreationTimeUtc = File.GetCreationTimeUtc(this.CachedPath)
                };

                CacheIndexer.Add(cachedImage, this.ImageCacheMaxMinutes);
            }

            if (cachedImage is null)
            {
                string blobPath = this.CachedPath.Substring(cloudCachedBlobContainer.Uri.ToString().Length + 1);
                CloudBlockBlob blockBlob = cloudCachedBlobContainer.GetBlockBlobReference(blobPath);

                if (await blockBlob.ExistsAsync().ConfigureAwait(false))
                {
                    // Pull the latest info.
                    await blockBlob.FetchAttributesAsync().ConfigureAwait(false);

                    if (blockBlob.Properties.LastModified.HasValue)
                    {
                        cachedImage = new CachedImage
                        {
                            Key = Path.GetFileNameWithoutExtension(this.CachedPath),
                            Path = this.CachedPath,
                            CreationTimeUtc = blockBlob.Properties.LastModified.Value.UtcDateTime
                        };

                        CacheIndexer.Add(cachedImage, this.ImageCacheMaxMinutes);
                    }
                }
            }

            if (cachedImage is null)
            {
                // Nothing in the cache so we should return true.
                isUpdated = true;
            }
            else
            {
                // Check to see if the cached image is set to expire
                // or a new file with the same name has replaced our current image
                if (this.IsExpired(cachedImage.CreationTimeUtc) || await this.IsUpdatedAsync(cachedImage.CreationTimeUtc).ConfigureAwait(false))
                {
                    CacheIndexer.Remove(this.CachedPath);
                    isUpdated = true;
                }
            }

            return isUpdated;
        }

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
        public override async Task AddImageToCacheAsync(Stream stream, string contentType)
        {
            string blobPath = this.CachedPath.Substring(cloudCachedBlobContainer.Uri.ToString().Length + 1);
            CloudBlockBlob blockBlob = cloudCachedBlobContainer.GetBlockBlobReference(blobPath);

            await blockBlob.UploadFromStreamAsync(stream).ConfigureAwait(false);

            blockBlob.Properties.ContentType = contentType;
            blockBlob.Properties.CacheControl = $"public, max-age={this.BrowserMaxDays * 86400}";
            await blockBlob.SetPropertiesAsync().ConfigureAwait(false);

            blockBlob.Metadata.Add("ImageProcessedBy", "ImageProcessor.Web/" + AssemblyVersion);
            await blockBlob.SetMetadataAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Trims the cache of any expired items in an asynchronous manner.
        /// </summary>
        /// <returns>
        /// The asynchronous <see cref="Task"/> representing an asynchronous operation.
        /// </returns>
        public override Task TrimCacheAsync()
        {
            if (!this.TrimCache)
            {
                return Task.FromResult(0);
            }

            this.ScheduleCacheTrimmer(async token =>
            {
                // Jump up to the parent branch to clean through the cache.
                string parent = string.Empty;

                if (this.FolderDepth > 0)
                {
                    var uri = new Uri(this.CachedPath);
                    string path = uri.GetLeftPart(UriPartial.Path).Substring(cloudCachedBlobContainer.Uri.ToString().Length + 1);
                    parent = path.Substring(0, 2);
                }

                BlobContinuationToken continuationToken = null;
                var results = new List<IListBlobItem>();

                // Loop through the all the files in a non blocking fashion.
                do
                {
                    BlobResultSegment response = await cloudCachedBlobContainer.ListBlobsSegmentedAsync(parent, true, BlobListingDetails.Metadata, 5000, continuationToken, null, null, token).ConfigureAwait(false);
                    continuationToken = response.ContinuationToken;
                    results.AddRange(response.Results);
                }
                while (!token.IsCancellationRequested && continuationToken != null);

                // Now leap through and delete.
                foreach (
                    CloudBlockBlob blob in
                    results.OfType<CloudBlockBlob>()
                           .OrderBy(b => b.Properties.LastModified?.UtcDateTime ?? new DateTime()))
                {
                    if (token.IsCancellationRequested || (blob.Properties.LastModified.HasValue && !this.IsExpired(blob.Properties.LastModified.Value.UtcDateTime)))
                    {
                        break;
                    }

                    // Remove from the cache and delete each CachedImage.
                    CacheIndexer.Remove(blob.Name);
                    await blob.DeleteAsync(token).ConfigureAwait(false);
                }
            });

            return Task.FromResult(0);
        }
        private string GetPathWithSasTokenQuery(string path)
        {
            if (cloudCachedBlobClient.Credentials.IsSharedKey)
            {
                BlobContainerPermissions permissions = cloudCachedBlobContainer.GetPermissions();
                if (permissions.PublicAccess != BlobContainerPublicAccessType.Off)
                {
                    // nothing is required for public blobs
                    return path;
                }
            }
            if (sasCache != null)
            {
                if (sasCache.CreationTime.AddMinutes(sasCache.ValidityMinutes / 2) > DateTime.UtcNow)
                {
                    return path + sasCache.SASQueryString;
                }
            }
            string sasQueryString = Settings.ContainsKey("SASQueryString") ? Settings["SASQueryString"] : null;
            if (!cloudCachedBlobClient.Credentials.IsSharedKey && string.IsNullOrEmpty(sasQueryString))
            {
                return path;
            }
            if (!cloudCachedBlobClient.Credentials.IsSharedKey)
            {
                return path + sasQueryString;
            }
            if (path.Contains("?"))
            {
                return path;
            }
            else
            {
                // Shared Access Signatures can only be generated under shared key credentials!
                string sasValidityMinutesSetting = Settings.ContainsKey("SASValidityInMinutes") ? Settings["SASValidityInMinutes"] : null;
                double.TryParse(sasValidityMinutesSetting, out double sasValidityMinutes);
                if (sasValidityMinutes <= 5)
                {
                    sasValidityMinutes = 30;
                }
                SharedAccessBlobPolicy accessPolicy = new SharedAccessBlobPolicy()
                {
                    Permissions = SharedAccessBlobPermissions.Read,
                    SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                    SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(sasValidityMinutes)
                };
                sasQueryString = cloudCachedBlobContainer.GetSharedAccessSignature(accessPolicy);
                sasCache = new SASCache()
                {
                    SASQueryString = sasQueryString,
                    CreationTime = DateTime.UtcNow,
                    ValidityMinutes = sasValidityMinutes
                };
                return path + sasQueryString;
            }
        }
        /// <summary>
        /// Rewrites the path to point to the cached image.
        /// </summary>
        /// <param name="context">
        /// The <see cref="HttpContext"/> encapsulating all information about the request.
        /// </param>
        public override void RewritePath(HttpContext context)
        {
            if (streamCachedImage)
            {
                string blobPath = this.CachedPath.Substring(cloudCachedBlobContainer.Uri.ToString().Length + 1);
                CloudBlockBlob blockBlob = cloudCachedBlobContainer.GetBlockBlobReference(blobPath);

                if (blockBlob.Exists())
                {
                    using (MemoryStream cachedStream = MemoryStreamPool.Shared.GetStream())
                    {
                        // Map headers to enable 304s to pass through
                        TrySetIfModifiedSinceDate(context, out AccessCondition accessCondition);
                        const string IfNoneMatch = "If-None-Match";

                        // TODO: Cache-Control?
                        if (context.Request.Headers[IfNoneMatch] != null)
                        {
                            accessCondition.IfNoneMatchETag = context.Request.Headers[IfNoneMatch];
                        }

                        bool is304 = false;
                        try
                        {
                            blockBlob.DownloadToStream(cachedStream, accessCondition);
                            cachedStream.Position = 0;
                        }
                        catch (StorageException ex)
                        {
                            // A 304 is not a true error, we still need to feed back.
                            var webException = ex.InnerException as WebException;
                            if (!(webException is null))
                            {
                                if (webException.Response != null && (((HttpWebResponse)webException.Response).StatusCode == HttpStatusCode.NotModified))
                                {
                                    is304 = true;
                                }
                                else
                                {
                                    ImageProcessorBootstrapper.Instance.Logger.Log<AzureBlobCache>("Unable to stream cached path: " + this.cachedRewritePath);
                                    return;
                                }
                            }
                        }

                        BlobProperties properties = blockBlob.Properties;
                        HttpResponse contextResponse = context.Response;

                        if (!string.IsNullOrWhiteSpace(properties.ETag))
                        {
                            contextResponse.Headers.Add("ETag", properties.ETag);
                        }

                        if (properties.LastModified.HasValue)
                        {
                            contextResponse.Headers.Add("Last-Modified", properties.LastModified.Value.UtcDateTime.ToString("R"));
                        }

                        cachedStream.CopyTo(contextResponse.OutputStream); // Will be empty on 304s

                        if (contextResponse.OutputStream.CanSeek)
                        {
                            contextResponse.OutputStream.Position = 0;
                        }

                        ImageProcessingModule.SetHeaders(
                            context,
                            !is304 ? properties.ContentType : null,
                            null,
                            this.BrowserMaxDays,
                            !is304 ? HttpStatusCode.OK : HttpStatusCode.NotModified);
                    }
                }
            }
            else
            {
                // Prevent redundant metadata request if paths match.
                if (this.CachedPath == this.cachedRewritePath)
                {
                    ImageProcessingModule.AddCorsRequestHeaders(context);
                    context.Response.Redirect(GetPathWithSasTokenQuery(this.CachedPath), false);
                    return;
                }

                var request = (HttpWebRequest)WebRequest.Create(this.cachedRewritePath);

                // Redirect the request to the blob URL
                request.Method = "HEAD";
                request.Timeout = timeout;

                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    response.Dispose();
                    ImageProcessingModule.AddCorsRequestHeaders(context);
                    context.Response.Redirect(GetPathWithSasTokenQuery(this.cachedRewritePath), false);
                }
                catch (WebException ex)
                {
                    response = (HttpWebResponse)ex.Response;

                    if (response != null)
                    {
                        HttpStatusCode responseCode = response.StatusCode;

                        // A 304 is not an error
                        // It appears that some CDN's on Azure (Akamai) do not work properly when making head requests.
                        // They will return a response url and other headers but a 500 status code.
                        if (responseCode == HttpStatusCode.NotModified
                            || response.ResponseUri.AbsoluteUri.Equals(this.cachedRewritePath, StringComparison.OrdinalIgnoreCase))
                        {
                            response.Dispose();
                            ImageProcessingModule.AddCorsRequestHeaders(context);
                            context.Response.Redirect(GetPathWithSasTokenQuery(this.cachedRewritePath), false);
                        }
                        else
                        {
                            response.Dispose();
                            ImageProcessorBootstrapper.Instance.Logger.Log<AzureBlobCache>("Unable to rewrite cached path to: " + this.cachedRewritePath);
                        }
                    }
                    else
                    {
                        // It's a 404, we should redirect to the cached path we have just saved to.
                        ImageProcessingModule.AddCorsRequestHeaders(context);
                        context.Response.Redirect(GetPathWithSasTokenQuery(this.CachedPath), false);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a value indicating whether the requested image has been updated.
        /// </summary>
        /// <param name="creationDate">The creation date.</param>
        /// <returns>The <see cref="bool"/></returns>
        protected virtual async Task<bool> IsUpdatedAsync(DateTime creationDate)
        {
            bool isUpdated = false;

            try
            {
                if (new Uri(this.RequestPath).IsFile)
                {
                    if (File.Exists(this.RequestPath))
                    {
                        // If it's newer than the cached file then it must be an update.
                        isUpdated = File.GetLastWriteTimeUtc(this.RequestPath) > creationDate;
                    }
                }
                else if (cloudSourceBlobContainer != null)
                {
                    string container = RemoteRegex.Replace(cloudSourceBlobContainer.Uri.ToString(), string.Empty);
                    string blobPath = RemoteRegex.Replace(this.RequestPath, string.Empty);
                    blobPath = blobPath.Replace(container, string.Empty).TrimStart('/');
                    CloudBlockBlob blockBlob = cloudSourceBlobContainer.GetBlockBlobReference(blobPath);

                    if (await blockBlob.ExistsAsync().ConfigureAwait(false))
                    {
                        // Pull the latest info.
                        await blockBlob.FetchAttributesAsync().ConfigureAwait(false);

                        if (blockBlob.Properties.LastModified.HasValue)
                        {
                            isUpdated = blockBlob.Properties.LastModified.Value.UtcDateTime > creationDate;
                        }
                    }
                }
                else if (Uri.IsWellFormedUriString(this.RequestPath, UriKind.Absolute))
                {
                    // Try and get the headers for the file, this should allow cache busting for remote files.
                    var request = (HttpWebRequest)WebRequest.Create(this.RequestPath);
                    request.Method = "HEAD";

                    using (var response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
                    {
                        isUpdated = response.LastModified.ToUniversalTime() > creationDate;
                    }
                }
            }
            catch
            {
                isUpdated = false;
            }

            return isUpdated;
        }

        /// <summary>
        /// Tries to set If-Modified-Since header however this crashes when context.Request.Headers["If-Modified-Since"] exists,
        /// but cannot be parsed. It cannot be parsed when it comes from Google Bot as UTC <example>Sun, 27 Nov 2016 20:01:45 UTC</example>
        /// so DateTime.TryParse. If it returns false, then log the error.
        /// </summary>
        /// <param name="context">The current context</param>
        /// <param name="accessCondition">The access condition.</param>
        private static void TrySetIfModifiedSinceDate(HttpContext context, out AccessCondition accessCondition)
        {
            accessCondition = new AccessCondition();
            const string IfModifiedSince = "If-Modified-Since";

            if (context.Request.Headers[IfModifiedSince] is null)
            {
                return;
            }

            string ifModifiedFromRequest = context.Request.Headers[IfModifiedSince];

            if (DateTime.TryParse(ifModifiedFromRequest, out DateTime ifModifiedDate))
            {
                accessCondition.IfModifiedSinceTime = ifModifiedDate;
            }
            else
            {
                const string Utc = "utc";
                if (ifModifiedFromRequest.IndexOf(Utc, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ifModifiedFromRequest = ifModifiedFromRequest.ToLower().Replace(Utc, string.Empty);

                    if (DateTime.TryParse(ifModifiedFromRequest, out ifModifiedDate))
                    {
                        accessCondition.IfModifiedSinceTime = ifModifiedDate;
                    }
                }
                else
                {
                    ImageProcessorBootstrapper.Instance.Logger.Log<AzureBlobCache>($"Unable to parse date {context.Request.Headers[IfModifiedSince]} for {context.Request.Url}");
                }
            }
        }

        /// <summary>
        /// Returns the cache container, creating a new one if none exists.
        /// </summary>
        /// <param name="cloudBlobClient"><see cref="CloudBlobClient"/> where the container is stored.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="accessType"><see cref="BlobContainerPublicAccessType"/> indicating the access permissions.</param>
        /// <returns>The <see cref="CloudBlobContainer"/></returns>
        private static CloudBlobContainer CreateContainer(CloudBlobClient cloudBlobClient, string containerName, BlobContainerPublicAccessType accessType)
        {
            CloudBlobContainer container = cloudBlobClient.GetContainerReference(containerName);
            if (!container.Exists())
            {
                if (cloudBlobClient.Credentials.IsSAS)
                {
                    // Shared access signatures (SAS) have some limitations compared to shared access keys
                    // read more on: https://docs.microsoft.com/en-us/azure/storage/common/storage-dotnet-shared-access-signature-part-1
                    string[] sasTokenProperties = cloudBlobClient.Credentials.SASToken.Split("&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    bool isAccountSas = sasTokenProperties.Where(k => k.ToLowerInvariant().StartsWith("ss=")).FirstOrDefault() != null;

                    string allowedServices = sasTokenProperties.Where(k => k.ToLowerInvariant().StartsWith("ss=")).FirstOrDefault();
                    if (allowedServices != null)
                    {
                        allowedServices = allowedServices.Split('=')[1].ToLower();
                    }
                    else
                    {
                        allowedServices = string.Empty;
                    }

                    string resourceTypes = sasTokenProperties.Where(k => k.ToLowerInvariant().StartsWith("srt=")).FirstOrDefault();
                    if (resourceTypes != null)
                    {
                        resourceTypes = resourceTypes.Split('=')[1].ToLower();
                    }
                    else
                    {
                        resourceTypes = string.Empty;
                    }

                    string permissions = sasTokenProperties.Where(k => k.ToLowerInvariant().StartsWith("sp=")).FirstOrDefault();
                    if (permissions != null)
                    {
                        permissions = permissions.Split('=')[1].ToLower();
                    }
                    else
                    {
                        permissions = string.Empty;
                    }

                    bool canCreateContainer = allowedServices.Contains('b') && resourceTypes.Contains('c') && permissions.Contains('c');
                    if (canCreateContainer)
                    {
                        container.CreateIfNotExists();

                        // cannot set permissions with sas access
                    }
                }
                else
                {
                    container.Create();
                    container.SetPermissions(new BlobContainerPermissions { PublicAccess = accessType });
                }

            }

            return container;
        }
    }
}
