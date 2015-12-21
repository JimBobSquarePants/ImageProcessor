// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AzureBlobCache.cs" company="James South">
//   Copyright (c) James South.
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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;

    using ImageProcessor.Web.Caching;
    using ImageProcessor.Web.Extensions;
    using ImageProcessor.Web.Helpers;
    using ImageProcessor.Web.HttpModules;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Provides an <see cref="IImageCache"/> implementation that uses Azure blob storage.
    /// The cache is self healing and cleaning.
    /// </summary>
    public class AzureBlobCache : ImageCacheBase
    {
        /// <summary>
        /// The regular expression for parsing a remote uri.
        /// </summary>
        private static readonly Regex RemoteRegex = new Regex("^http(s)?://", RegexOptions.Compiled);

        /// <summary>
        /// The assembly version.
        /// </summary>
        private static readonly string AssemblyVersion = typeof(ImageProcessingModule).Assembly.GetName().Version.ToString();

        /// <summary>
        /// The cloud cached blob container.
        /// </summary>
        private readonly CloudBlobContainer cloudCachedBlobContainer;

        /// <summary>
        /// The cloud source blob container.
        /// </summary>
        private readonly CloudBlobContainer cloudSourceBlobContainer;

        /// <summary>
        /// The cached root url for a content delivery network.
        /// </summary>
        private readonly string cachedCdnRoot;

        /// <summary>
        /// Determines if the CDN request is redirected or rewritten
        /// </summary>
        private readonly bool streamCachedImage;

        /// <summary>
        /// The cached rewrite path.
        /// </summary>
        private string cachedRewritePath;

        /// <summary>
        /// The content MIME type.
        /// </summary>
        private string mimeType;

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
        /// The querystring containing instructions.
        /// </param>
        public AzureBlobCache(string requestPath, string fullPath, string querystring)
            : base(requestPath, fullPath, querystring)
        {
            // Retrieve storage accounts from connection string.
            CloudStorageAccount cloudCachedStorageAccount = CloudStorageAccount.Parse(this.Settings["CachedStorageAccount"]);

            // Create the blob clients.
            CloudBlobClient cloudCachedBlobClient = cloudCachedStorageAccount.CreateCloudBlobClient();

            // Retrieve references to a container.
            this.cloudCachedBlobContainer = CreateContainer(cloudCachedBlobClient, this.Settings["CachedBlobContainer"], BlobContainerPublicAccessType.Blob);

            string sourceAccount = this.Settings.ContainsKey("SourceStorageAccount") ? this.Settings["SourceStorageAccount"] : string.Empty;

            // Repeat for source if it exists
            if (!string.IsNullOrWhiteSpace(sourceAccount))
            {
                CloudStorageAccount cloudSourceStorageAccount = CloudStorageAccount.Parse(this.Settings["SourceStorageAccount"]);
                CloudBlobClient cloudSourceBlobClient = cloudSourceStorageAccount.CreateCloudBlobClient();
                this.cloudSourceBlobContainer = cloudSourceBlobClient.GetContainerReference(this.Settings["SourceBlobContainer"]);
            }

            this.cachedCdnRoot = this.Settings.ContainsKey("CachedCDNRoot")
                                     ? this.Settings["CachedCDNRoot"]
                                     : this.cloudCachedBlobContainer.Uri.ToString().TrimEnd(this.cloudCachedBlobContainer.Name.ToCharArray());

            // This setting was added to facilitate streaming of the blob resource directly instead of a redirect. This is beneficial for CDN purposes
            // but caution should be taken if not used with a CDN as it will add quite a bit of overhead to the site. 
            // See: https://github.com/JimBobSquarePants/ImageProcessor/issues/161
            this.streamCachedImage = this.Settings.ContainsKey("StreamCachedImage") && this.Settings["StreamCachedImage"].ToLower() == "true";
        }

        /// <summary>
        /// Gets a value indicating whether the image is new or updated in an asynchronous manner.
        /// </summary>
        /// <returns>
        /// The asynchronous <see cref="Task"/> returning the value.
        /// </returns>
        public override async Task<bool> IsNewOrUpdatedAsync()
        {
            string cachedFileName = await this.CreateCachedFileNameAsync();

            // Collision rate of about 1 in 10000 for the folder structure.
            // That gives us massive scope to store millions of files.
            string pathFromKey = string.Join("\\", cachedFileName.ToCharArray().Take(6));
            this.CachedPath = Path.Combine(this.cloudCachedBlobContainer.Uri.ToString(), pathFromKey, cachedFileName).Replace(@"\", "/");
            this.cachedRewritePath = Path.Combine(this.cachedCdnRoot, this.cloudCachedBlobContainer.Name, pathFromKey, cachedFileName).Replace(@"\", "/");

            bool isUpdated = false;
            CachedImage cachedImage = CacheIndexer.Get(this.CachedPath);

            if (new Uri(this.CachedPath).IsFile)
            {
                FileInfo fileInfo = new FileInfo(this.CachedPath);

                if (fileInfo.Exists)
                {
                    // Pull the latest info.
                    fileInfo.Refresh();

                    cachedImage = new CachedImage
                    {
                        Key = Path.GetFileNameWithoutExtension(this.CachedPath),
                        Path = this.CachedPath,
                        CreationTimeUtc = fileInfo.CreationTimeUtc
                    };

                    CacheIndexer.Add(cachedImage);
                }
            }

            if (cachedImage == null)
            {
                string blobPath = this.CachedPath.Substring(this.cloudCachedBlobContainer.Uri.ToString().Length + 1);
                CloudBlockBlob blockBlob = this.cloudCachedBlobContainer.GetBlockBlobReference(blobPath);

                if (await blockBlob.ExistsAsync())
                {
                    // Pull the latest info.
                    await blockBlob.FetchAttributesAsync();

                    if (blockBlob.Properties.LastModified.HasValue)
                    {
                        cachedImage = new CachedImage
                        {
                            Key = Path.GetFileNameWithoutExtension(this.CachedPath),
                            Path = this.CachedPath,
                            CreationTimeUtc = blockBlob.Properties.LastModified.Value.UtcDateTime
                        };

                        CacheIndexer.Add(cachedImage);
                    }
                }
            }

            if (cachedImage == null)
            {
                // Nothing in the cache so we should return true.
                isUpdated = true;
            }
            else
            {
                // Check to see if the cached image is set to expire.
                if (this.IsExpired(cachedImage.CreationTimeUtc))
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
            this.mimeType = contentType;
            string blobPath = this.CachedPath.Substring(this.cloudCachedBlobContainer.Uri.ToString().Length + 1);
            CloudBlockBlob blockBlob = this.cloudCachedBlobContainer.GetBlockBlobReference(blobPath);

            await blockBlob.UploadFromStreamAsync(stream);

            blockBlob.Properties.ContentType = contentType;
            blockBlob.Properties.CacheControl = string.Format("public, max-age={0}", this.MaxDays * 86400);
            await blockBlob.SetPropertiesAsync();

            blockBlob.Metadata.Add("ImageProcessedBy", "ImageProcessor.Web/" + AssemblyVersion);
            await blockBlob.SetMetadataAsync();
        }

        /// <summary>
        /// Trims the cache of any expired items in an asynchronous manner.
        /// </summary>
        /// <returns>
        /// The asynchronous <see cref="Task"/> representing an asynchronous operation.
        /// </returns>
        public override async Task TrimCacheAsync()
        {
            Uri uri = new Uri(this.CachedPath);
            string path = uri.GetLeftPart(UriPartial.Path).Substring(this.cloudCachedBlobContainer.Uri.ToString().Length + 1);
            string directory = path.Substring(0, path.LastIndexOf('/'));
            string parent = directory.Substring(0, directory.LastIndexOf('/'));

            BlobContinuationToken continuationToken = null;
            List<IListBlobItem> results = new List<IListBlobItem>();

            // Loop through the all the files in a non blocking fashion.
            do
            {
                BlobResultSegment response = await this.cloudCachedBlobContainer
                    .ListBlobsSegmentedAsync(parent, true, BlobListingDetails.Metadata, 5000, continuationToken, null, null);
                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);
            }
            while (continuationToken != null);

            // Now leap through and delete.
            foreach (CloudBlockBlob blob in results
                .Where((blobItem, type) => blobItem is CloudBlockBlob)
                .Cast<CloudBlockBlob>()
                .OrderBy(b => b.Properties.LastModified != null ? b.Properties.LastModified.Value.UtcDateTime : new DateTime()))
            {
                if (blob.Properties.LastModified.HasValue
                    && !this.IsExpired(blob.Properties.LastModified.Value.UtcDateTime))
                {
                    break;
                }

                // Remove from the cache and delete each CachedImage.
                CacheIndexer.Remove(blob.Name);
                await blob.DeleteAsync();
            }
        }

        /// <summary>
        /// Gets a string identifying the cached file name.
        /// </summary>
        /// <returns>
        /// The asynchronous <see cref="Task"/> returning the value.
        /// </returns>
        public override async Task<string> CreateCachedFileNameAsync()
        {
            string streamHash = string.Empty;

            try
            {
                if (new Uri(this.RequestPath).IsFile)
                {
                    // Get the hash for the filestream. That way we can ensure that if the image is
                    // updated but has the same name we will know.
                    FileInfo imageFileInfo = new FileInfo(this.RequestPath);
                    if (imageFileInfo.Exists)
                    {
                        // Pull the latest info.
                        imageFileInfo.Refresh();

                        // Checking the stream itself is far too processor intensive so we make a best guess.
                        string creation = imageFileInfo.CreationTimeUtc.ToString(CultureInfo.InvariantCulture);
                        string length = imageFileInfo.Length.ToString(CultureInfo.InvariantCulture);
                        streamHash = string.Format("{0}{1}", creation, length);
                    }
                }
                else if (this.cloudSourceBlobContainer != null)
                {
                    string container = RemoteRegex.Replace(this.cloudSourceBlobContainer.Uri.ToString(), string.Empty);
                    string blobPath = RemoteRegex.Replace(this.RequestPath, string.Empty);
                    blobPath = blobPath.Replace(container, string.Empty).TrimStart('/');
                    CloudBlockBlob blockBlob = this.cloudSourceBlobContainer.GetBlockBlobReference(blobPath);

                    if (await blockBlob.ExistsAsync())
                    {
                        // Pull the latest info.
                        await blockBlob.FetchAttributesAsync();

                        if (blockBlob.Properties.LastModified.HasValue)
                        {
                            string creation = blockBlob.Properties
                                                       .LastModified.Value.UtcDateTime
                                                       .ToString(CultureInfo.InvariantCulture);

                            string length = blockBlob.Properties.Length.ToString(CultureInfo.InvariantCulture);
                            streamHash = string.Format("{0}{1}", creation, length);
                        }
                    }
                }
                else
                {
                    // Try and get the headers for the file, this should allow cache busting for remote files.
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.RequestPath);
                    request.Method = "HEAD";

                    using (HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync()))
                    {
                        string lastModified = response.LastModified.ToUniversalTime().ToString(CultureInfo.InvariantCulture);
                        string length = response.ContentLength.ToString(CultureInfo.InvariantCulture);
                        streamHash = string.Format("{0}{1}", lastModified, length);
                    }
                }
            }
            catch
            {
                streamHash = string.Empty;
            }

            // Use an sha1 hash of the full path including the querystring to create the image name.
            // That name can also be used as a key for the cached image and we should be able to use
            // The characters of that hash as sub-folders.
            string parsedExtension = ImageHelpers.GetExtension(this.FullPath, this.Querystring);
            string encryptedName = (streamHash + this.FullPath).ToSHA1Fingerprint();

            string cachedFileName = string.Format(
                 "{0}.{1}",
                 encryptedName,
                 !string.IsNullOrWhiteSpace(parsedExtension) ? parsedExtension.Replace(".", string.Empty) : "jpg");

            return cachedFileName;
        }

        /// <summary>
        /// Rewrites the path to point to the cached image.
        /// </summary>
        /// <param name="context">
        /// The <see cref="HttpContext"/> encapsulating all information about the request.
        /// </param>
        public override void RewritePath(HttpContext context)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.cachedRewritePath);

            if (this.streamCachedImage)
            {
                // Write the blob storage directly to the stream
                request.Method = "GET";

                TryFiveTimes(() =>
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        Stream cachedStream = response.GetResponseStream();

                        if (cachedStream != null)
                        {
                            HttpResponse contextResponse = context.Response;
                            cachedStream.CopyTo(contextResponse.OutputStream);

                            // Mimetype can be null when returning from the cache.
                            ImageProcessingModule.SetHeaders(
                                context,
                                string.IsNullOrWhiteSpace(this.mimeType) ? contextResponse.ContentType : this.mimeType,
                                null,
                                this.MaxDays,
                                response.StatusCode);
                        }
                    }
                });
            }
            else
            {
                // Redirect the request to the blob URL
                request.Method = "HEAD";

                TryFiveTimes(() =>
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        HttpStatusCode responseCode = response.StatusCode;
                        ImageProcessingModule.AddCorsRequestHeaders(context);
                        context.Response.Redirect(responseCode == HttpStatusCode.NotFound ? this.CachedPath : this.cachedRewritePath, false);
                    }
                });
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
            container.CreateIfNotExists();
            container.SetPermissions(new BlobContainerPermissions { PublicAccess = accessType });
            return container;
        }

        /// <summary>
        /// Tries to execute a delegate action five times.
        /// </summary>
        /// <param name="delegateAction">The delegate to be executed</param>
        private static void TryFiveTimes(Action delegateAction)
        {
            for (int retry = 0;; retry++)
            {
                try
                {
                    delegateAction();
                    return;
                }
                catch (Exception)
                {
                    if (retry >= 5)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
