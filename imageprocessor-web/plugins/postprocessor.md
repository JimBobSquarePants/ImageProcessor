---
permalink: imageprocessor-web/plugins/postprocessor/
layout: plugin
title: Post Processor
heading: Plugins - Post Processor
subheading: An extension that allows for post processing using various third party tools.
sublinks:
 - "#about|PostProcessor"
---
<section id="about">

# PostProcessor

<a href="https://www.nuget.org/packages/ImageProcessor.Web.PostProcessor/" 
   role="button" 
   class="download" 
   data-ga-category="Plugin Actions" 
   data-ga-action="Plugin Links" 
   data-ga-label="PostProcessor Plugin Nuget Link"><i class="fa fa-download"></i>Plugins.PostProcessor</a>

The PostProcessor plugin is an extension to the processing pipeline that applies various optimisation
techniques using third party plugins once an image has been cached. These optimisations can reduce images by up to 
60% in certain circumstances.

The various tools included in the plugin are:

 - gifsicle : http://www.lcdf.org/gifsicle/
 - optipng : http://optipng.sourceforge.net/
 - pngcrush : http://pmt.sourceforge.net/pngcrush/
 - jpegtran : http://jpegclub.org/

Once installed, the plugin requires no further configuration.

</section>
