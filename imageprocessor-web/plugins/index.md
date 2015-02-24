---
permalink: imageprocessor-web/plugins/
layout: subpage
title: Plugins
heading: Plugins
subheading: Additional plugins for enhancing the functionality of ImageProcessor.Web.
sublinks:
 - "#about|About"
 - "#azure|Azure Blob Cache"
---
<section id="about">
#Additional Plugins

**ImageProcessor.Web** has a plugin based architecture which allows for additional functionality to
be [added to the library](../extending). The following plugins are available for download.

</section>
<hr />
<section id="azure">
#Azure Blob Cache

The Azure blob cache pluging is an extension to the caching mechanism which allows caching of images within
[Azure Blob Storage](http://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-blobs/) to serve 
via a content delivery network. This is extremely useful for load balanced or high traffic sites where images have to be served 
outwith the website for increased performance.

See the [Azure Blob Cache](azure-blob-cache) documentation for details.

</section>