---
permalink: imageprocessor-web/imageprocessingmodule/brightness/
redirect_from: "imageprocessor-web/imageprocessingmodule/brightness.html"

title: Brightness
subheading: Brightness
sublinks: ["#usage|Usage","#example|Examples"]
---
<section id="usage">

# Brightness

Adjusts the brightness of images. Pass the desired percentage
value (without the '%') to the processor.

Requests are passed thus:

{% highlight xml %}
<!--Increasing-->
http://your-image?brightness=25
<!--Decreasing-->
http://your-image?brightness=-25
{% endhighlight %}

</section>
---
<section id="example">
# Examples

### Original

![Original image]({{ /assets/img/originals/lock.jpg | relative_url }})

### 25% Brightness

![Image with brightness adjusted]({{ /assets/img/brightness/lock.jpg | relative_url }})

</section>