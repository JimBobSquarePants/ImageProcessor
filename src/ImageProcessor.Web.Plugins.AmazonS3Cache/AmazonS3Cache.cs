// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AzureBlobCache.cs" company="James South">
//   Copyright (c) James South.
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

    using ImageProcessor.Web.Caching;
    using ImageProcessor.Web.Extensions;
    using ImageProcessor.Web.Helpers;
    using ImageProcessor.Web.HttpModules;

    using Amazon;
    using Amazon.S3;
    using Amazon.S3.Model;
    using Amazon.S3.Transfer;

    /// <summary>
    /// Provides an <see cref="IImageCache"/> implementation that uses Amazon S3 storage.
    /// The cache is self healing and cleaning.
    /// </summary>
    public class AmazonS3Cache : ImageCacheBase
    {
        /// <summary>
        /// Image Processor Cache folder in S3.
        /// </summary>
        private string ImageProcessorCachePrefix = @"imageprocessor_cache/";

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
        /// The Amazon S3 client.
        /// </summary>
        private readonly AmazonS3Client s3ClientCache;

        /// <summary>
        /// The cached root url for a content delivery network.
        /// </summary>
        private readonly string cachedCdnRoot;

        /// <summary>
        /// Return false if Amazon S3 Access Key, Secret Key or Bucket Name are empty strings.
        /// </summary>
        private bool AwsIsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(AwsAccessKey) && !string.IsNullOrWhiteSpace(AwsSecretKey)
                       && !string.IsNullOrWhiteSpace(AwsBucketName);
            }
        }

        /// <summary>
        /// Amazon S3 Access Key.
        /// </summary>
        private string AwsAccessKey { get; set; }

        /// <summary>
        /// Amazon S3 Secret Key.
        /// </summary>
        private string AwsSecretKey { get; set; }

        /// <summary>
        /// Amazon S3 Bucket Name.
        /// </summary>
        private string AwsBucketName { get; set; }

        /// <summary>
        /// Amazon S3 Region Endpoint.
        /// </summary>
        private RegionEndpoint AwsRegionEndpoint { get; set; }

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
            this.AwsAccessKey = this.Settings["awsAccessKey"];
            this.AwsSecretKey = this.Settings["awsSecretKey"];
            this.AwsBucketName = this.Settings["awsBucketName"];
            this.AwsRegionEndpoint = GetRegionEndpoint();

            if (AwsIsValid)
            {
                // Create S3 client from AWS Access Key and AWS Secret Access Key.
                this.s3ClientCache = new AmazonS3Client(this.AwsAccessKey, this.AwsSecretKey, this.AwsRegionEndpoint);
            }
            else
            {
                //throw new AmazonS3Exception();
            }

            this.cachedCdnRoot = this.Settings.ContainsKey("CachedCDNRoot")
                                     ? this.Settings["CachedCDNRoot"]
                                     : string.Empty;
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
            this.CachedPath =
                Path.Combine(this.cachedCdnRoot, this.ImageProcessorCachePrefix, pathFromKey, cachedFileName)
                    .Replace(@"\", "/");

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
            if (AwsIsValid)
            {
                var fullPath = GetFullPath(this.CachedPath);

                // Check if the local file exsit
                if (!File.Exists(fullPath))
                {
                    return;
                }

                try
                {
                    var transferUtility = new TransferUtility(this.s3ClientCache);

                    var path = GetFolderStructureForAmazon(this.CachedPath);
                    var filename = GetFileNameFromPath(this.CachedPath);
                    var key = GetKey(path, filename);

                    var transferUtilityUploadRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = this.AwsBucketName,
                        InputStream = stream,
                        Key = key,
                        CannedACL = S3CannedACL.PublicRead
                    };

                    await transferUtility.UploadAsync(transferUtilityUploadRequest);
                }
                catch (AmazonS3Exception amazonS3Exception)
                {
                    //todo: handle exceptions?
                    throw amazonS3Exception;
                }
            }
        }

        /// <summary>
        /// Trims the cache of any expired items in an asynchronous manner.
        /// </summary>
        /// <returns>
        /// The asynchronous <see cref="Task"/> representing an asynchronous operation.
        /// </returns>
        public override async Task TrimCacheAsync()
        {
            ListObjectsRequest request = new ListObjectsRequest
            {
                BucketName = this.AwsBucketName,
                Prefix = this.ImageProcessorCachePrefix,
                Delimiter = @"/"
            };

            List<S3Object> results = new List<S3Object>();

            ListObjectsResponse response = await this.s3ClientCache.ListObjectsAsync(request);
            results.AddRange(response.S3Objects);

            foreach (var file in response.S3Objects
                                         .OrderBy(x => x.LastModified != null ? x.LastModified.ToUniversalTime() : new DateTime()))
            {
                if (file.LastModified != null && !this.IsExpired(file.LastModified.ToUniversalTime()))
                {
                    break;
                }

                CacheIndexer.Remove(file.Key);
                await this.s3ClientCache.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = this.AwsBucketName,
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
                                                  !string.IsNullOrWhiteSpace(parsedExtension)
                                                      ? parsedExtension.Replace(".", string.Empty)
                                                      : "jpg");

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
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.CachedPath);
            request.Method = "HEAD";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                HttpStatusCode responseCode = response.StatusCode;
                context.Response.Redirect(responseCode == HttpStatusCode.NotFound ? this.CachedPath : this.RequestPath, false);
            }
        }


        /// <summary>
        /// Helper to get full path using CachedCdnRoot
        /// </summary>
        /// <param name="path">Web path to file's folder</param>
        /// <param name="fileName">File name</param>
        /// <returns>Key value</returns>
        private string GetFullPath(string path)
        {
            return !path.StartsWith(this.cachedCdnRoot)
                       ? Path.Combine(this.cachedCdnRoot, path)
                       : path;
        }


        /// <summary>
        /// Helper to get file name from path
        /// </summary>
        /// <param name="path">Web path to file's folder</param>
        /// <param name="fileName">File name</param>
        /// <returns>Key value</returns>
        private string GetFileNameFromPath(string path)
        {
            return Path.GetFileName(path);
        }


        /// <summary>
        /// Helper to get folder structure from amazon
        /// </summary>
        /// <param name="path">Web path to file's folder</param>
        /// <returns>Key value</returns>
        protected string GetFolderStructureForAmazon(string path)
        {
            var output = Path.Combine(this.cachedCdnRoot, path.Split('\\').Count() > 1 ? Path.GetDirectoryName(path) : path);

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
        /// <returns>Region Endpoint</returns>
        private RegionEndpoint GetRegionEndpoint()
        {
            var regionEndpointAsString = this.Settings["awsBucketName"];

            switch (regionEndpointAsString)
            {
                case "EUWest1":
                    return RegionEndpoint.EUWest1;
                    break;

                //add more conditions
                default:
                    return RegionEndpoint.EUWest1;
                    break;
            }
        }
    }
}
