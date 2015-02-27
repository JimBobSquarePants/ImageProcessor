---
permalink: imageprocessor-web/imageprocessingmodule/crop/
redirect_from: "imageprocessor-web/imageprocessingmodule/crop.html"

title: Crop
heading: Methods
subheading: Crop
sublinks: ["#usage|Usage","#example|Examples"]
---
<section id="usage">

# Crop

Crops the current image to the given location and size.
There are two modes available:

 1. Pixel based - Supply the upper-left coordinates and the new width/height.

 2. Percentage based - Supply the left, top, right, and bottom percentages as a decimal between 0 and 1 to crop with an indicator to switch mode.

Requests are passed thus:

{% highlight xml %}
<!--Pixel-->
http://your-image?crop=x,y,width,height
<!--Percentage: 
    Current -  Each value is a decimal between 0 and 100. 
    Legacy - Each value is a decimal between 0 and 1. -->
http://your-image?crop=left,top,right,bottom&cropmode=percent
{% endhighlight %}

</section>
---
<section id="example">

# Examples

### Original

![Original image]({{ site.baseurl }}/assets/img/originals/coffee.jpg)

### Cropped

![Image with contrast adjusted]({{ site.baseurl }}/assets/img/crop/coffee.jpg)

</section>