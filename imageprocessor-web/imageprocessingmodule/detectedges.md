---
permalink: imageprocessor-web/imageprocessingmodule/detectedges/
redirect_from: "imageprocessor-web/imageprocessingmodule/detectedges.html"

title: DetectEdges
heading: Methods
subheading: DetectEdges
sublinks: ["#usage|Usage","#example|Examples"]
---
<section id="usage">

# DetectEdges

Detects the edges in the current image using various one and two dimensional algorithms.
If the `greyscale` parameter is set to false the detected edges will maintain the pixel
colors of the originl image.

<div class="alert" role="alert">
<p>Added version 4.1.0</p>
</div>

Available filters are:

- kayyali
- kirsch
- laplacian3X3
- laplacian5X5
- laplacianffgaussian
- prewitt
- robertscross
- scharr
- sobel

Requests are passed thus:

{% highlight xml %}
http://your-image?detectedges=kayyali&greyscale=false
{% endhighlight %}

</section>
---
<section id="example">
# Examples

### Original
![Original image]({{ site.dynamicimageroot }}woman.jpg?width=600)

### Kayyali
![Image with kayyali filter applied]({{ site.dynamicimageroot }}woman.jpg?width=600&detectedges=kayyali)

### Kirsch
![Image with kirsch filter applied]({{ site.dynamicimageroot }}woman.jpg?width=600&detectedges=kirsch)

### Laplacian3X3
![Image with laplacian 3X3 filter applied]({{ site.dynamicimageroot }}woman.jpg?width=600&detectedges=laplacian3x3)

### Laplacian5X5
![Image with laplacian 5X5 filter applied]({{ site.dynamicimageroot }}woman.jpg?width=600&detectedges=laplacian5x5)

### LaplacianOfGaussian
![Image with laplacian gaussian filter applied]({{ site.dynamicimageroot }}woman.jpg?width=600&detectedges=laplacianofgaussian)

### Prewitt
![Image with prewitt filter applied]({{ site.dynamicimageroot }}woman.jpg?width=600&detectedges=prewitt)

### RobertsCross
![Image with roberts cross filter applied]({{ site.dynamicimageroot }}woman.jpg?width=600&detectedges=robertscross)

### Scharr
![Image with scharr filter applied]({{ site.dynamicimageroot }}woman.jpg?width=600&detectedges=scharr)

### Sobel
![Image with sobel filter applied]({{ site.dynamicimageroot }}woman.jpg?width=600&detectedges=sobel)

</section>