---
permalink: imageprocessor-web/imageprocessingmodule/autorotate/
redirect_from: "imageprocessor-web/imageprocessingmodule/autorotate.html"

title: AutoRotate
subheading: AutoRotate
sublinks: ["#usage|Usage"]
---
<section id="usage">
#AutoRotate

Performs auto-rotation to ensure that EXIF defined rotation is reflected in the final image.

Requests are passed thus:

{% highlight xml %}
http://your-image?autorotate=true
{% endhighlight %}
    
###Remarks

If EXIF preservation is set to preserve metadata during processing this method will not alter the images rotation.
</section>
