---
permalink: imageprocessor-web/imageprocessingmodule/contrast/
redirect_from: "imageprocessor-web/imageprocessingmodule/contrast.html"

title: Contrast
subheading: Contrast
sublinks: ["#usage|Usage","#example|Examples"]
---
<section id="usage">
#Contrast

Adjusts the contrast of images. Pass the desired percentage
value (without the '%') to the processor.

Requests are passed thus:

{% highlight xml %}
<!--Increasing-->
http://your-image?contrast=25
<!--Decreasing-->
http://your-image?contrast=-25
{% endhighlight %}

</section>
---
<section id="example">
#Examples

###Original
![Original image]({{ site.baseurl }}/assets/img/originals/blackforrest.jpg)

###25% Contrast
![Image with contrast adjusted]({{ site.baseurl }}/assets/img/contrast/blackforrest.jpg)
</section>