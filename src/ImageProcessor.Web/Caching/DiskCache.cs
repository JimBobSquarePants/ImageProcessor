// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiskCache.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides an <see cref="IImageCache" /> implementation that is file system based.
//   The cache is self healing and cleaning.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Hosting;

    using ImageProcessor.Configuration;
    using ImageProcessor.Web.Extensions;

    /// <summary>
    /// Provides an <see cref="IImageCache"/> implementation that is file system based.
    /// The cache is self healing and cleaning.
    /// </summary>
    public class DiskCache : ImageCacheBase
    {
        /// <summary>
        /// The maximum number of files allowed in the directory.
        /// </summary>
        /// <remarks>
        /// NTFS directories can handle up to 10,000 files in the directory before slowing down.
        /// This will help us to ensure that don't go over that limit.
        /// <see href="http://stackoverflow.com/questions/197162/ntfs-performance-and-large-volumes-of-files-and-directories"/>
        /// <see href="http://stackoverflow.com/questions/115882/how-do-you-deal-with-lots-of-small-files"/>
        /// <see href="http://stackoverflow.com/questions/1638219/millions-of-small-graphics-files-and-how-to-overcome-slow-file-system-access-on"/>
        /// </remarks>
        private const int MaxFilesCount = 100;

        /// <summary>
        /// Used to lock against when checking the cached folder path.
        /// </summary>
        private static object cachePathValidatorLock = new object();

        /// <summary>
        /// Whether the cached path has been checked.
        /// </summary>
        private static bool cachePathValidatorCheck;

        /// <summary>
        /// Stores the resulting validated absolute cache folder path
        /// </summary>
        private static string validatedAbsoluteCachePath;

        /// <summary>
        /// Stores the resulting validated virtual cache folder path - if it's within the web root
        /// </summary>
        private static string validatedVirtualCachePath;

        /// <summary>
        /// The virtual cache path.
        /// </summary>
        private readonly string virtualCachePath;

        /// <summary>
        /// The absolute path to virtual cache path on the server.
        /// </summary>
        private readonly string absoluteCachePath;

        /// <summary>
        /// The virtual path to the cached file.
        /// </summary>
        private string virtualCachedFilePath;

        /// <summary>
        /// The create time of the cached image
        /// </summary>
        private DateTime cachedImageCreationTimeUtc = DateTime.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiskCache"/> class.
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
        public DiskCache(string requestPath, string fullPath, string querystring)
            : base(requestPath, fullPath, querystring)
        {
            string configuredPath = this.Settings["VirtualCachePath"];

            this.absoluteCachePath = GetValidatedAbsolutePath(configuredPath, out string virtualPath);
            this.virtualCachePath = virtualPath;
        }

        /// <summary>
        /// Gets a value indicating whether the image is new or updated in an asynchronous manner.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public override async Task<bool> IsNewOrUpdatedAsync()
        {
            // TODO: Before this check is performed it should be throttled. For example, only perform this check
            // if the last time it was checked is greater than 5 seconds. This would be much better for perf
            // if there is a high throughput of image requests.
            string cachedFileName = await this.CreateCachedFileNameAsync().ConfigureAwait(false);
            this.CachedPath = CachedImageHelper.GetCachedPath(this.absoluteCachePath, cachedFileName, false, this.FolderDepth);
            this.virtualCachedFilePath = CachedImageHelper.GetCachedPath(this.virtualCachePath, cachedFileName, true, this.FolderDepth);

            bool isUpdated = false;
            CachedImage cachedImage = CacheIndexer.Get(this.CachedPath);

            if (cachedImage == null)
            {
                var info = new FileInfo(this.CachedPath);
                if (info.Exists)
                {
                    cachedImage = new CachedImage
                    {
                        Key = Path.GetFileNameWithoutExtension(this.CachedPath),
                        Path = this.CachedPath,
                        CreationTimeUtc = info.LastWriteTimeUtc
                    };

                    CacheIndexer.Add(cachedImage, this.ImageCacheMaxMinutes);
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
                else
                {
                    // Set cachedImageCreationTimeUtc so we can sender Last-Modified or ETag header when using Response.TransmitFile()
                    this.cachedImageCreationTimeUtc = cachedImage.CreationTimeUtc;
                }
            }

            return isUpdated;
        }

        /// <summary>
        /// Adds the image to the cache in an asynchronous manner.
        /// </summary>
        /// <param name="stream">The stream containing the image data.</param>
        /// <param name="contentType">The content type of the image.</param>
        /// <returns>
        /// The <see cref="Task"/> representing an asynchronous operation.
        /// </returns>
        public override async Task AddImageToCacheAsync(Stream stream, string contentType)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(this.CachedPath));
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            // This is a hack. I don't know why there would ever be double access to the file but there is
            // and it's causing errors to be thrown.
            // Ensure that an attempt at overwrite is still made, so cached images can be updated
            // https://github.com/JimBobSquarePants/ImageProcessor/issues/629
            try
            {
                using (FileStream fileStream = File.Create(this.CachedPath, (int)stream.Length))
                {
                    await stream.CopyToAsync(fileStream).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                ImageProcessorBootstrapper.Instance.Logger.Log<DiskCache>($"Unable to write to file: {this.CachedPath}, {ex.Message}");
            }
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

            this.ScheduleCacheTrimmer(token =>
            {
                var rootDirectory = Path.GetDirectoryName(this.CachedPath);
                if (rootDirectory != null)
                {
                    // Jump up to the parent branch to clean through the cache
                    // UNC folders can throw exceptions if the file doesn't exist
                    var directories = SafeEnumerateDirectories(validatedAbsoluteCachePath).Reverse().ToList();
                    foreach (string directory in directories)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        try
                        {
                            var files = Directory.EnumerateFiles(directory).Select(f => new FileInfo(f)).OrderBy(f => f.CreationTimeUtc).ToList();
                            var count = files.Count;
                            foreach (var fileInfo in files)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    break;
                                }

                                try
                                {
                                    // If the group count is equal to the max count minus 1 then we know we have reduced the number of items below the maximum allowed
                                    // We'll cleanup any orphaned expired files though
                                    if (!this.IsExpired(fileInfo.CreationTimeUtc) && count <= MaxFilesCount - 1)
                                    {
                                        break;
                                    }

                                    // Remove from the cache and delete each CachedImage
                                    CacheIndexer.Remove(fileInfo.Name);
                                    fileInfo.Delete();
                                    count--;
                                }
                                catch (Exception ex)
                                {
                                    // Log it but skip to the next file
                                    ImageProcessorBootstrapper.Instance.Logger.Log<DiskCache>($"Unable to clean cached file: {fileInfo.FullName}, {ex.Message}");
                                }
                            }

                            // If the directory is empty, delete it to remove the FCN
                            if (count == 0
                                && Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly).Length == 0)
                            {
                                Directory.Delete(directory);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log it but skip to the next directory
                            ImageProcessorBootstrapper.Instance.Logger.Log<DiskCache>($"Unable to clean cached directory: {directory}, {ex.Message}");
                        }
                    }
                }

                return Task.FromResult(0);
            });

            return Task.FromResult(0);
        }

        /// <summary>
        /// Rewrites the path to point to the cached image.
        /// </summary>
        /// <param name="context">
        /// The <see cref="HttpContext"/> encapsulating all information about the request.
        /// </param>
        public override void RewritePath(HttpContext context)
        {
            if (!string.IsNullOrWhiteSpace(validatedVirtualCachePath))
            {
                // The cached file is valid so just rewrite the path.
                context.RewritePath(this.virtualCachedFilePath, false);
            }
            else
            {
                // Check if the ETag matches (doing this here because context.RewritePath seems to handle it automatically
                string eTagFromHeader = context.Request.Headers["If-None-Match"];
                string eTag = this.GetETag();
                if (!string.IsNullOrEmpty(eTagFromHeader) && !string.IsNullOrEmpty(eTag) && eTagFromHeader == eTag)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                    context.Response.StatusDescription = "Not Modified";
                    HttpModules.ImageProcessingModule.SetHeaders(context, this.BrowserMaxDays);
                    context.Response.End();
                }
                else
                {
                    // The file is outside of the web root so we cannot just rewrite the path since that won't work.
                    // We basically have a few options:

                    // 1. Create a custom VirtualPathProvider - this would work but we don't get all of the goodness that comes with
                    // ASP.NET StaticFileHandler such as sending the correct cache headers, etc... you can see what I mean by
                    // looking at the source: https://referencesource.microsoft.com/#System.Web/StaticFileHandler.cs,492
                    // 2. HttpResponse.TransmitFile - this actually uses the IIS/Windows kernel to do the transfer so it is very fast,
                    // I've tested the header results and they are identical to the response headers set when using the StaticFileHandler
                    // 3. Set cache headers, etc... manually with regards to how the StaticFileHandler does it:
                    // https://referencesource.microsoft.com/#System.Web/StaticFileHandler.cs,505

                    // 4. Use reflection to invoke the StaticFileHandler somehow
                    // 5. Use a custom StaticFileHandler like https://code.google.com/archive/p/talifun-web/wikis/StaticFileHandler.wiki

                    // I've opted to go with the simplest solution and use TransmitFile, I've looked into the source of this and it uses the
                    // Windows Kernel, I'm not sure where in the source the headers get written but if you analyze the request/response headers when
                    // this is used, they are exactly the same as if the StaticFileHandler (i.e. RewritePath) is used, so this seems perfect and easy!

                    // We need to manually write out the Content Type header here, the TransmitFile does not do this for you
                    // whereas the RewritePath actually does.
                    // https://github.com/JimBobSquarePants/ImageProcessor/issues/529
                    string extension = Helpers.ImageHelpers.Instance.GetExtension(this.FullPath, this.Querystring);
                    string mimeType = Helpers.ImageHelpers.Instance.GetContentTypeForExtension(extension);
                    context.Response.ContentType = mimeType;

                    // Since we are going to call Response.End(), we need to go ahead and set the headers
                    HttpModules.ImageProcessingModule.SetHeaders(context, this.BrowserMaxDays);
                    this.SetETagHeader(context);
                    long length = new FileInfo(this.CachedPath).Length;
                    context.Response.AddHeader("Content-Length", length.ToString());

                    context.Response.TransmitFile(this.CachedPath, 0, length);

                    // This is quite important expecially if the request is a when an `IImageService` handles the request
                    // based on a custom request handler such as "database.axd/logo.png". If we don't end the request here
                    // and because we are not rewriting any paths, the request will continue to try to execute this handler
                    // and errors will occur. It should be fine that we are ending the request pipeline since
                    // all we really want to do here is send the file above. There's some arguments about using
                    // `ApplicationInstance.CompleteRequest();` instead of Response.End() but in this case I believe it is
                    // correct to use Response.End(), a good write-up of why this is can be found here:
                    // see: http://stackoverflow.com/a/36968241/694494
                    try
                    {
                        context.Response.End();
                    }
                    catch (ThreadAbortException)
                    {
                        Thread.ResetAbort();
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
                    var info = new FileInfo(this.RequestPath);
                    if (info.Exists)
                    {
                        // If it's newer than the cached file then it must be an update.
                        isUpdated = info.LastWriteTimeUtc > creationDate;
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
        /// The internal method that performs the actual validation which can be unit tested
        /// </summary>
        /// <param name="originalPath">
        /// The original path to validate which could be an absolute or a virtual path
        /// </param>
        /// <param name="mapPath">
        /// A function to use to perform the MapPath
        /// </param>
        /// <param name="getDirectoryInfo">
        /// A function to use to create the DirectoryInfo instance
        /// (this allows us to unit test)
        /// </param>
        /// <param name="virtualCachePath">
        /// If the absolute cache path is within the web root then the result of this will be the virtual path
        /// of the cache folder. If the absolute path is not within the web root then this will be null.
        /// </param>
        /// <returns>
        /// The absolute path to the cache folder
        /// </returns>
        internal static string GetValidatedCachePathsImpl(string originalPath, Func<string, string> mapPath, Func<string, FileSystemInfo> getDirectoryInfo, out string virtualCachePath)
        {
            string webRoot = mapPath("~/");

            string absPath = string.Empty;

            if (originalPath.IsValidVirtualPathName())
            {
                // GetFullPath will resolve any relative paths like ".." in the path
                absPath = Path.GetFullPath(mapPath(originalPath));
            }
            else if (Path.IsPathRooted(originalPath) && originalPath.IndexOfAny(Path.GetInvalidPathChars()) == -1)
            {
                // Determine if this is an absolute path
                // in this case this should be a real path, it's the best check we can do without a try/catch, but if this
                // does throw, we'll let it throw anyways.
                absPath = originalPath;
            }

            if (string.IsNullOrEmpty(absPath))
            {
                // Didn't pass the simple validation checks
                string message = "'VirtualCachePath' is not a valid virtual path. " + originalPath;
                ImageProcessorBootstrapper.Instance.Logger.Log<DiskCache>(message);
                throw new ConfigurationErrorsException("DiskCache: " + message);
            }

            // Create a DirectoryInfo object to truly validate which will throw if it's not correct
            FileSystemInfo dirInfo = getDirectoryInfo(absPath);
            bool isInWebRoot = dirInfo.FullName.TrimEnd('/').StartsWith(webRoot.TrimEnd('/'));

            if (!dirInfo.Exists)
            {
                Directory.CreateDirectory(dirInfo.FullName);
            }

            // This does a reverse map path:
            virtualCachePath = isInWebRoot
                ? dirInfo.FullName.Replace(webRoot, "~/").Replace(@"\", "/")
                : null;

            return dirInfo.FullName;
        }

        /// <summary>
        /// This will get the validated absolute path which is based on the configured value one time
        /// </summary>
        /// <param name="originalPath">The original path</param>
        /// <param name="virtualPath">The resulting virtual path if the path is within the web-root</param>
        /// <returns>The <see cref="string"/></returns>
        /// <remarks>
        /// We are performing this statically in order to avoid any overhead used when performing the validation since
        /// this occurs for each image when it only needs to be done once
        /// </remarks>
        private static string GetValidatedAbsolutePath(string originalPath, out string virtualPath)
        {
            string absoluteCachePath = LazyInitializer.EnsureInitialized(
                ref validatedAbsoluteCachePath,
                ref cachePathValidatorCheck,
                ref cachePathValidatorLock,
                () =>
                {
                    Func<string, string> mapPath = HostingEnvironment.MapPath;
                    if (originalPath.Contains("/.."))
                    {
                        // If that is the case this means that the user may be traversing beyond the wwwroot
                        // so we'll need to cater for that. HostingEnvironment.MapPath will throw a HttpException
                        // if the request goes beyond the webroot so we'll need to use our own MapPath method.
                        mapPath = s =>
                        {
                            try
                            {
                                return HostingEnvironment.MapPath(s);
                            }
                            catch (HttpException)
                            {
                                // need to user our own logic
                                return s.Replace("~/", HttpRuntime.AppDomainAppPath).Replace("/", "\\");
                            }
                        };
                    }

                    string result = GetValidatedCachePathsImpl(
                        originalPath,
                        mapPath,
                        s => new DirectoryInfo(s),
                        out string virtualCacheFolderPath);

                    validatedVirtualCachePath = virtualCacheFolderPath;
                    return result;
                });

            if (!string.IsNullOrWhiteSpace(validatedVirtualCachePath))
            {
                // Set the virtual cache path to the original one specified, it's just a normal virtual path like ~/App_Data/Blah
                virtualPath = validatedVirtualCachePath;
            }
            else
            {
                // It's outside of the web root, therefore it is an absolute path, we'll need to just have the virtualPath set
                // to the absolute path but deal with it accordingly based on the isCachePathInWebRoot flag
                virtualPath = absoluteCachePath;
            }

            return absoluteCachePath;
        }

        /// <summary>
        /// Returns an enumerable collection of directory paths that matches a specified search pattern and search subdirectory option.
        /// Will return an empty enumerable on exception. Quick and dirty but does what I need just now.
        /// </summary>
        /// <param name="directoryPath">
        /// The path to the directory to search within.
        /// </param>
        /// <param name="searchPattern">
        /// The search string to match against the names of directories. This parameter can contain a combination of valid literal path 
        /// and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions. The default pattern is "*", which returns all files.
        /// </param>
        /// <param name="searchOption">
        /// One of the enumeration values that specifies whether the search operation should include only 
        /// the current directory or all subdirectories. The default value is AllDirectories.
        /// </param>
        /// <returns>
        /// An enumerable collection of directories that matches searchPattern and searchOption.
        /// </returns>
        private static IEnumerable<string> SafeEnumerateDirectories(string directoryPath, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            IEnumerable<string> directories;

            try
            {
                directories = Directory.EnumerateDirectories(directoryPath, searchPattern, searchOption);
            }
            catch
            {
                return Enumerable.Empty<string>();
            }

            return directories;
        }

        /// <summary>
        /// Sets the ETag Header
        /// </summary>
        /// <param name="context"></param>
        private void SetETagHeader(HttpContext context)
        {
            string eTag = this.GetETag();
            if (!string.IsNullOrEmpty(eTag))
            {
                context.Response.Cache.SetETag(eTag);
            }
        }

        /// <summary>
        /// Creates an ETag value from the current creation time.
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        private string GetETag()
        {
            if (this.cachedImageCreationTimeUtc != DateTime.MinValue)
            {
                long lastModFileTime = this.cachedImageCreationTimeUtc.ToFileTime();
                long nowFileTime = DateTime.UtcNow.ToFileTime();
                string hexFileTime = lastModFileTime.ToString("X8", System.Globalization.CultureInfo.InvariantCulture);
                if ((nowFileTime - lastModFileTime) <= 30000000)
                {
                    return "W/\"" + hexFileTime + "\"";
                }

                return "\"" + hexFileTime + "\"";
            }

            return null;
        }
    }
}
