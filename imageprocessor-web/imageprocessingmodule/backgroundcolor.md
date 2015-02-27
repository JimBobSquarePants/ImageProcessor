---
permalink: imageprocessor-web/imageprocessingmodule/backgroundcolor/
redirect_from: "imageprocessor-web/imageprocessingmodule/backgroundcolor.html"

title: BackgroundColor
subheading: BackgroundColor
sublinks: ["#usage|Usage","#example|Examples"]
---
<section id="usage">

# BackgroundColor

Changes the background color of the current image.</p>

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

![Original image]({{ site.baseurl }}/assets/img/rounded/beach.png)

### Purple Background Color

![Image with a purple background color applied]({{ site.baseurl }}/assets/img/bgcolor/beach.jpg)

</section>
