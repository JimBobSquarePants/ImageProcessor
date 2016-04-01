using System;
using System.Collections.Generic;

namespace ImageProcessorCore.Formats
{
    /// <summary>
    /// The class maintains a list of well know tiff tags. A good resource for Tiff Tag info
    /// is http://www.awaresystems.be/imaging/tiff/tifftags.html. This site maintains a huge
    /// list of tags with names and descriptions of each tag.
    /// </summary>
    public class TiffTagRegistry
    {
        private List<TiffTag> _tags;

        private static readonly Lazy<TiffTagRegistry> Lazy = new Lazy<TiffTagRegistry>(() => new TiffTagRegistry());

        public static TiffTagRegistry Instance => Lazy.Value;

        private TiffTagRegistry()
        {
            BuildRegistryOfWellKnownTags();
        }
        
        public IEnumerable<TiffTag> Tags => _tags;

        private void BuildRegistryOfWellKnownTags()
        {
            _tags = new List<TiffTag>
            {
                new TiffTag { TagId = 0, TagGroup = "General", Name = "Ignore", Description = "Tag Placeholder" },
                new TiffTag { TagId = 254, TagGroup = "General", Name = "New Sub File Type", Description = "A general indication of the kind of data contained in this subfile."},
                new TiffTag { TagId = 255, TagGroup = "General", Name = "Sub File Type", Description = "A general indication of the kind of data contained in this subfile."},
                new TiffTag { TagId = 256, TagGroup = "General", Name = "Image Width", Description = "The number of columns in the image, i.e., the number of pixels per row."},
                new TiffTag { TagId = 257, TagGroup = "General", Name = "Image Length", Description = "The number of rows of pixels in the image."},
                new TiffTag { TagId = 258, TagGroup = "General", Name = "Bits Per Sample", Description = "Number of bits per component."},
                new TiffTag { TagId = 259, TagGroup = "General", Name = "Compression Schema", Description = "Compression scheme used on the image data."},
                new TiffTag { TagId = 262, TagGroup = "General", Name = "Photometric Interpretation", Description = "The color space of the image data."},
                new TiffTag { TagId = 263, TagGroup = "General", Name = "Thresh Holding", Description = "For black and white TIFF files that represent shades of gray, the technique used to convert from gray to black and white pixels."},
                new TiffTag { TagId = 264, TagGroup = "General", Name = "Cell Width", Description = "The width of the dithering or halftoning matrix used to create a dithered or halftoned bilevel file."},
                new TiffTag { TagId = 265, TagGroup = "General", Name = "Cell Length", Description = "The length of the dithering or halftoning matrix used to create a dithered or halftoned bilevel file."},
                new TiffTag { TagId = 266, TagGroup = "General", Name = "Fill Order", Description = "The logical order of bits within a byte."},
                new TiffTag { TagId = 269, TagGroup = "General", Name = "Document Name", Description = "The name of the document from which this image was scanned."},
                new TiffTag { TagId = 270, TagGroup = "General", Name = "Image Description", Description = "A string that describes the subject of the image."},
                new TiffTag { TagId = 271, TagGroup = "General", Name = "Make", Description = "The scanner manufacturer."},
                new TiffTag { TagId = 272, TagGroup = "General", Name = "Model", Description = "The scanner model name or number."},
                new TiffTag { TagId = 274, TagGroup = "General", Name = "Orientation", Description = "The orientation of the image with respect to the rows and columns."},
                new TiffTag { TagId = 277, TagGroup = "General", Name = "Samples Per Pixel", Description = ""},
                new TiffTag { TagId = 278, TagGroup = "General", Name = "Rows Per Strip", Description = "The number of rows in a strip of data."},
                new TiffTag { TagId = 279, TagGroup = "General", Name = "Strip Byte Count", Description = "The number of bytes for a strip of data"},
                new TiffTag { TagId = 280, TagGroup = "General", Name = "Minimum Sample Value", Description = "The minimum sample value."},
                new TiffTag { TagId = 281, TagGroup = "General", Name = "Maximum Sample Value", Description = "The maximum sample value."},
                new TiffTag { TagId = 282, TagGroup = "General", Name = "X Resolution", Description = ""},
                new TiffTag { TagId = 283, TagGroup = "General", Name = "Y Resolution", Description = ""},
                new TiffTag { TagId = 284, TagGroup = "General", Name = "Planer Config", Description = ""},
                new TiffTag { TagId = 285, TagGroup = "General", Name = "Page Name", Description = ""},
                new TiffTag { TagId = 286, TagGroup = "General", Name = "X Position", Description = ""},
                new TiffTag { TagId = 287, TagGroup = "General", Name = "Y Position", Description = ""},
                new TiffTag { TagId = 288, TagGroup = "General", Name = "Free Offsets", Description = ""},
                new TiffTag { TagId = 289, TagGroup = "General", Name = "Free Byte Count", Description = ""},
                new TiffTag { TagId = 296, TagGroup = "General", Name = "Resolution Unit", Description = "The unit of measurement for XResolution and YResolution." },
                new TiffTag { TagId = 305, TagGroup = "General", Name = "Software", Description = ""},
                new TiffTag { TagId = 306, TagGroup = "General", Name = "Creation Date And Time", Description = ""},
                new TiffTag { TagId = 315, TagGroup = "General", Name = "Artist", Description = ""},
                new TiffTag { TagId = 316, TagGroup = "General", Name = "Computer Created On", Description = ""},
                new TiffTag { TagId = 318, TagGroup = "General", Name = "White Point", Description = "Image white point."},
                new TiffTag { TagId = 320, TagGroup = "General", Name = "Color Map", Description = "RGB map for pallete image"},
                new TiffTag { TagId = 330, TagGroup = "General", Name = "Sub Image ID", Description = "Sub Image Descriptor"},
                new TiffTag { TagId = 531, TagGroup = "General", Name = "YCbCrPositioning", Description ="Specifies the positioning of subsampled chrominance components relative to luminance samples."},
                new TiffTag { TagId = 33434, TagGroup = "Exif", Name ="Exposure Time", Description = "Exposure time, given in seconds."},
                new TiffTag { TagId = 33437, TagGroup = "Exif", Name ="F Stop", Description = "The F number"},
                new TiffTag { TagId = 34665, TagGroup = "Exif", Name = "Exif IFD", Description ="A pointer to the Exif IFD"},
                new TiffTag { TagId = 34850, TagGroup = "Exif", Name ="Exposure Program", Description = "The class of the program used by the camera to set exposure when the picture is taken."},
                new TiffTag { TagId = 34852, TagGroup = "Exif", Name ="Spectral Sensitivity", Description = "Indicates the spectral sensitivity of each channel of the camera used."},
                new TiffTag { TagId = 34855, TagGroup = "Exif", Name ="ISO Speed Rating", Description = "Indicates the ISO Speed and ISO Latitude of the camera or input device as specified in ISO 12232."},
                new TiffTag { TagId = 34856, TagGroup = "Exif", Name ="Opto-Electric Conversion Factor", Description = "Indicates the Opto-Electric Conversion Function (OECF) specified in ISO 14524."},
                new TiffTag { TagId = 36864, TagGroup = "Exif", Name ="Exif Version", Description = "The version of the supported Exif standard."},
                new TiffTag { TagId = 36867, TagGroup = "Exif", Name ="Original Date And Time", Description = "The date and time when the original image data was generated."},
                new TiffTag { TagId = 36868, TagGroup = "Exif", Name ="Date and Time Digitized", Description = "The date and time when the image was stored as digital data."},
                new TiffTag { TagId = 37121, TagGroup = "Exif", Name ="Components Configuration", Description = "Specific to compressed data; specifies the channels and complements PhotometricInterpretation"},
                new TiffTag { TagId = 37122, TagGroup = "Exif", Name ="Compressed Bits Per Pixel", Description = "Specific to compressed data; states the compressed bits per pixel."},
                new TiffTag { TagId = 37377, TagGroup = "Exif", Name ="Shutter Speed Value", Description = "Shutter speed."},
                new TiffTag { TagId = 37378, TagGroup = "Exif", Name ="Aperture Value", Description = "The lens aperture."},
                new TiffTag { TagId = 37379, TagGroup = "Exif", Name = "Brightness Value", Description = "The value of brightness." },
                new TiffTag { TagId = 37380, TagGroup = "Exif", Name = "Exposure Bias", Description = "The exposure bias." },
                new TiffTag { TagId = 37381, TagGroup = "Exif", Name = "Max Aperture Value", Description = "The smallest F number of the lens." },
                new TiffTag { TagId = 37382, TagGroup = "Exif", Name = "Subject Distance", Description = "The distance to the subject, given in meters." },
                new TiffTag { TagId = 37383, TagGroup = "Exif", Name = "Metering Mode", Description = "The metering mode." },
                new TiffTag { TagId = 37384, TagGroup = "Exif", Name = "Light Source", Description = "The kind of light source." },
                new TiffTag { TagId = 37385, TagGroup = "Exif", Name = "Flash", Description = "Indicates the status of flash when the image was shot." },
                new TiffTag { TagId = 37386, TagGroup = "Exif", Name = "Focal Length", Description = "The actual focal length of the lens, in mm." },
                new TiffTag { TagId = 40963, TagGroup = "Exif", Name = "Pixel Y Dimension", Description = "Specific to compressed data; the valid height of the meaningful image." },
                new TiffTag { TagId = 40962, TagGroup = "Exif", Name = "Pixel X Dimension", Description = "Specific to compressed data; the valid width of the meaningful image." },
                new TiffTag { TagId = 41492, TagGroup = "Exif", Name = "Subject Location", Description = "Indicates the location of the main subject in the scene." },
                new TiffTag { TagId = 41987, TagGroup = "Exif", Name = "White Balance", Description = "Indicates the white balance mode set when the image was shot." },
                new TiffTag { TagId = 42016, TagGroup = "Exif", Name = "Image Unique ID", Description = "Indicates an identifier assigned uniquely to each image." }
            };

        }
     
    }
}
