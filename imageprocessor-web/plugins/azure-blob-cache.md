---
permalink: imageprocessor-web/plugins/azure-blob-cache/
layout: plugin
title: Azure Blob Cache
heading: Plugins - Azure Blob Cache
subheading: An extension that allows for caching on Azure Blob Storage.
sublinks:
 - "#about|Azure Blob Cache"
 - "#config|Configuration"
---
<section id="about">
#Azure Blob Cache

<a href="https://nuget.org/packages/ImageProcessor.Web.Plugins.AzureBlobCache/" role="button" class="download" data-ga-category="Plugin Actions" data-ga-action="Plugin Links" data-ga-label="AzureBlobCache Plugin Nuget Link"><i class="fa fa-download"></i>Plugins.AzureBlobCache</a>

<div class="alert" role="alert">

This plugin was added in version [4.2.0](https://www.nuget.org/packages/ImageProcessor.Web/4.2.0). 

</div>

The Azure blob cache pluging is an extension to the caching mechanism which allows caching of images within
[AzureBlob Storage](http://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-blobs/) to serve
via a content delivery network. This is extremely useful for load balanced or high traffic sites where images have to be served
outwith the website for increased performance.

The caching mechanism will store processed image files from any allowable image location including but not limited to
local, remote, and blob stored locations. The cache is self cleaning and will automatically update itself should a source
image change. It is interchangeable with the default disk cache with very little configuration.
</section>
<hr />
<section id="config">
#Configuration

Upon installation the following will be added to the [Cache.Config](../configuration/#cacheconfig) section in the 
configuration files.

{% highlight xml %}
<caching currentCache="AzureBlobCache">
  <caches>
    <!-- Disk cache configuration removed for brevity -->
    <cache name="AzureBlobCache" type="ImageProcessor.Web.Caching.AzureBlobCache, ImageProcessor.Web.Caching.AzureBlobCache" maxDays="365">
      <settings>
        <!-- The Account, Container and CDN details -->
        <setting key="CachedStorageAccount" value="DefaultEndpointsProtocol=https;AccountName=[CacheAccountName];AccountKey=[CacheAccountKey]"/>
        <setting key="CachedBlobContainer" value="cache"/>
        <!-- Full CDN root url e.g http://123456.vo.msecnd.net/ -->
        <setting key="CachedCDNRoot" value="[CdnRootUrl]"/>
        <!-- 
            Optional settings for better identifcation of source images if stored in 
            Azure blob storage.
         -->
        <setting key="SourceStorageAccount" value=""/>
        <setting key="SourceBlobContainer" value=""/>
      </settings>
    </cache>
  </caches>
</caching>
{% endhighlight %}
</section>