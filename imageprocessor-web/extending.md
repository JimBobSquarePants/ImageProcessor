---
permalink: imageprocessor-web/extending/
layout: subpage
title: Extending ImageProcessor.Web
heading: Extending ImageProcessor.Web
subheading: Interfaces to allow the extension of ImageProcessor.Web
sublinks:
 - "#iwebgraphicsprocessor|IWebGraphicsProcessor"
 - "#iimageservice|IImageService"
 - "#iimagecache|IImageCache"
---

# Extending ImageProcessor.Web

ImageProcessor.Web is built with extensibility in mind. All the individual processors, all the image source, and image cache methods
used within the ImageProcessingModule can be replaced with individual implementations. 

<div id="iwebgraphicsprocessor">

## IWebGraphicsProcessor

All the individual image processors follow an interface `IWebGraphicsprocessor` which allows developers to extend the library.

View the [source code](https://github.com/JimBobSquarePants/ImageProcessor/tree/master/src/ImageProcessor.Web/Processors) to see examples.


{% highlight c# %}
/// <summary>
/// Defines properties and methods for ImageProcessor.Web Plugins.
/// </summary>
public interface IWebGraphicsProcessor
{
    /// <summary>
    /// Gets the regular expression to search strings for.
    /// </summary>
    Regex RegexPattern { get; }

    /// <summary>
    /// Gets the order in which this processor is to be used in a chain.
    /// </summary>
    int SortOrder { get; }

    /// <summary>
    /// Gets the associated graphics processor.
    /// </summary>
    IGraphicsProcessor Processor { get; }
 
    /// <summary>
    /// The position in the original string where the first character of the captured substring was found.
    /// </summary>
    /// <param name="queryString">
    /// The query string to search.
    /// </param>
    /// <returns>
    /// The zero-based starting position in the original string where the captured substring was found.
    /// </returns>
    int MatchRegexIndex(string queryString);
}
{% endhighlight %}
</div>
---
<div id="iimageservice">

## IImageService

The `IImageService` defines methods and properties which allow developers to extend ImageProcessor to retrieve
images from alternate locations to process.
View the [source code](https://github.com/JimBobSquarePants/ImageProcessor/tree/master/src/ImageProcessor.Web/Services/) to see examples.

<div class ="alert" role="alert">

This interface was added in version [4.1.0](https://www.nuget.org/packages/ImageProcessor.Web/4.1.0).

</div>

{% highlight c# %}
/// <summary>
///  Defines properties and methods for allowing retrieval of images from different sources.
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Gets or sets the prefix for the given implementation.
    /// <remarks>
    /// This value is used as a prefix for any image requests that should use this service.
    /// </remarks>
    /// </summary>
    string Prefix { get; set; }

    /// <summary>
    /// Gets a value indicating whether the image service requests files from
    /// the locally based file system.
    /// </summary>
    bool IsFileLocalService { get; }

    /// <summary>
    /// Gets or sets any additional settings required by the service.
    /// </summary>
    Dictionary<string, string> Settings { get; set; }

    /// <summary>
    /// Gets or sets the white list of <see cref="System.Uri"/>. 
    /// </summary>
    Uri[] WhiteList { get; set; }

    /// <summary>
    /// Gets a value indicating whether the current request passes sanitizing rules.
    /// </summary>
    /// <param name="path">
    /// The image path.
    /// </param>
    /// <returns>
    /// <c>True</c> if the request is valid; otherwise, <c>False</c>.
    /// </returns>
    bool IsValidRequest(string path);

    /// <summary>
    /// Gets the image using the given identifier.
    /// </summary>
    /// <param name="id">
    /// The value identifying the image to fetch.
    /// </param>
    /// <returns>
    /// The <see cref="System.Byte"/> array containing the image data.
    /// </returns>
    Task<byte[]> GetImage(object id);
}
{% endhighlight %}
</div>
---
<div id="iimagecache">

## IImageCache

The `IImageCache` defines methods and properties which allow developers to extend ImageProcessor to persist
cached images in alternate locations. For eample: Azure Blob Containers.
View the [source code](https://github.com/JimBobSquarePants/ImageProcessor/tree/master/src/ImageProcessor.Web/Caching/) to see examples.

<div class ="alert" role="alert">

This interface was added in version [4.2.0](https://www.nuget.org/packages/ImageProcessor.Web/4.2.0).

</div>

{% highlight c# %}
/// <summary>
///  Defines properties and methods for allowing caching of images to different sources.
/// </summary>
public interface IImageCache
{
    /// <summary>
    /// Gets or sets any additional settings required by the cache.
    /// </summary>
    Dictionary<string, string> Settings { get; set; }

    /// <summary>
    /// Gets the path to the cached image.
    /// </summary>
    string CachedPath { get; }

    /// <summary>
    /// Gets or sets the maximum number of days to store the image.
    /// </summary>
    int MaxDays { get; set; }

    /// <summary>
    /// Gets a value indicating whether the image is new or updated in an asynchronous manner.
    /// </summary>
    /// <returns>
    /// The asynchronous <see cref="Task"/> returning the value.
    /// </returns>
    Task<bool> IsNewOrUpdatedAsync();

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
    Task AddImageToCacheAsync(Stream stream, string contentType);

    /// <summary>
    /// Trims the cache of any expired items in an asynchronous manner.
    /// </summary>
    /// <returns>
    /// The asynchronous <see cref="Task"/> representing an asynchronous operation.
    /// </returns>
    Task TrimCacheAsync();

    /// <summary>
    /// Gets a string identifying the cached file name in an asynchronous manner.
    /// </summary>
    /// <returns>
    /// The asynchronous <see cref="Task"/> returning the value.
    /// </returns>
    Task<string> CreateCachedFileNameAsync();

    /// <summary>
    /// Rewrites the path to point to the cached image.
    /// </summary>
    /// <param name="context">
    /// The <see cref="HttpContext"/> encapsulating all information about the request.
    /// </param>
    void RewritePath(HttpContext context);
}
{% endhighlight %}
</div>