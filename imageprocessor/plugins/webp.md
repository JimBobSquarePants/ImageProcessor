---
permalink: imageprocessor/plugins/webp/
layout: plugin
title: WebP
heading: Plugins - WebP
subheading: An extension that allows for processing of Google's WebP format.
sublinks:
 - "#about|WebP"
---
<section id="about">
#WebP Image Support

<a href="https://nuget.org/packages/ImageProcessor.Plugins.WebP/" role="button" class="download" data-ga-category="Plugin Actions" data-ga-action="Plugin Links" data-ga-label="WebP Plugin Nuget Link"><i class="fa fa-download"></i>Plugins.WebP</a>

<div class="alert" role="alert">

This plugin was added in version [2.0.0](https://www.nuget.org/packages/ImageProcessor/2.0.0). 

</div>

The WebP plugin is an extension to **ImageProcessor** that allows processing of images with Google's [WebP](https://developers.google.com/speed/webp/) format.

This is a new image format that provides lossless and lossy compression for images on the web. WebP lossless images are 26% smaller in size compared to PNGs. 
WebP lossy images are 25-34% smaller in size compared to JPEG images at equivalent SSIM index. 
    
WebP supports lossless transparency (also known as alpha channel) with just 22% additional bytes. 
Transparency is also supported with lossy compression and typically provides 3x smaller file sizes compared to PNG when lossy compression is acceptable for the red/green/blue color channels.

WebP support is added by adding an implementation of <a href="../extending/#isupportedimageformat" rel="chapter">ISupportedImageFormat<i class="fa fa-file-text-o"></i></a> to the solution for ImageProcessor to load automatically.

<div class="alert" role="alert">

Requires msvcr120.dll from the [Visual C++ Redistributable Package for Visual Studio 2013](http://www.microsoft.com/en-us/download/details.aspx?id=40784) to be installed on the server.

If you are using version 1.0.2  or lower it instead requires msvcr110.dll from the [Visual C++ Redistributable Package for Visual Studio 2012](http://www.microsoft.com/en-us/download/details.aspx?id=30679) to be installed.

</div>

</section>
