// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AmazonS3Cache.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides an <see cref="IImageCache" /> implementation that uses Amazon S3 storage.
//   The cache is self healing and cleaning.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Plugins.AmazonS3Cache
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;

    using Amazon;
    using Amazon.S3;
    using Amazon.S3.Model;
    using Amazon.S3.Transfer;

    using ImageProcessor.Configuration;
    using ImageProcessor.Web.Caching;
    using ImageProcessor.Web.HttpModules;

    /// <summary>
    /// Provides an <see cref="IImageCache"/> implementation that uses Amazon S3 storage.
    /// The cache is self healing and cleaning.
    /// </summary>
    public class AmazonS3Cache : ImageCacheBase
    {
        private static readonly object SyncLock = new object();

        /// <summary>
        /// The regular expression for parsing a remote uri.
        /// </summary>
        private static readonly Regex RemoteRegex = new Regex("^http(s)?://", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

        /// <summary>
        /// The assembly version.
        /// </summary>
        private static readonly string AssemblyVersion = typeof(ImageProcessingModule).Assembly.GetName().Version.ToString();

        /// <summary>
        /// The Amazon S3 client.
        /// </summary>
        private static AmazonS3Client amazonS3ClientCache;

        /// <summary>
        /// The cloud source S3 container.
        /// </summary>
        private static AmazonS3Client amazonS3SourceCache;

        /// <summary>
        /// The cached root url for a content delivery network.
        /// </summary>
        private static string cachedCdnRoot;

        /// <summary>
        /// The Amazon S3 Access Key.
        /// </summary>
        private static string awsAccessKey;

        /// <summary>
        /// The Amazon S3 Secret Key.
        /// </summary>
        private static string awsSecretKey;

        /// <summary>
        /// The Amazon S3 Bucket Name.
        /// </summary>
        private static string awsBucketName;

        /// <summary>
        /// The Key prefix to use for cache files
        /// </summary>
        private static string awsCacheKeyPrefix;

        /// <summary>
        /// Determines if the CDN request is redirected or rewritten
        /// </summary>
        private static bool streamCachedImage;

        /// <summary>
        /// The timeout length for requesting the CDN url.
        /// </summary>
        private static int timeout = 1000;

        /// <summary>
        /// The Amazon S3 Source URI.
        /// </summary>
        private static string cloudSourceUri;

        /// <summary>
        /// The Amazon S3 Source Bucket Name.
        /// </summary>
        private static string amazonS3SourceBucketName;

        /// <summary>
        /// The cached rewrite path.
        /// </summary>
        private string cachedRewritePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="AmazonS3Cache"/> class.
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
        public AmazonS3Cache(string requestPath, string fullPath, string querystring)
            : base(requestPath, fullPath, querystring)
        {
            if (!(amazonS3ClientCache is null))
            {
                return;
            }

            lock (SyncLock)
            {
                if (!(amazonS3ClientCache is null))
                {
                    return;
                }

                awsAccessKey = this.Settings.ContainsKey("AwsAccessKey") ? this.Settings["AwsAccessKey"] : string.Empty;
                awsSecretKey = this.Settings.ContainsKey("AwsSecretKey") ? this.Settings["AwsSecretKey"] : string.Empty;

                awsBucketName = this.Settings["AwsBucketName"];
                awsCacheKeyPrefix = this.Settings["AwsCacheKeyPrefix"];
                string endPointRegion = this.Settings.ContainsKey("RegionEndpoint") ? this.Settings["RegionEndpoint"] : string.Empty;
                RegionEndpoint awsRegionEndpoint = this.GetRegionEndpoint(endPointRegion);

                if (AwsIsValid)
                {
                    // If Keys are provided then use these, otherwise we'll use IAM access keys
                    amazonS3ClientCache = new AmazonS3Client(awsAccessKey, awsSecretKey, awsRegionEndpoint);
                }
                else if (!string.IsNullOrWhiteSpace(awsBucketName))
                {
                    amazonS3ClientCache = new AmazonS3Client(awsRegionEndpoint);
                }

                cachedCdnRoot = this.Settings.ContainsKey("CachedCDNRoot") ? this.Settings["CachedCDNRoot"] : string.Empty;

                if (amazonS3SourceCache == null)
                {
                    amazonS3SourceBucketName = this.Settings.ContainsKey("SourceS3Bucket") ? this.Settings["SourceS3Bucket"] : string.Empty;
                    cloudSourceUri = this.Settings.ContainsKey("CloudSourceURI") ? this.Settings["CloudSourceURI"] : string.Empty;

                    string sourceAccount = this.Settings.ContainsKey("SourceStorageAccount")
                        ? this.Settings["SourceStorageAccount"]
                        : string.Empty;

                    // Repeat for source if it exists
                    if (!string.IsNullOrWhiteSpace(amazonS3SourceBucketName))
                    {
                        string awsSourceAccessKey = this.Settings.ContainsKey("AwsSourceAccessKey") ? this.Settings["AwsSourceAccessKey"] : string.Empty;
                        string awsSourceSecretKey = this.Settings.ContainsKey("AwsSourceSecretKey") ? this.Settings["AwsSourceSecretKey"] : string.Empty;
                        string sourceEndPointRegion = this.Settings.ContainsKey("SourceRegionEndpoint") ? this.Settings["SourceRegionEndpoint"] : string.Empty;
                        RegionEndpoint awsSourceRegionEndpoint = this.GetRegionEndpoint(sourceEndPointRegion);

                        if (!string.IsNullOrWhiteSpace(awsSourceAccessKey)
                            && !string.IsNullOrWhiteSpace(awsSourceSecretKey))
                        {
                            // If Keys are provided then use these, otherwise we'll use IAM access keys
                            amazonS3SourceCache = new AmazonS3Client(awsSourceAccessKey, awsSourceSecretKey, awsSourceRegionEndpoint);
                        }
                        else
                        {
                            amazonS3SourceCache = new AmazonS3Client(awsSourceRegionEndpoint);
                        }
                    }
                }

                if (this.Settings.ContainsKey("CachedCDNTimeout"))
                {
                    int.TryParse(this.Settings["CachedCDNTimeout"], out int t);
                    timeout = t;
                }

                // This setting was added to facilitate streaming of the blob resource directly instead of a redirect. This is beneficial for CDN purposes
                // but caution should be taken if not used with a CDN as it will add quite a bit of overhead to the site.
                streamCachedImage = this.Settings.ContainsKey("StreamCachedImage")
                    && string.Equals(this.Settings["StreamCachedImage"], "true", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Gets a value indicating whether Amazon S3 Access Key, Secret Key or Bucket Name are empty strings: 
        /// i.e. whether <see cref="AwsIsValid"/>.
        /// </summary>
        private static bool AwsIsValid => !string.IsNullOrWhiteSpace(awsAccessKey)
                                   && !string.IsNullOrWhiteSpace(awsSecretKey)
                                   && !string.IsNullOrWhiteSpace(awsBucketName);

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

            // Collision rate of about 1 in 10000 for the folder structure.
            // That gives us massive scope to store millions of files.
            string pathFromKey = string.Join("\\", cachedFileName.ToCharArray().Take(6));
            this.CachedPath = Path.Combine(cachedCdnRoot, awsCacheKeyPrefix, pathFromKey, cachedFileName)
                                  .Replace(@"\", "/");

            this.cachedRewritePath = this.CachedPath;

            bool isUpdated = false;
            CachedImage cachedImage = CacheIndexer.Get(this.CachedPath);

            if (new Uri(this.CachedPath).IsFile)
            {
                var fileInfo = new FileInfo(this.CachedPath);

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

                    CacheIndexer.Add(cachedImage, this.ImageCacheMaxMinutes);
                }
            }

            if (cachedImage == null)
            {
                try
                {
                    string path = this.GetFolderStructureForAmazon(this.CachedPath);
                    string filename = Path.GetFileName(this.CachedPath);
                    string key = this.GetKey(path, filename);

                    var objectMetaDataRequest = new GetObjectMetadataRequest
                    {
                        BucketName = awsBucketName,
                        Key = key,
                    };

                    GetObjectMetadataResponse response = await amazonS3ClientCache.GetObjectMetadataAsync(objectMetaDataRequest).ConfigureAwait(false);

                    if (response != null)
                    {
                        cachedImage = new CachedImage
                        {
                            Key = key,
                            Path = this.CachedPath,
                            CreationTimeUtc = response.LastModified.ToUniversalTime()
                        };

                        CacheIndexer.Add(cachedImage, this.ImageCacheMaxMinutes);
                    }
                }
                catch (AmazonS3Exception)
                {
                    // Nothing in S3 so we should return true.
                    isUpdated = true;
                }
            }

            if (cachedImage == null)
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
            var transferUtility = new TransferUtility(amazonS3ClientCache);

            string path = this.GetFolderStructureForAmazon(this.CachedPath);
            string filename = Path.GetFileName(this.CachedPath);
            string key = this.GetKey(path, filename);

            var transferUtilityUploadRequest = new TransferUtilityUploadRequest
            {
                BucketName = awsBucketName,
                InputStream = stream,
                Key = key,
                CannedACL = S3CannedACL.PublicRead,
                Headers =
                {
                    CacheControl = string.Format("public, max-age={0}",this.MaxDays* 86400),
                    ContentType = contentType
                }
            };

            transferUtilityUploadRequest.Metadata.Add("x-amz-meta-ImageProcessedBy", "ImageProcessor.Web/" + AssemblyVersion);

            await transferUtility.UploadAsync(transferUtilityUploadRequest).ConfigureAwait(false);
        }

        /// <summary>
        /// Trims the cache of any expired items in an asynchronous manner.
        /// </summary>
        /// <returns>
        /// The asynchronous <see cref="Task"/> representing an asynchronous operation.
        /// </returns>
        public override async Task TrimCacheAsync()
        {
            string path = this.GetFolderStructureForAmazon(this.CachedPath);
            string directory = path.Substring(0, path.LastIndexOf('/'));
            string parent = directory.Substring(0, directory.LastIndexOf('/') + 1);

            var request = new ListObjectsRequest
            {
                BucketName = awsBucketName,
                Prefix = parent,
                Delimiter = "/"
            };

            var results = new List<S3Object>();

            do
            {
                ListObjectsResponse response = await amazonS3ClientCache.ListObjectsAsync(request).ConfigureAwait(false);

                results.AddRange(response.S3Objects);

                // If response is truncated, set the marker to get the next 
                // set of keys.
                if (response.IsTruncated)
                {
                    request.Marker = response.NextMarker;
                }
                else
                {
                    request = null;
                }
            }
            while (request != null);

            foreach (S3Object file in results.OrderBy(x => x.LastModified.ToUniversalTime()))
            {
                if (!this.IsExpired(file.LastModified.ToUniversalTime()))
                {
                    break;
                }

                CacheIndexer.Remove(file.Key);
                await amazonS3ClientCache.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = awsBucketName,
                    Key = file.Key
                }).ConfigureAwait(false);
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
            var request = (HttpWebRequest)WebRequest.Create(this.cachedRewritePath);

            if (streamCachedImage)
            {
                // Map headers to enable 304s to pass through
                if (context.Request.Headers["If-Modified-Since"] != null)
                {
                    TrySetIfModifiedSinceDate(context, request);
                }

                string[] mapRequestHeaders = { "Cache-Control", "If-None-Match" };
                foreach (string h in mapRequestHeaders)
                {
                    if (context.Request.Headers[h] != null)
                    {
                        request.Headers.Add(h, context.Request.Headers[h]);
                    }
                }

                // Write the blob storage directly to the stream
                request.Method = "GET";
                request.Timeout = timeout;

                HttpWebResponse response = null;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    // A 304 is not an error
                    // It appears that some CDN's on Azure (Akamai) do not work properly when making head requests.
                    // They will return a response url and other headers but a 500 status code.
                    if (ex.Response != null && (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotModified
                                                || ex.Response.ResponseUri.AbsoluteUri.Equals(this.cachedRewritePath, StringComparison.OrdinalIgnoreCase)))
                    {
                        response = (HttpWebResponse)ex.Response;
                    }
                    else
                    {
                        response?.Dispose();
                        ImageProcessorBootstrapper.Instance.Logger.Log<AmazonS3Cache>("Unable to stream cached path: " + this.cachedRewritePath);
                        return;
                    }
                }

                Stream cachedStream = response.GetResponseStream();

                if (cachedStream != null)
                {
                    HttpResponse contextResponse = context.Response;

                    // If streaming but not using a CDN the headers will be null.
                    // See https://github.com/JimBobSquarePants/ImageProcessor/pull/466
                    string etagHeader = response.Headers["ETag"];
                    if (!string.IsNullOrWhiteSpace(etagHeader))
                    {
                        contextResponse.Headers.Add("ETag", etagHeader);
                    }

                    string lastModifiedHeader = response.Headers["Last-Modified"];
                    if (!string.IsNullOrWhiteSpace(lastModifiedHeader))
                    {
                        contextResponse.Headers.Add("Last-Modified", lastModifiedHeader);
                    }

                    cachedStream.CopyTo(contextResponse.OutputStream); // Will be empty on 304s
                    ImageProcessingModule.SetHeaders(
                        context,
                        response.StatusCode == HttpStatusCode.NotModified ? null : response.ContentType,
                        null,
                        this.BrowserMaxDays,
                        response.StatusCode);
                }

                cachedStream?.Dispose();
                response.Dispose();
            }
            else
            {
                // Prevent redundant metadata request if paths match.
                if (this.CachedPath == this.cachedRewritePath)
                {
                    ImageProcessingModule.AddCorsRequestHeaders(context);
                    context.Response.Redirect(this.CachedPath, false);
                    return;
                }

                // Redirect the request to the blob URL
                request.Method = "HEAD";
                request.Timeout = timeout;

                HttpWebResponse response = null;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    response.Dispose();
                    ImageProcessingModule.AddCorsRequestHeaders(context);
                    context.Response.Redirect(this.cachedRewritePath, false);
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
                            context.Response.Redirect(this.cachedRewritePath, false);
                        }
                        else
                        {
                            response.Dispose();
                            ImageProcessorBootstrapper.Instance.Logger.Log<AmazonS3Cache>("Unable to rewrite cached path to: " + this.cachedRewritePath);
                        }
                    }
                    else
                    {
                        // It's a 404, we should redirect to the cached path we have just saved to.
                        ImageProcessingModule.AddCorsRequestHeaders(context);
                        context.Response.Redirect(this.CachedPath, false);
                    }
                }
            }
        }

        /// <summary>
        /// Helper to get folder structure from amazon
        /// </summary>
        /// <param name="path">Web path to file's folder</param>
        /// <returns>Key value</returns>
        protected string GetFolderStructureForAmazon(string path)
        {
            string output = path.Replace(cachedCdnRoot, string.Empty);
            output = output.Replace(Path.GetFileName(output), string.Empty);

            if (output.StartsWith("/"))
            {
                output = output.Substring(1);
            }

            if (!output.EndsWith("/"))
            {
                output += "/";
            }

            return output;
        }

        /// <summary>
        /// Helper to construct object key from path
        /// </summary>
        /// <param name="path">Web path to file's folder</param>
        /// <param name="fileName">File name</param>
        /// <returns>Key value</returns>
        private string GetKey(string path, string fileName)
        {
            // Ensure path is relative to root
            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            return Path.Combine(path, fileName);
        }

        /// <summary>
        /// Helper to get AWS Region Endpoint from configuration file
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns>Region Endpoint</returns>
        private RegionEndpoint GetRegionEndpoint(string endpoint)
        {
            string lclEndpoint = endpoint.ToLower();

            if (!string.IsNullOrEmpty(lclEndpoint))
            {
                return RegionEndpoint.GetBySystemName(lclEndpoint);
            }
            else
            {
                return RegionEndpoint.EUWest1;
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
                if ((Uri.IsWellFormedUriString(this.RequestPath, UriKind.Absolute)) && (new Uri(this.RequestPath).IsFile))
                {
                    if (File.Exists(this.RequestPath))
                    {
                        // If it's newer than the cached file then it must be an update.
                        isUpdated = File.GetLastWriteTimeUtc(this.RequestPath) > creationDate;
                    }
                }
                else if (amazonS3SourceCache != null)
                {
                    string container = RemoteRegex.Replace(cloudSourceUri, string.Empty);
                    string s3Path = RemoteRegex.Replace(this.RequestPath, string.Empty);
                    string key = s3Path.Replace(container, string.Empty).TrimStart('/');

                    var objectMetaDataRequest = new GetObjectMetadataRequest
                    {
                        BucketName = amazonS3SourceBucketName,
                        Key = key,
                    };

                    GetObjectMetadataResponse response = await amazonS3ClientCache.GetObjectMetadataAsync(objectMetaDataRequest).ConfigureAwait(false);

                    if (response != null)
                    {
                        isUpdated = response.LastModified.ToUniversalTime() > creationDate;
                    }
                }
                else
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
        /// Tries to set IfModifiedSince header however this crashes when context.Request.Headers["If-Modified-Since"] exists,
        /// but cannot be parsed. It cannot be parsed when it comes from Google Bot as UTC <example>Sun, 27 Nov 2016 20:01:45 UTC</example>
        /// so DateTime.TryParse. If it returns false, then log the error.
        /// </summary>
        /// <param name="context">The current context</param>
        /// <param name="request">The current request</param>
        private static void TrySetIfModifiedSinceDate(HttpContext context, HttpWebRequest request)
        {
            const string IfModifiedSince = "If-Modified-Since";

            if (context.Request.Headers[IfModifiedSince] is null)
            {
                return;
            }

            string ifModifiedFromRequest = context.Request.Headers[IfModifiedSince];

            if (DateTime.TryParse(ifModifiedFromRequest, out DateTime ifModifiedDate))
            {
                request.IfModifiedSince = ifModifiedDate;
            }
            else
            {
                const string Utc = "utc";
                if (ifModifiedFromRequest.IndexOf(Utc, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ifModifiedFromRequest = ifModifiedFromRequest.ToLower().Replace(Utc, string.Empty);

                    if (DateTime.TryParse(ifModifiedFromRequest, out ifModifiedDate))
                    {
                        request.IfModifiedSince = ifModifiedDate;
                    }
                }
                else
                {
                    ImageProcessorBootstrapper.Instance.Logger.Log<AmazonS3Cache>($"Unable to parse date {context.Request.Headers["If-Modified-Since"]} for {context.Request.Url}");
                }
            }
        }
    }
}
