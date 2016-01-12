// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageProcessingModule.cs" company="James South">
//   Copyright (c) James South.
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
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Hosting;

    using ImageProcessor.Imaging.Formats;
    using ImageProcessor.Web.Caching;
    using ImageProcessor.Web.Configuration;
    using ImageProcessor.Web.Extensions;
    using ImageProcessor.Web.Helpers;
    using ImageProcessor.Web.Services;

    /// <summary>
    /// Processes any image requests within the web application.
    /// </summary>
    public sealed class ImageProcessingModule : IHttpModule
    {
        #region Fields

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
        private static readonly Regex PresetRegex = new Regex(@"preset=[^&]+", RegexOptions.Compiled);

        /// <summary>
        /// The regular expression to search strings for protocols with.
        /// </summary>
        private static readonly Regex ProtocolRegex = new Regex("http(s)?://", RegexOptions.Compiled);

        /// <summary>
        /// The assembly version.
        /// </summary>
        private static readonly string AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// Ensures duplicate requests are atomic.
        /// </summary>
        private static readonly AsyncDuplicateLock Locker = new AsyncDuplicateLock();

        /// <summary>
        /// Whether to preserve exif meta data.
        /// </summary>
        private static bool? preserveExifMetaData;

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
        #endregion

        #region Destructors
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
        #endregion

        /// <summary>
        /// The process querystring event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The <see cref="ProcessQueryStringEventArgs"/>.
        /// </param>
        /// <returns>Returns the processed querystring.</returns>
        public delegate string ProcessQuerystringEventHandler(object sender, ProcessQueryStringEventArgs e);

        /// <summary>
        /// The event that is called when a new image is processed.
        /// </summary>
        public static event EventHandler<PostProcessingEventArgs> OnPostProcessing;

        /// <summary>
        /// The event that is called when a querystring is processed.
        /// </summary>
        public static event ProcessQuerystringEventHandler OnProcessQuerystring;

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
        public static void SetHeaders(HttpContext context, string responseType, IEnumerable<string> dependencyPaths, int maxDays, HttpStatusCode? statusCode = null)
        {
            HttpResponse response = context.Response;

            if (response.Headers["ImageProcessedBy"] == null)
            {
                response.AddHeader("ImageProcessedBy", "ImageProcessor.Web/" + AssemblyVersion);
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
            cache.SetRevalidation(HttpCacheRevalidation.AllCaches);

            AddCorsRequestHeaders(context);
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
            if (!string.IsNullOrEmpty(context.Request.Headers["Origin"]))
            {
                string origin = context.Request.Headers["Origin"];

                ImageSecuritySection.CORSOriginElement origins =
                    ImageProcessorConfiguration.Instance.GetImageSecuritySection().CORSOrigin;

                if (origins == null || origins.WhiteList == null)
                {
                    return;
                }

                // Check the url is from a whitelisted location.
                Uri url = new Uri(origin);
                string upper = url.Host.ToUpperInvariant();

                // Check for root or sub domain.
                bool validUrl = false;
                foreach (Uri uri in origins.WhiteList)
                {
                    if (uri.ToString() == "*")
                    {
                        validUrl = true;
                        break;
                    }

                    if (!uri.IsAbsoluteUri)
                    {
                        Uri rebaseUri = new Uri("http://" + uri.ToString().TrimStart('.', '/'));
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

                if (validUrl)
                {
                    context.Response.AddHeader("Access-Control-Allow-Origin", origin);
                }
            }
        }

        #region IHttpModule Members
        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="application">
        /// An <see cref="T:System.Web.HttpApplication"/> that provides
        /// access to the methods, properties, and events common to all
        /// application objects within an ASP.NET application
        /// </param>
        public void Init(HttpApplication application)
        {
            if (preserveExifMetaData == null)
            {
                preserveExifMetaData = ImageProcessorConfiguration.Instance.PreserveExifMetaData;
            }

            if (fixGamma == null)
            {
                fixGamma = ImageProcessorConfiguration.Instance.FixGamma;
            }

            if (interceptAllRequests == null)
            {
                interceptAllRequests = ImageProcessorConfiguration.Instance.InterceptAllRequests;
            }

            EventHandlerTaskAsyncHelper postAuthorizeHelper = new EventHandlerTaskAsyncHelper(this.PostAuthorizeRequest);
            application.AddOnPostAuthorizeRequestAsync(postAuthorizeHelper.BeginEventHandler, postAuthorizeHelper.EndEventHandler);

            application.PostReleaseRequestState += this.PostReleaseRequestState;

            EventHandlerTaskAsyncHelper onEndRquestsHelper = new EventHandlerTaskAsyncHelper(this.OnEndRequest);
            application.AddOnEndRequestAsync(onEndRquestsHelper.BeginEventHandler, onEndRquestsHelper.EndEventHandler);
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
        #endregion

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
        private Task PostAuthorizeRequest(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;
            return this.ProcessImageAsync(context);
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
        /// <returns>
        /// The <see cref="T:System.Threading.Tasks.Task"/>.
        /// </returns>
        private async Task OnEndRequest(object sender, EventArgs e)
        {
            if (this.imageCache != null)
            {
                // Trim the cache.
                await this.imageCache.TrimCacheAsync();

                // Reset the cache.
                this.imageCache = null;
            }
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

            object responseTypeObject = context.Items[CachedResponseTypeKey];
            object dependencyFileObject = context.Items[CachedResponseFileDependency];

            string responseType = responseTypeObject as string;
            List<string> dependencyFiles = dependencyFileObject as List<string>;

            // Set the headers
            if (this.imageCache != null)
            {
                SetHeaders(context, responseType, dependencyFiles, this.imageCache.MaxDays);
            }
        }

        #region Private
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

            // Should we ignore this request?
            if (request.Unvalidated.RawUrl.ToUpperInvariant().Contains("IPIGNORE=TRUE"))
            {
                return;
            }

            IImageService currentService = this.GetImageServiceForRequest(request);

            if (currentService != null)
            {
                bool isFileLocal = currentService.IsFileLocalService;
                string url = request.Url.ToString();
                bool isLegacy = ProtocolRegex.Matches(url).Count > 1;
                bool hasMultiParams = url.Count(f => f == '?') > 1;
                string requestPath;
                string queryString = string.Empty;
                string urlParameters = string.Empty;

                // Legacy support. I'd like to remove this asap.
                if (isLegacy && hasMultiParams)
                {
                    // We need to split the querystring to get the actual values we want.
                    string[] paths = url.Split('?');
                    requestPath = paths[1];

                    // Handle extension-less urls.
                    if (paths.Length > 3)
                    {
                        queryString = paths[3];
                        urlParameters = paths[2];
                    }
                    else if (paths.Length > 1)
                    {
                        queryString = paths[2];
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(currentService.Prefix))
                    {
                        requestPath = currentService.IsFileLocalService
                            ? HostingEnvironment.MapPath(request.Path)
                            : request.Path;
                        queryString = request.QueryString.ToString();
                    }
                    else
                    {
                        // Parse any protocol values from settings.
                        string protocol = currentService.Settings.ContainsKey("Protocol")
                                              ? currentService.Settings["Protocol"] + "://"
                                              : currentService.GetType() == typeof(RemoteImageService) ? request.Url.Scheme + "://" : string.Empty;

                        // Handle requests that require parameters.
                        if (hasMultiParams)
                        {
                            string[] paths = url.Split('?');
                            requestPath = protocol
                                          + request.Path.TrimStart('/').Remove(0, currentService.Prefix.Length).TrimStart('/')
                                          + "?" + paths[1];
                            queryString = paths[2];
                        }
                        else
                        {
                            requestPath = protocol + request.Path.TrimStart('/').Remove(0, currentService.Prefix.Length).TrimStart('/');
                            queryString = request.QueryString.ToString();
                        }
                    }
                }

                // Replace any presets in the querystring with the actual value.
                queryString = this.ReplacePresetsInQueryString(queryString);

                // Execute the handler which can change the querystring 
                queryString = this.CheckQuerystringHandler(context, queryString, request.Unvalidated.RawUrl);

                // Break out if we don't meet critera.
                bool interceptAll = interceptAllRequests != null && interceptAllRequests.Value;
                if (string.IsNullOrWhiteSpace(requestPath) || (!interceptAll && string.IsNullOrWhiteSpace(queryString)))
                {
                    return;
                }

                string parts = !string.IsNullOrWhiteSpace(urlParameters) ? "?" + urlParameters : string.Empty;
                string fullPath = string.Format("{0}{1}?{2}", requestPath, parts, queryString);
                object resourcePath;

                // More legacy support code.
                if (hasMultiParams)
                {
                    resourcePath = string.IsNullOrWhiteSpace(urlParameters)
                        ? new Uri(requestPath, UriKind.RelativeOrAbsolute)
                        : new Uri(requestPath + "?" + urlParameters, UriKind.RelativeOrAbsolute);
                }
                else
                {
                    resourcePath = requestPath;
                }

                // Check whether the path is valid for other requests.
                if (!currentService.IsValidRequest(resourcePath.ToString()))
                {
                    return;
                }

                string combined = requestPath + fullPath + queryString;
                using (await Locker.LockAsync(combined))
                {
                    // Create a new cache to help process and cache the request.
                    this.imageCache = (IImageCache)ImageProcessorConfiguration.Instance
                        .ImageCache.GetInstance(requestPath, fullPath, queryString);

                    // Is the file new or updated?
                    bool isNewOrUpdated = await this.imageCache.IsNewOrUpdatedAsync();
                    string cachedPath = this.imageCache.CachedPath;

                    // Only process if the file has been updated.
                    if (isNewOrUpdated)
                    {
                        // Process the image.
                        bool exif = preserveExifMetaData != null && preserveExifMetaData.Value;
                        bool gamma = fixGamma != null && fixGamma.Value;
                        using (ImageFactory imageFactory = new ImageFactory(exif, gamma))
                        {
                            byte[] imageBuffer = null;
                            string mimeType;

                            try
                            {
                                imageBuffer = await currentService.GetImage(resourcePath);
                            }
                            catch (HttpException ex)
                            {
                                // We want 404's to be handled by IIS so that other handlers/modules can still run.
                                if (ex.GetHttpCode() == (int)HttpStatusCode.NotFound)
                                {
                                    return;
                                }
                            }

                            if (imageBuffer == null)
                            {
                                return;
                            }

                            using (MemoryStream inStream = new MemoryStream(imageBuffer))
                            {
                                // Process the Image
                                MemoryStream outStream = new MemoryStream();

                                if (!string.IsNullOrWhiteSpace(queryString))
                                {
                                    imageFactory.Load(inStream).AutoProcess(queryString).Save(outStream);
                                    mimeType = imageFactory.CurrentImageFormat.MimeType;
                                }
                                else
                                {
                                    await inStream.CopyToAsync(outStream);
                                    mimeType = FormatUtilities.GetFormat(outStream).MimeType;
                                }

                                // Fire the post processing event.
                                EventHandler<PostProcessingEventArgs> handler = OnPostProcessing;
                                if (handler != null)
                                {
                                    string extension = Path.GetExtension(cachedPath);
                                    PostProcessingEventArgs args = new PostProcessingEventArgs
                                    {
                                        Context = context,
                                        ImageStream = outStream,
                                        ImageExtension = extension
                                    };

                                    handler(this, args);
                                    outStream = args.ImageStream;
                                }

                                // Add to the cache.
                                await this.imageCache.AddImageToCacheAsync(outStream, mimeType);

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
                                        ? new List<string> { cachedPath }
                                        : new List<string> { requestPath, cachedPath };
                                }
                                else
                                {
                                    context.Items[CachedResponseFileDependency] = Path.GetFileName(requestPath) == requestPath
                                        ? null
                                        : new List<string> { requestPath };
                                }
                            }
                            else if (isFileCached)
                            {
                                context.Items[CachedResponseFileDependency] = new List<string> { cachedPath };
                            }
                        }
                    }

                    // The cached file is valid so just rewrite the path.
                    this.imageCache.RewritePath(context);

                    // Redirect if not a locally store file.
                    if (!new Uri(cachedPath).IsFile)
                    {
                        context.ApplicationInstance.CompleteRequest();
                    }
                }
            }
        }

        /// <summary>
        /// Replaces preset values stored in the configuration in the querystring.
        /// </summary>
        /// <param name="queryString">
        /// The query string.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> containing the updated querystring.
        /// </returns>
        private string ReplacePresetsInQueryString(string queryString)
        {
            if (!string.IsNullOrWhiteSpace(queryString))
            {
                foreach (Match match in PresetRegex.Matches(queryString))
                {
                    if (match.Success)
                    {
                        string preset = match.Value.Split('=')[1];

                        // We use the processor config system to store the preset values.
                        string replacements = ImageProcessorConfiguration.Instance.GetPresetSettings(preset);
                        queryString = Regex.Replace(queryString, match.Value, replacements ?? string.Empty);
                    }
                }
            }

            return queryString;
        }

        /// <summary>
        /// Checks if there is a handler that changes the querystring and executes that handler.
        /// </summary>
        /// <param name="context">The current request context.</param>
        /// <param name="queryString">The query string.</param>
        /// <param name="rawUrl">The raw request url.</param>
        /// <returns>
        /// The <see cref="string"/> containing the updated querystring.
        /// </returns>
        private string CheckQuerystringHandler(HttpContext context, string queryString, string rawUrl)
        {
            // Fire the process querystring event.
            ProcessQuerystringEventHandler handler = OnProcessQuerystring;
            if (handler != null)
            {
                ProcessQueryStringEventArgs args = new ProcessQueryStringEventArgs
                {
                    Context = context,
                    Querystring = queryString ?? string.Empty,
                    RawUrl = rawUrl ?? string.Empty
                };
                queryString = handler(this, args);
            }

            return queryString;
        }

        /// <summary>
        /// Gets the correct <see cref="IImageService"/> for the given request.
        /// </summary>
        /// <param name="request">
        /// The current image request.
        /// </param>
        /// <returns>
        /// The <see cref="IImageService"/>.
        /// </returns>
        private IImageService GetImageServiceForRequest(HttpRequest request)
        {
            IImageService imageService = null;
            IList<IImageService> services = ImageProcessorConfiguration.Instance.ImageServices;

            string path = request.Path.TrimStart('/');
            foreach (IImageService service in services)
            {
                string key = service.Prefix;
                if (!string.IsNullOrWhiteSpace(key) && path.StartsWith(key, StringComparison.InvariantCultureIgnoreCase))
                {
                    imageService = service;
                }
            }

            if (imageService != null)
            {
                return imageService;
            }

            // Return the file based service.
            if (services.Any(s => s.GetType() == typeof(LocalFileImageService)))
            {
                IImageService service = services.First(s => s.GetType() == typeof(LocalFileImageService));
                if (service.IsValidRequest(path))
                {
                    return service;
                }
            }

            // Return the next unprefixed service.
            return services.FirstOrDefault(s => string.IsNullOrWhiteSpace(s.Prefix) && s.IsValidRequest(path) && s.GetType() != typeof(LocalFileImageService));
        }
        #endregion
    }
}