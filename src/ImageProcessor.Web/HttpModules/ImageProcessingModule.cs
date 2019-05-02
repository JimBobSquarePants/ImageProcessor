// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageProcessingModule.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Processes any image requests within the web application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.HttpModules
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Hosting;
    using ImageProcessor.Common.Exceptions;
    using ImageProcessor.Configuration;
    using ImageProcessor.Imaging;
    using ImageProcessor.Imaging.Formats;
    using ImageProcessor.Web.Caching;
    using ImageProcessor.Web.Configuration;
    using ImageProcessor.Web.Extensions;
    using ImageProcessor.Web.Helpers;
    using ImageProcessor.Web.Processors;
    using ImageProcessor.Web.Services;

    /// <summary>
    /// Processes any image requests within the web application.
    /// </summary>
    public class ImageProcessingModule : IHttpModule
    {
        /// <summary>
        /// The key for storing the response type of the current image.
        /// </summary>
        private const string CachedResponseTypeKey = "CACHED_IMAGE_RESPONSE_TYPE_054F217C-11CF-49FF-8D2F-698E8E6EB58F";

        /// <summary>
        /// The key for storing the file dependency of the current image.
        /// </summary>
        private const string CachedResponseFileDependency = "CACHED_IMAGE_DEPENDENCY_054F217C-11CF-49FF-8D2F-698E8E6EB58F";

        /// <summary>
        /// The regular expression to search strings for presets with.
        /// </summary>
        private static readonly Regex PresetRegex = new Regex("preset=[^&]+", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

        /// <summary>
        /// The regular expression to search strings for protocols with.
        /// </summary>
        private static readonly Regex ProtocolRegex = new Regex("http(s)?://", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

        /// <summary>
        /// The base assembly version.
        /// </summary>
        private static readonly string AssemblyVersion = typeof(ImageFactory).Assembly.GetName().Version.ToString();

        /// <summary>
        /// The web assembly version.
        /// </summary>
        private static readonly string WebAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// Ensures duplicate requests are atomic.
        /// </summary>
        private static readonly AsyncKeyLock Locker = new AsyncKeyLock();

        /// <summary>
        /// Whether to allow known cache busters.
        /// </summary>
        private static bool? allowCacheBuster;

        /// <summary>
        /// Whether to preserve exif meta data.
        /// </summary>
        private static bool? preserveExifMetaData;

        /// <summary>
        /// The meta data mode to use.
        /// </summary>
        private static MetaDataMode? metaDataMode;

        /// <summary>
        /// Whether to perform gamma correction when performing processing.
        /// </summary>
        private static bool? fixGamma;

        /// <summary>
        /// Whether to to intercept all image requests including ones
        /// without querystring parameters.
        /// </summary>
        private static bool? interceptAllRequests;

        /// <summary>
        /// A value indicating whether this instance of the given entity has been disposed.
        /// </summary>
        /// <value><see langword="true"/> if this instance has been disposed; otherwise, <see langword="false"/>.</value>
        /// <remarks>
        /// If the entity is disposed, it must not be disposed a second
        /// time. The isDisposed field is set the first time the entity
        /// is disposed. If the isDisposed field is true, then the Dispose()
        /// method will not dispose again. This help not to prolong the entity's
        /// life in the Garbage Collector.
        /// </remarks>
        private bool isDisposed;

        /// <summary>
        /// The image cache.
        /// </summary>
        private IImageCache imageCache;

        /// <summary>
        /// Finalizes an instance of the <see cref="T:ImageProcessor.Web.HttpModules.ImageProcessingModule"/> class.
        /// </summary>
        /// <remarks>
        /// Use C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method
        /// does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide destructors in types derived from this class.
        /// </remarks>
        ~ImageProcessingModule()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            this.Dispose(false);
        }

        /// <summary>
        /// Event to use to validate the request or manipulate the request parameters
        /// </summary>
        /// <remarks>
        /// This can be used by developers to cancel the request based on the parameters specified or used to manipulate the parameters
        /// </remarks>
        public static event EventHandler<ValidatingRequestEventArgs> ValidatingRequest;

        /// <summary>
        /// The event that is called when a new image is processed.
        /// </summary>
        public static event EventHandler<PostProcessingEventArgs> OnPostProcessing;

        /// <summary>
        /// This will make the browser and server keep the output
        /// in its cache and thereby improve performance.
        /// </summary>
        /// <param name="context">
        /// the <see cref="T:System.Web.HttpContext">HttpContext</see> object that provides
        /// references to the intrinsic server objects
        /// </param>
        /// <param name="responseType">The HTTP MIME type to send.</param>
        /// <param name="dependencyPaths">The dependency path for the cache dependency.</param>
        /// <param name="maxDays">The maximum number of days to store the image in the browser cache.</param>
        /// <param name="statusCode">An optional status code to send to the response.</param>
        public static void SetHeaders(HttpContext context, string responseType, string[] dependencyPaths, int maxDays, HttpStatusCode? statusCode = null)
        {
            HttpResponse response = context.Response;

            if (response.Headers["ImageProcessedBy"] == null)
            {
                response.AddHeader("ImageProcessedBy", $"ImageProcessor/{AssemblyVersion} - ImageProcessor.Web/{WebAssemblyVersion}");
            }

            HttpCachePolicy cache = response.Cache;
            cache.SetCacheability(HttpCacheability.Public);
            cache.VaryByHeaders["Accept-Encoding"] = true;

            if (!string.IsNullOrWhiteSpace(responseType))
            {
                response.ContentType = responseType;
            }

            if (dependencyPaths != null)
            {
                context.Response.AddFileDependencies(dependencyPaths.ToArray());
                cache.SetLastModifiedFromFileDependencies();
            }

            if (statusCode != null)
            {
                response.StatusCode = (int)statusCode;
            }

            cache.SetExpires(DateTime.Now.ToUniversalTime().AddDays(maxDays));
            cache.SetMaxAge(new TimeSpan(maxDays, 0, 0, 0));

            if (ParseCacheBuster(context.Request.QueryString))
            {
                cache.AppendCacheExtension("immutable");
            }
            else
            {
                cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            }

            AddCorsRequestHeaders(context);
        }

        /// <summary>
        /// This will make the browser and server keep the output in its cache and thereby improve performance.
        /// </summary>
        /// <param name="context">
        /// the <see cref="T:System.Web.HttpContext">HttpContext</see> object that provides
        /// references to the intrinsic server objects
        /// </param>
        /// <param name="maxDays">The maximum number of days to store the image in the browser cache.</param>
        public static void SetHeaders(HttpContext context, int maxDays)
        {
            object responseTypeObject = context.Items[CachedResponseTypeKey];
            object dependencyFileObject = context.Items[CachedResponseFileDependency];

            string responseType = responseTypeObject as string;
            string[] dependencyFiles = dependencyFileObject as string[];

            SetHeaders(context, responseType, dependencyFiles, maxDays);
        }

        /// <summary>
        /// Adds response headers allowing Cross Origin Requests if the current origin request
        /// passes sanitizing rules.
        /// </summary>
        /// <param name="context">
        /// the <see cref="T:System.Web.HttpContext">HttpContext</see> object that provides
        /// references to the intrinsic server objects
        /// </param>
        public static void AddCorsRequestHeaders(HttpContext context)
        {
            string origin = context.Request.Headers["Origin"];
            if (string.IsNullOrEmpty(origin))
            {
                return;
            }

            // Now we check to see if the the url is from a whitelisted location.
            if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri url))
            {
                return;
            }

            ImageSecuritySection.CORSOriginElement origins = ImageProcessorConfiguration.Instance.GetImageSecuritySection().CORSOrigin;

            if (origins?.WhiteList == null)
            {
                return;
            }

            // Check for root or sub domain.
            bool validUrl = false;
            foreach (ImageSecuritySection.SafeUrl safeUrl in origins.WhiteList)
            {
                Uri uri = safeUrl.Url;

                if (uri.ToString() == "*")
                {
                    validUrl = true;
                    break;
                }

                if (!uri.IsAbsoluteUri)
                {
                    var rebaseUri = new Uri($"http://{uri.ToString().TrimStart('.', '/')}");
                    validUrl = url.Host.StartsWith(rebaseUri.Host, StringComparison.OrdinalIgnoreCase) || url.Host.EndsWith(rebaseUri.Host, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    validUrl = url.Host.StartsWith(uri.Host, StringComparison.OrdinalIgnoreCase) || url.Host.EndsWith(uri.Host, StringComparison.OrdinalIgnoreCase);
                }

                if (validUrl)
                {
                    break;
                }
            }

            if (validUrl)
            {
                context.Response.AddHeader("Access-Control-Allow-Origin", origin);
            }
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">
        /// An <see cref="T:System.Web.HttpApplication"/> that provides
        /// access to the methods, properties, and events common to all
        /// application objects within an ASP.NET application
        /// </param>
        public void Init(HttpApplication context)
        {
            if (preserveExifMetaData == null)
            {
                preserveExifMetaData = ImageProcessorConfiguration.Instance.PreserveExifMetaData;
            }

            if (metaDataMode == null)
            {
                metaDataMode = ImageProcessorConfiguration.Instance.MetaDataMode;
            }

            if (allowCacheBuster == null)
            {
                allowCacheBuster = ImageProcessorConfiguration.Instance.AllowCacheBuster;
            }

            if (fixGamma == null)
            {
                fixGamma = ImageProcessorConfiguration.Instance.FixGamma;
            }

            if (interceptAllRequests == null)
            {
                interceptAllRequests = ImageProcessorConfiguration.Instance.InterceptAllRequests;
            }

            var postAuthorizeHelper = new EventHandlerTaskAsyncHelper(this.PostAuthorizeRequest);
            context.AddOnPostAuthorizeRequestAsync(postAuthorizeHelper.BeginEventHandler, postAuthorizeHelper.EndEventHandler);

            context.PostReleaseRequestState += this.PostReleaseRequestState;
            context.EndRequest += this.OnEndRequest;
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SuppressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Occurs when the user for the current request has been authorized.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// An <see cref="T:System.EventArgs">EventArgs</see> that contains the event data.
        /// </param>
        /// <returns>
        /// The <see cref="T:System.Threading.Tasks.Task"/>.
        /// </returns>
        protected virtual Task PostAuthorizeRequest(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;
            return this.ProcessImageAsync(context);
        }

        /// <summary>
        /// Gets url for the current request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected virtual string GetRequestUrl(HttpRequest request) => request.Unvalidated.RawUrl;

        /// <summary>
        /// Disposes the object and frees resources for the Garbage Collector.
        /// </summary>
        /// <param name="disposing">
        /// If true, the object gets disposed.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose of any managed resources here.
            }

            // Call the appropriate methods to clean up
            // unmanaged resources here.
            // Note disposing is done.
            this.isDisposed = true;
        }

        /// <summary>
        /// Occurs when the ASP.NET event handler finishes execution.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// An <see cref="T:System.EventArgs">EventArgs</see> that contains the event data.
        /// </param>
        private void OnEndRequest(object sender, EventArgs e)
        {
            // Reset the cache.
            this.imageCache = null;
        }

        /// <summary>
        /// Occurs when ASP.NET has completed executing all request event handlers and the request
        /// state data has been stored.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// An <see cref="T:System.EventArgs">EventArgs</see> that contains the event data.
        /// </param>
        private void PostReleaseRequestState(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;

            // Set the headers
            if (this.imageCache != null)
            {
                SetHeaders(context, this.imageCache.BrowserMaxDays);
            }
        }

        /// <summary>
        /// Processes the image.
        /// </summary>
        /// <param name="context">
        /// the <see cref="T:System.Web.HttpContext">HttpContext</see> object that provides
        /// references to the intrinsic server objects
        /// </param>
        /// <returns>
        /// The <see cref="T:System.Threading.Tasks.Task"/>.
        /// </returns>
        private async Task ProcessImageAsync(HttpContext context)
        {
            HttpRequest request = context.Request;
            string rawUrl = this.GetRequestUrl(request);

            // Should we ignore this request?
            if (string.IsNullOrWhiteSpace(rawUrl) || rawUrl.IndexOf("ipignore=true", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return;
            }

            // Sometimes the request is url encoded
            // See https://github.com/JimBobSquarePants/ImageProcessor/issues/478
            // This causes a bit of a nightmare as the incoming request is corrupted and cannot be used for splitting
            // out each url part. This becomes a manual job.
            string url = rawUrl;
            string applicationPath = request.ApplicationPath;

            IImageService currentService = this.GetImageServiceForRequest(url, applicationPath);

            if (currentService == null)
            {
                return;
            }

            // Parse url
            UrlParser.ParseUrl(url, currentService.Prefix, out string requestPath, out string queryString);
            string originalQueryString = queryString;

            // Replace any presets in the querystring with the actual value.
            queryString = this.ReplacePresetsInQueryString(queryString);

            var httpContextBase = new HttpContextWrapper(context);

            // Execute the handler which can change the querystring
            var validatingArgs = new ValidatingRequestEventArgs(httpContextBase, queryString);
            this.OnValidatingRequest(validatingArgs);

            // If the validation has failed based on events, return
            if (validatingArgs.Cancel)
            {
                ImageProcessorBootstrapper.Instance.Logger.Log<ImageProcessingModule>($"Image processing for {url} has been cancelled by an event");
                return;
            }

            // Re-assign based on event handlers
            queryString = validatingArgs.QueryString;
            if (string.IsNullOrWhiteSpace(originalQueryString) && !string.IsNullOrWhiteSpace(queryString))
            {
                // Add new query string to URL
                url = url + "?" + queryString;
            }
            else if (!string.IsNullOrWhiteSpace(queryString) && !string.Equals(originalQueryString, queryString))
            {
                url = Regex.Replace(url, Regex.Escape(originalQueryString), queryString, RegexOptions.IgnoreCase);
            }

            // Map the request path if file local.
            bool isFileLocal = currentService.IsFileLocalService;
            if (currentService.IsFileLocalService)
            {
                requestPath = HostingEnvironment.MapPath(requestPath);
            }

            if (string.IsNullOrWhiteSpace(requestPath))
            {
                return;
            }

            // Parse any protocol values from settings if no protocol is present.
            if (currentService.Settings.TryGetValue("Protocol", out var protocol) && (ProtocolRegex.Matches(requestPath) is var matches && (matches.Count == 0 || matches[0].Index > 0)))
            {
                // ReSharper disable once PossibleNullReferenceException
                requestPath = protocol + "://" + requestPath.TrimStart('/');
            }

            // Break out if we don't meet critera.
            // First check that the request path is valid and whether we are intercepting all requests or the querystring is valid.
            bool interceptAll = interceptAllRequests != null && interceptAllRequests.Value;
            if (string.IsNullOrWhiteSpace(requestPath) || (!interceptAll && string.IsNullOrWhiteSpace(queryString)))
            {
                return;
            }

            // Check whether the path is valid for other requests.
            // We've already checked the unprefixed requests in GetImageServiceForRequest().
            if (!string.IsNullOrWhiteSpace(currentService.Prefix) && !currentService.IsValidRequest(requestPath))
            {
                return;
            }

            bool isNewOrUpdated = false;
            string cachedPath = string.Empty;
            bool processing = false;
            IWebGraphicsProcessor[] processors = null;
            AnimationProcessMode mode = AnimationProcessMode.First;

            using (await Locker.ReaderLockAsync(rawUrl).ConfigureAwait(false))
            {
                // Parse the url to see whether we should be doing any work. 
                // If we're not intercepting all requests and we don't have valid instructions we shoul break here.
                if (!string.IsNullOrWhiteSpace(queryString))
                {
                    // Attempt to match querystring and processors.
                    processors = ImageFactoryExtensions.GetMatchingProcessors(queryString);

                    // Animation is not a processor but can be a specific request so we should allow it.
                    mode = this.ParseAnimationMode(queryString, out bool processAnimation);

                    // Are we processing or cache busting?
                    processing = processors != null && (processors.Length > 0 || processAnimation);
                    bool cacheBusting = ParseCacheBuster(queryString);
                    if (!processing && !cacheBusting)
                    {
                        // No? Someone is either attacking the server or hasn't read the instructions.
                        string message = $"The request {request.Unvalidated.RawUrl} could not be understood by the server due to malformed syntax.";
                        ImageProcessorBootstrapper.Instance.Logger.Log<ImageProcessingModule>(message);
                        return;
                    }
                }

                // Create a new cache to help process and cache the request.
                this.imageCache = (IImageCache)ImageProcessorConfiguration.Instance
                    .ImageCache.GetInstance(requestPath, url, queryString);

                // Is the file new or updated?
                isNewOrUpdated = await this.imageCache.IsNewOrUpdatedAsync().ConfigureAwait(false);
                cachedPath = this.imageCache.CachedPath;

                if (!isNewOrUpdated)
                {
                    // The cached file is valid so just rewrite the path.
                    this.imageCache.RewritePath(context);

                    // Redirect if not a locally store file.
                    if (!new Uri(cachedPath).IsFile)
                    {
                        context.ApplicationInstance.CompleteRequest();
                    }

                    return;
                }
            }

            // Only process if the file has been updated.
            using (await Locker.WriterLockAsync(rawUrl).ConfigureAwait(false))
            {
                // Ok let's get the image
                byte[] imageBuffer = null;
                string mimeType;

                try
                {
                    if (currentService is IImageService2 imageService2)
                    {
                        imageBuffer = await imageService2.GetImage(requestPath, context).ConfigureAwait(false);
                    }
                    else
                    {
                        imageBuffer = await currentService.GetImage(requestPath).ConfigureAwait(false);
                    }
                }
                catch (HttpException ex)
                {
                    // We want 404's to be handled by IIS so that other handlers/modules can still run.
                    if (ex.GetHttpCode() == (int)HttpStatusCode.NotFound)
                    {
                        ImageProcessorBootstrapper.Instance.Logger.Log<ImageProcessingModule>(ex.Message);
                        return;
                    }
                }

                if (imageBuffer == null)
                {
                    return;
                }

                // Using recyclable streams here should dramatically reduce the overhead required
                using (MemoryStream inStream = MemoryStreamPool.Shared.GetStream("inStream", imageBuffer, 0, imageBuffer.Length))
                {
                    // Process the Image. Use a recyclable stream here to reduce the allocations
                    MemoryStream outStream = MemoryStreamPool.Shared.GetStream();

                    if (!string.IsNullOrWhiteSpace(queryString))
                    {
                        if (processing)
                        {
                            // Process the image.
                            bool exif = preserveExifMetaData != null && preserveExifMetaData.Value;
                            MetaDataMode metaMode = !exif ? MetaDataMode.None : metaDataMode.Value;
                            bool gamma = fixGamma != null && fixGamma.Value;

                            try
                            {
                                using (var imageFactory = new ImageFactory(metaMode, gamma) { AnimationProcessMode = mode })
                                {
                                    imageFactory.Load(inStream).AutoProcess(processors).Save(outStream);
                                    mimeType = imageFactory.CurrentImageFormat.MimeType;
                                }
                            }
                            catch (ImageFormatException)
                            {
                                ImageProcessorBootstrapper.Instance.Logger.Log<ImageProcessingModule>($"Request {url} is not a valid image.");
                                return;
                            }
                        }
                        else
                        {
                            // We're cache-busting. Allow the value to be cached
                            await inStream.CopyToAsync(outStream).ConfigureAwait(false);
                            mimeType = FormatUtilities.GetFormat(outStream).MimeType;
                        }
                    }
                    else
                    {
                        // We're capturing all requests.
                        await inStream.CopyToAsync(outStream).ConfigureAwait(false);
                        mimeType = FormatUtilities.GetFormat(outStream).MimeType;
                    }

                    // Fire the post processing event.
                    EventHandler<PostProcessingEventArgs> handler = OnPostProcessing;
                    if (handler != null)
                    {
                        string extension = Path.GetExtension(cachedPath);
                        var args = new PostProcessingEventArgs
                        {
                            Context = context,
                            ImageStream = outStream,
                            ImageExtension = extension
                        };

                        handler(this, args);
                        outStream = args.ImageStream;
                    }

                    // Add to the cache.
                    await this.imageCache.AddImageToCacheAsync(outStream, mimeType).ConfigureAwait(false);

                    // Cleanup
                    outStream.Dispose();
                }

                // Store the response type and cache dependency in the context for later retrieval.
                context.Items[CachedResponseTypeKey] = mimeType;
                bool isFileCached = new Uri(cachedPath).IsFile;

                if (isFileLocal)
                {
                    if (isFileCached)
                    {
                        // Some services might only provide filename so we can't monitor for the browser.
                        context.Items[CachedResponseFileDependency] = Path.GetFileName(requestPath) == requestPath
                            ? new[] { cachedPath }
                            : new[] { requestPath, cachedPath };
                    }
                    else
                    {
                        context.Items[CachedResponseFileDependency] = Path.GetFileName(requestPath) == requestPath
                            ? null
                            : new[] { requestPath };
                    }
                }
                else if (isFileCached)
                {
                    context.Items[CachedResponseFileDependency] = new[] { cachedPath };
                }

                // The cached file has been saved so now rewrite the path.
                this.imageCache.RewritePath(context);

                // Redirect if not a locally store file.
                if (!new Uri(cachedPath).IsFile)
                {
                    context.ApplicationInstance.CompleteRequest();
                }

                // Trim the cache.
                await this.imageCache.TrimCacheAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Return a value indicating whether common cache buster variables are being passed through.
        /// </summary>
        /// <param name="queryString">The query string to search.</param>
        /// <returns>
        ///   <c>true</c> if the query string contains cache buster variables; otherwise, <c>false</c>.
        /// </returns>
        private static bool ParseCacheBuster(string queryString) => ParseCacheBuster(HttpUtility.ParseQueryString(queryString));

        /// <summary>
        /// Return a value indicating whether common cache buster variables are being passed through.
        /// </summary>
        /// <param name="queryString">The query string to search.</param>
        /// <returns>
        ///   <c>true</c> if the query string contains cache buster variables; otherwise, <c>false</c>.
        /// </returns>
        private static bool ParseCacheBuster(NameValueCollection queryString)
        {
            return allowCacheBuster != null && allowCacheBuster.Value
                && (queryString.AllKeys.Contains("v", StringComparer.OrdinalIgnoreCase)
                || queryString.AllKeys.Contains("rnd", StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the animation mode passed in through the querystring, defaults to the default behaviour (All) if nothing found.
        /// </summary>
        /// <param name="queryString">The query string to search.</param>
        /// <param name="process">
        /// Whether to process the request. True if <see cref="AnimationProcessMode.First"/>
        /// has been explicitly requested.
        /// </param>
        /// <returns>
        /// The process mode for frames in animated images.
        /// </returns>
        private AnimationProcessMode ParseAnimationMode(string queryString, out bool process)
        {
            AnimationProcessMode mode = AnimationProcessMode.All;

            string decoded = !string.IsNullOrWhiteSpace(queryString) ? HttpUtility.HtmlDecode(queryString) : string.Empty;
            NameValueCollection queryCollection = HttpUtility.ParseQueryString(decoded);
            process = false;

            if (queryCollection["animationprocessmode"] is string animationProcessMode)
            {
                mode = QueryParamParser.Instance.ParseValue<AnimationProcessMode>(animationProcessMode);

                // Common sense would dictate that requesting AnimationProcessMode.All is a pointless request
                // since that is the default behaviour and shouldn't be requested on its own.
                // But hey! this is the internet and it's impossible to stop people twisting your intent into
                // whatever bizarre thing they choose. Those cats are crazy!
                process = true;
            }

            return mode;
        }

        /// <summary>
        /// Replaces preset values stored in the configuration in the querystring.
        /// </summary>
        /// <param name="queryString">The query string.</param>
        /// <returns>
        /// The <see cref="string"/> containing the updated querystring.
        /// </returns>
        private string ReplacePresetsInQueryString(string queryString)
        {
            if (!string.IsNullOrWhiteSpace(queryString))
            {
                queryString = PresetRegex.Replace(queryString, match =>
                {
                    string preset = match.Value.Split('=')[1];

                    // We use the processor config system to store the preset values.
                    return ImageProcessorConfiguration.Instance.GetPresetSettings(preset);
                });
            }

            return queryString;
        }

        /// <summary>
        /// Raises the ValidatingRequest event
        /// </summary>
        /// <param name="args">The <see cref="ValidatingRequestEventArgs"/></param>
        private void OnValidatingRequest(ValidatingRequestEventArgs args) => ValidatingRequest?.Invoke(this, args);

        /// <summary>
        /// Gets the correct <see cref="IImageService"/> for the given request.
        /// </summary>
        /// <param name="url">The current image request url.</param>
        /// <param name="applicationPath">The application path.</param>
        /// <returns>
        /// The <see cref="IImageService"/>.
        /// </returns>
        private IImageService GetImageServiceForRequest(string url, string applicationPath)
        {
            IList<IImageService> services = ImageProcessorConfiguration.Instance.ImageServices;

            // Remove the Application Path from the Request.Path.
            // This allows applications running on localhost as sub applications to work.
            string path = url.Split('?')[0].TrimStart(applicationPath).TrimStart('/');
            foreach (IImageService service in services)
            {
                var key = service.Prefix;
                if (!string.IsNullOrWhiteSpace(key) && path.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                {
                    return service;
                }
            }

            // Return the next unprefixed service.
            return services.FirstOrDefault(s => string.IsNullOrWhiteSpace(s.Prefix) && s.IsValidRequest(path));
        }
    }
}