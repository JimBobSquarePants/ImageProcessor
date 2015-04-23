---
permalink: imageprocessor-web/imageprocessingmodule/backgroundcolor/
redirect_from: "imageprocessor-web/imageprocessingmodule/backgroundcolor.html"

title: BackgroundColor
subheading: BackgroundColor
sublinks: ["#usage|Usage","#example|Examples"]
---
<section id="usage">

# BackgroundColor

Changes the background color of the current image. This functionality is useful 
for adding a background when resizing image formats without an alpha channel. 

Requests are passed thus:
{% highlight xml %}
<!--Hex-->
http://your-image?bgcolor=800080
<!--RGBA-->
http://your-image?bgcolor=128,0,128,255
<!--Known Color-->
http://your-image?bgcolor=purple
{% endhighlight %}
</section>
---
<section id="example">

# Examples

### Original

![Original image]({{ site.dynamicimageroot }}beach.jpg?width=600&height=300)

### Purple Background Color

![Image with a purple background color applied]({{ site.dynamicimageroot }}beach.jpg?width=600&height=300&bgcolor=purple)

</section>
