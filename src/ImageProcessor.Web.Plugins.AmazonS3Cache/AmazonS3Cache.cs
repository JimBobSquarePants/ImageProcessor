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
    using System.Globalization;
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
    using ImageProcessor.Web.Extensions;
    using ImageProcessor.Web.Helpers;
    using ImageProcessor.Web.HttpModules;

    /// <summary>
    /// Provides an <see cref="IImageCache"/> implementation that uses Amazon S3 storage.
    /// The cache is self healing and cleaning.
    /// </summary>
    public class AmazonS3Cache : ImageCacheBase
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
        /// The Amazon S3 client.
        /// </summary>
        private readonly AmazonS3Client amazonS3ClientCache;

        /// <summary>
        /// The cached root url for a content delivery network.
        /// </summary>
        private readonly string cachedCdnRoot;

        /// <summary>
        /// The Amazon S3 Access Key.
        /// </summary>
        private readonly string awsAccessKey;

        /// <summary>
        /// The Amazon S3 Secret Key.
        /// </summary>
        private readonly string awsSecretKey;

        /// <summary>
        /// The Amazon S3 Bucket Name.
        /// </summary>
        private readonly string awsBucketName;

        /// <summary>
        /// The Key prefix to use for cache files
        /// </summary>
        private string awsCacheKeyPrefix;

        /// <summary>
        /// Determines if the CDN request is redirected or rewritten
        /// </summary>
        private readonly bool streamCachedImage;

        /// <summary>
        /// The timeout length for requesting the CDN url.
        /// </summary>
        private readonly int timeout = 1000;

        /// <summary>
        /// The cached rewrite path.
        /// </summary>
        private string cachedRewritePath;

        /// <summary>
        /// The cloud source S3 container.
        /// </summary>
        private readonly AmazonS3Client amazonS3SourceCache;

        /// <summary>
        /// The Amazon S3 Source URI.
        /// </summary>
        private readonly string cloudSourceUri;

        /// <summary>
        /// The Amazon S3 Source Bucket Name.
        /// </summary>
        private readonly string amazonS3SourceBucketName;

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
            awsAccessKey = Settings.ContainsKey("AwsAccessKey") ? Settings["AwsAccessKey"] : string.Empty;
            awsSecretKey = Settings.ContainsKey("AwsSecretKey") ? Settings["AwsSecretKey"] : string.Empty;

            this.awsBucketName = this.Settings["AwsBucketName"];
            this.awsCacheKeyPrefix = this.Settings["AwsCacheKeyPrefix"];
            string endPointRegion = Settings.ContainsKey("RegionEndpoint") ? Settings["RegionEndpoint"] : string.Empty;
            RegionEndpoint awsRegionEndpoint = GetRegionEndpoint(endPointRegion);

            if (this.AwsIsValid)
            {
                // If Keys are provided then use these, otherwise we'll use IAM access keys
                this.amazonS3ClientCache = new AmazonS3Client(this.awsAccessKey, this.awsSecretKey, awsRegionEndpoint);
            }
            else if (!string.IsNullOrWhiteSpace(this.awsBucketName))
            {
                this.amazonS3ClientCache = new AmazonS3Client(awsRegionEndpoint);
            }

            this.cachedCdnRoot = this.Settings.ContainsKey("CachedCDNRoot") ? this.Settings["CachedCDNRoot"] : string.Empty;

            if (amazonS3SourceCache == null)
            {
                amazonS3SourceBucketName = Settings.ContainsKey("SourceS3Bucket") ? Settings["SourceS3Bucket"] : string.Empty;
                cloudSourceUri = Settings.ContainsKey("CloudSourceURI") ? Settings["CloudSourceURI"] : string.Empty;

                string sourceAccount = this.Settings.ContainsKey("SourceStorageAccount")
                    ? this.Settings["SourceStorageAccount"]
                    : string.Empty;

                // Repeat for source if it exists
                if (!string.IsNullOrWhiteSpace(amazonS3SourceBucketName))
                {
                    string awsSourceAccessKey = Settings.ContainsKey("AwsSourceAccessKey") ? Settings["AwsSourceAccessKey"] : string.Empty;
                    string awsSourceSecretKey = Settings.ContainsKey("AwsSourceSecretKey") ? Settings["AwsSourceSecretKey"] : string.Empty;
                    string sourceEndPointRegion = Settings.ContainsKey("SourceRegionEndpoint") ? Settings["SourceRegionEndpoint"] : string.Empty;
                    RegionEndpoint awsSourceRegionEndpoint = GetRegionEndpoint(sourceEndPointRegion);

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
                this.timeout = t;
            }

            // This setting was added to facilitate streaming of the blob resource directly instead of a redirect. This is beneficial for CDN purposes
            // but caution should be taken if not used with a CDN as it will add quite a bit of overhead to the site.
            this.streamCachedImage = this.Settings.ContainsKey("StreamCachedImage") && this.Settings["StreamCachedImage"].ToLower() == "true";


        }

        /// <summary>
        /// Gets a value indicating whether Amazon S3 Access Key, Secret Key or Bucket Name are empty strings: 
        /// i.e. whether <see cref="AwsIsValid"/>.
        /// </summary>
        private bool AwsIsValid => !string.IsNullOrWhiteSpace(this.awsAccessKey)
                                   && !string.IsNullOrWhiteSpace(this.awsSecretKey)
                                   && !string.IsNullOrWhiteSpace(this.awsBucketName);

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
            string cachedFileName = await this.CreateCachedFileNameAsync();

            // Collision rate of about 1 in 10000 for the folder structure.
            // That gives us massive scope to store millions of files.
            string pathFromKey = string.Join("\\", cachedFileName.ToCharArray().Take(6));
            this.CachedPath = Path.Combine(this.cachedCdnRoot, this.awsCacheKeyPrefix, pathFromKey, cachedFileName)
                                  .Replace(@"\", "/");

            this.cachedRewritePath = this.CachedPath;

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

                    GetObjectMetadataRequest objectMetaDataRequest = new GetObjectMetadataRequest
                    {
                        BucketName = this.awsBucketName,
                        Key = key,
                    };

                    GetObjectMetadataResponse response = await this.amazonS3ClientCache.GetObjectMetadataAsync(objectMetaDataRequest);

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
                catch (AmazonS3Exception ex)
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
                if (this.IsExpired(cachedImage.CreationTimeUtc) || await this.IsUpdatedAsync(cachedImage.CreationTimeUtc))
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
            TransferUtility transferUtility = new TransferUtility(this.amazonS3ClientCache);

            string path = this.GetFolderStructureForAmazon(this.CachedPath);
            string filename = Path.GetFileName(this.CachedPath);
            string key = this.GetKey(path, filename);

            TransferUtilityUploadRequest transferUtilityUploadRequest = new TransferUtilityUploadRequest
            {
                BucketName = this.awsBucketName,
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

            await transferUtility.UploadAsync(transferUtilityUploadRequest);
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

            ListObjectsRequest request = new ListObjectsRequest
            {
                BucketName = this.awsBucketName,
                Prefix = parent,
                Delimiter = @"/"
            };

            List<S3Object> results = new List<S3Object>();

            do
            {
                ListObjectsResponse response = await this.amazonS3ClientCache.ListObjectsAsync(request);

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
                await this.amazonS3ClientCache.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = this.awsBucketName,
                    Key = file.Key
                });
            }
        }

        /// <summary>
        /// Gets a string identifying the cached file name.
        /// </summary>
        /// <returns>
        /// The asynchronous <see cref="Task"/> returning the value.
        /// </returns>
        //public override async Task<string> CreateCachedFileNameAsync()
        //{
        //    string streamHash = string.Empty;

        //    try
        //    {
        //        if (new Uri(this.RequestPath).IsFile)
        //        {
        //            // Get the hash for the filestream. That way we can ensure that if the image is
        //            // updated but has the same name we will know.
        //            FileInfo imageFileInfo = new FileInfo(this.RequestPath);
        //            if (imageFileInfo.Exists)
        //            {
        //                // Pull the latest info.
        //                imageFileInfo.Refresh();

        //                // Checking the stream itself is far too processor intensive so we make a best guess.
        //                string creation = imageFileInfo.CreationTimeUtc.ToString(CultureInfo.InvariantCulture);
        //                string length = imageFileInfo.Length.ToString(CultureInfo.InvariantCulture);
        //                streamHash = string.Format("{0}{1}", creation, length);
        //            }
        //        }
        //        else
        //        {
        //            // Try and get the headers for the file, this should allow cache busting for remote files.
        //            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.RequestPath);
        //            request.Method = "HEAD";

        //            using (HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync()))
        //            {
        //                string lastModified = response.LastModified.ToUniversalTime().ToString(CultureInfo.InvariantCulture);
        //                string length = response.ContentLength.ToString(CultureInfo.InvariantCulture);
        //                streamHash = string.Format("{0}{1}", lastModified, length);
        //            }
        //        }

        //        // TODO: Pull from other cloud folder/bucket to match the AzureBlobCache behaviour.
        //    }
        //    catch (Exception ex)
        //    {
        //        streamHash = string.Empty;
        //    }

        //    // Use an sha1 hash of the full path including the querystring to create the image name.
        //    // That name can also be used as a key for the cached image and we should be able to use
        //    // The characters of that hash as sub-folders.
        //    string parsedExtension = ImageHelpers.Instance.GetExtension(this.FullPath, this.Querystring);
        //    string encryptedName = (streamHash + this.FullPath).ToSHA1Fingerprint();

        //    string cachedFileName = string.Format(
        //         "{0}.{1}",
        //         encryptedName,
        //         !string.IsNullOrWhiteSpace(parsedExtension) ? parsedExtension.Replace(".", string.Empty) : "jpg");

        //    return cachedFileName;
        //}

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
                request.Timeout = this.timeout;

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
                request.Timeout = this.timeout;

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
            string output = path.Replace(this.cachedCdnRoot, string.Empty);
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
        /// Tries to execute a delegate action five times.
        /// </summary>
        /// <param name="delegateAction">The delegate to be executed</param>
        /// <param name="exceptionAction">The delegate to throw on error.</param>
        private static void TryFiveTimes(Action delegateAction, Action exceptionAction)
        {
            for (int retry = 0; retry < 5; retry++)
            {
                try
                {
                    delegateAction();
                    break;
                }
                catch (Exception)
                {
                    if (retry == 4)
                    {
                        exceptionAction();
                        throw;
                    }
                }
            }
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
        /// <returns>Region Endpoint</returns>
        private RegionEndpoint GetRegionEndpoint(string endpoint)
        {
            string lclEndpoint = endpoint.ToLower();

            if (!String.IsNullOrEmpty(lclEndpoint))
                return  RegionEndpoint.GetBySystemName(lclEndpoint);
            else
                return RegionEndpoint.EUWest1;
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
                if ((Uri.IsWellFormedUriString(this.RequestPath,UriKind.Absolute)) && (new Uri(this.RequestPath).IsFile))
                {
                    if (File.Exists(this.RequestPath))
                    {
                        // If it's newer than the cached file then it must be an update.
                        isUpdated = File.GetLastWriteTimeUtc(this.RequestPath) > creationDate;
                    }
                }
                else if(amazonS3SourceCache != null)
                {
                    string container = RemoteRegex.Replace(this.cloudSourceUri, string.Empty);
                    string s3Path = RemoteRegex.Replace(this.RequestPath, string.Empty);
                    string key = s3Path.Replace(container, string.Empty).TrimStart('/');

                    GetObjectMetadataRequest objectMetaDataRequest = new GetObjectMetadataRequest
                    {
                        BucketName = this.amazonS3SourceBucketName,
                        Key = key,
                    };

                    GetObjectMetadataResponse response = await this.amazonS3ClientCache.GetObjectMetadataAsync(objectMetaDataRequest);

                    if (response != null)
                    {
                        isUpdated = response.LastModified.ToUniversalTime() > creationDate;
                    }
                }
                else
                {
                    // Try and get the headers for the file, this should allow cache busting for remote files.
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.RequestPath);
                    request.Method = "HEAD";

                    using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
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
            DateTime ifModifiedDate;

            string ifModifiedFromRequest = context.Request.Headers["If-Modified-Since"];

            if (DateTime.TryParse(ifModifiedFromRequest, out ifModifiedDate))
            {
                request.IfModifiedSince = ifModifiedDate;
            }
            else
            {
                if (ifModifiedFromRequest.ToLower().Contains("utc"))
                {
                    ifModifiedFromRequest = ifModifiedFromRequest.ToLower().Replace("utc", string.Empty);

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
