---
permalink: imageprocessor-web/imageprocessingmodule/animationprocessmode/

title: animationprocessmode
subheading: Animation Process Mode
sublinks: ["#usage|Usage"]
---
<section id="usage">

# Animation Process Mode

Defines whether gif images are processed to preserve animation or processed keeping the first frame only.

<div class="alert" role="alert">
<p>Added version 4.5.3</p>
</div>

Requests are passed thus:

{% highlight xml %}
http://your-image?animationprocessmode=all

http://your-image?animationprocessmode=first
{% endhighlight %}
    
### Remarks

Defaults to `animationprocessmode=all`
</section>
