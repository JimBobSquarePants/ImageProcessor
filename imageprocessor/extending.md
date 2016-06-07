---
permalink: imageprocessor/extending/
layout: subpage
title: Extending ImageProcessor
heading: Extending ImageProcessor
subheading: Interfaces to allow the extension of ImageProcessor
sublinks:
 - "#igraphicsprocessor|IGraphicsProcessor"
 - "#isupportedimageformat|ISupportedImageFormat"
---

# Extending ImageProcessor

ImageProcessor is built with extensibility in mind. All the individual processors used within the ImageFactory methods
follow an interface `IGraphicsprocessor` which allows developers to easily extend the library.
View the [source code](https://github.com/JimBobSquarePants/ImageProcessor/tree/master/src/ImageProcessor/Processors) to see examples.
     

<div id="igraphicsprocessor">

## IGraphicsProcessor

All the individual image processors follow an interface `IWebGraphicsprocessor` which allows developers to extend the library.

View the [source code](https://github.com/JimBobSquarePants/ImageProcessor/tree/master/src/ImageProcessor.Web/Processors) to see examples.


{% highlight c# %}
/// <summary>
/// Defines properties and methods for ImageProcessor Plugins.
/// </summary>
public interface IGraphicsProcessor
{
    /// <summary>
    /// Gets or sets the DynamicParameter.
    /// </summary>
    dynamic DynamicParameter { get; set; }
    
    /// <summary>
    /// Gets or sets any additional settings required by the processor.
    /// </summary>
    Dictionary<string, string> Settings { get; set; }
 
    /// <summary>
    /// Processes the image.
    /// </summary>
    /// <param name="factory">
    /// The current instance of the <see cref="T:ImageProcessor.ImageFactory" /> class 
    /// containing the image to process.
    /// </param>
    /// <returns>
    /// The processed image from the current instance of the 
    /// <see cref="T:ImageProcessor.ImageFactory" /> class.
    /// </returns>
    Image ProcessImage(ImageFactory factory);
}
{% endhighlight %}
</div>
---
<div id="isupportedimageformat">

## ISupportedImageFormat

It is also possible to extend the image formats that ImageProcessor is capable of manipulating by providing a new class implementing
the `ISupportedImageFormat`` interface. ImageProcessor will automatically search for classes implementing that interface when initializing.
View the [source code](https://github.com/JimBobSquarePants/ImageProcessor/tree/master/src/ImageProcessor/Imaging/Formats) to see examples.  

{% highlight c# %}
/// <summary>
/// The ISupportedImageFormat interface providing information about image 
/// formats to ImageProcessor.
/// </summary>
public interface ISupportedImageFormat
{
    /// <summary>
    /// Gets the file headers.
    /// </summary>
    byte[][] FileHeaders { get; }
    
    /// <summary>
    /// Gets the list of file extensions.
    /// </summary>
    string[] FileExtensions { get; }
    
    /// <summary>
    /// Gets the standard identifier used on the Internet to indicate the type 
    /// of data that a file contains. 
    /// </summary>
    string MimeType { get; }
    
    /// <summary>
    /// Gets the default file extension.
    /// </summary>
    string DefaultExtension { get; }
    
    /// <summary>
    /// Gets the file format of the image. 
    /// </summary>
    ImageFormat ImageFormat { get; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the image format is indexed.
    /// </summary>
    bool IsIndexed { get; set; }
    
    /// <summary>
    /// Gets or sets the quality of output for images.
    /// </summary>
    int Quality { get; set; }
    
    /// <summary>
    /// Applies the given processor the current image.
    /// </summary>
    /// <param name="processor">
    /// The processor delegate.
    /// </param>
    /// <param name="factory">
    /// The <see cref="ImageFactory" />.
    /// </param>
    void ApplyProcessor(Func<imagefactory, image> processor, ImageFactory factory);
    
    /// <summary>
    /// Loads the image to process. 
    /// </summary>
    /// <param name="stream">
    /// The <see cref="T:System.IO.Stream" /> containing the image information.
    /// </param>
    /// <returns>
    /// The <see cref="T:System.Drawing.Image" />.
    /// </returns>
    Image Load(Stream stream);
    
    /// <summary>
    /// Saves the current image to the specified output stream.
    /// </summary>
    /// <param name="stream">
    /// The <see cref="T:System.IO.Stream"/> to save the image information to.
    /// </param>
    /// <param name="image">
    /// The <see cref="T:System.Drawing.Image"/> to save.
    /// </param>
    /// <param name="bitDepth">
    /// The color depth in number of bits per pixel to save the image with.
    /// </param>
    /// <returns>
    /// The <see cref="T:System.Drawing.Image"/>.
    /// </returns>
    Image Save(Stream stream, Image image, long bitDepth);
    
    /// <summary>
    /// Saves the current image to the specified file path.
    /// </summary>
    /// <param name="path">The path to save the image to.</param>
    /// <param name="image">
    /// The <see cref="T:System.Drawing.Image" /> to save.
    /// </param>
    /// <param name="bitDepth">
    /// The color depth in number of bits per pixel to save the image with.
    /// </param>
    /// <returns>
    /// The <see cref="T:System.Drawing.Image" />.
    /// </returns>
    Image Save(string path, Image image, long bitDepth);
}
{% endhighlight %}
</div>