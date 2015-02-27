---
permalink: imageprocessor-web/imageprocessingmodule/alpha/
redirect_from: "imageprocessor-web/imageprocessingmodule/alpha.html"

title: Alpha
subheading: Alpha
sublinks: ["#usage|Usage","#example|Examples"]
---
<section id="usage">

# Alpha

Adjusts the alpha transparency of images. Pass the desired percentage
value (without the '%') to the processor.

Requests are passed thus:

{% highlight xml %}
http://your-image?alpha=50
{% endhighlight %}
</section>
---
<section id="example">

# Examples

### Original

![Original image]({{ site.baseurl }}/assets/img/originals/balloon.jpg)

### 50% Alpha

![Image with alpha transparency adjusted]({{ site.baseurl }}/assets/img/alpha/balloon.png)
</section>
