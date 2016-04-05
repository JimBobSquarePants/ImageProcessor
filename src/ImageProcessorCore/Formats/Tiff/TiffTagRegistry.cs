using System;
using System.Collections.Generic;

namespace ImageProcessorCore.Formats
{
    /// <summary>
    /// The class maintains a list of well know tiff tags. A good resource for Tiff Tag info
    /// is http://www.awaresystems.be/imaging/tiff/tifftags.html. This site maintains a huge
    /// list of tags with names and descriptions of each tag. 
    /// http://www.cipa.jp/std/documents/e/DC-010-2012_E.pdf gives some context for tags
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
                new TiffTag { TagId = 0, TagGroup = "GPS", Name = "GPS Version", Description = "Indicates the version of GPS Info" },
                new TiffTag { TagId = 1, TagGroup = "GPS", Name = "GPS Latitude Ref", Description = "Indicates whether the latitude is north or south latitude." },
                new TiffTag { TagId = 2, TagGroup = "GPS", Name = "GPS Latitude", Description = "Indicates the latitude." },
                new TiffTag { TagId = 3, TagGroup = "GPS", Name = "GPS Longitude Ref", Description = "Indicates whether the longitude is east or west longitude." },
                new TiffTag { TagId = 4, TagGroup = "GPS", Name = "GPS Longitude", Description = "Indicates the longitude." },
                new TiffTag { TagId = 5, TagGroup = "GPS", Name = "GPS Altitude Ref", Description = "Indicates the altitude used as the reference altitude." },
                new TiffTag { TagId = 6, TagGroup = "GPS", Name = "GPS Altitude", Description = "Indicates the altitude based on the reference in GPS Altitude Ref." },
                new TiffTag { TagId = 7, TagGroup = "GPS", Name = "GPS Time Stamp", Description = "Indicates the time as UTC (Coordinated Universal Time)." },
                new TiffTag { TagId = 8, TagGroup = "GPS", Name = "GPS Satellites", Description = "Indicates the GPS satellites used for measurements." },
                new TiffTag { TagId = 9, TagGroup = "GPS", Name = "GPS Status", Description = "Indicates the status of the GPS receiver when the image is recorded." },
                new TiffTag { TagId = 10, TagGroup = "GPS", Name = "GPS Measure Mode", Description = "Indicates the GPS measurement mode." },
                new TiffTag { TagId = 11, TagGroup = "GPS", Name = "GPS DOP", Description = "Indicates the GPS DOP (data degree of precision)." },
                new TiffTag { TagId = 12, TagGroup = "GPS", Name = "GPS Speed Ref", Description = "Indicates the unit used to express the GPS receiver speed of movement." },
                new TiffTag { TagId = 13, TagGroup = "GPS", Name = "GPS Speed", Description = "Indicates the speed of GPS receiver movement." },
                new TiffTag { TagId = 14, TagGroup = "GPS", Name = "GPS Track Ref", Description = "Indicates the reference for giving the direction of GPS receiver movement." },
                new TiffTag { TagId = 15, TagGroup = "GPS", Name = "GPS Track", Description = "Indicates the direction of GPS receiver movement." },
                new TiffTag { TagId = 16, TagGroup = "GPS", Name = "GPS Img Direction Ref", Description = "Indicates the reference for giving the direction of the image when it is captured." },
                new TiffTag { TagId = 17, TagGroup = "GPS", Name = "GPS Img Direction", Description = "Indicates the direction of the image when it was captured." },
                new TiffTag { TagId = 18, TagGroup = "GPS", Name = "GPS Map Datum", Description = "Indicates the geodetic survey data used by the GPS receiver." },
                new TiffTag { TagId = 19, TagGroup = "GPS", Name = "GPS Dest Latitude Ref", Description = "Indicates whether the latitude of the destination point is north or south latitude." },
                new TiffTag { TagId = 20, TagGroup = "GPS", Name = "GPS Dest Latitude", Description = "Indicates the latitude of the destination point." },
                new TiffTag { TagId = 21, TagGroup = "GPS", Name = "GPS Dest Longitude Ref", Description = "Indicates whether the longitude of the destination point is east or west longitude." },
                new TiffTag { TagId = 22, TagGroup = "GPS", Name = "GPS Dest Longitude", Description = "Indicates the longitude of the destination point." },
                new TiffTag { TagId = 23, TagGroup = "GPS", Name = "GPS Dest Bearing Ref", Description = "Indicates the reference used for giving the bearing to the destination point." },
                new TiffTag { TagId = 24, TagGroup = "GPS", Name = "GPS Dest Bearing", Description = "Indicates the bearing to the destination point." },
                new TiffTag { TagId = 25, TagGroup = "GPS", Name = "GPS Dest Distance Ref", Description = "Indicates the unit used to express the distance to the destination point." },
                new TiffTag { TagId = 26, TagGroup = "GPS", Name = "GPS Dest Distance", Description = "Indicates the distance to the destination point." },
                new TiffTag { TagId = 27, TagGroup = "GPS", Name = "GPS Processing Method", Description = "A character string recording the name of the method used for location finding." },
                new TiffTag { TagId = 28, TagGroup = "GPS", Name = "GPS Area Information", Description = "A character string recording the name of the GPS area." },
                new TiffTag { TagId = 29, TagGroup = "GPS", Name = "GPS Date Stamp", Description = "A character string recording date and time information relative to UTC (Coordinated Universal Time)." },
                new TiffTag { TagId = 30, TagGroup = "GPS", Name = "GPS Differential", Description = "Indicates whether differential correction is applied to the GPS receiver." },

                new TiffTag { TagId = 254, TagGroup = "General", Name = "New Sub File Type", Description = "A general indication of the kind of data contained in this subfile."},
                new TiffTag { TagId = 255, TagGroup = "General", Name = "Sub File Type", Description = "A general indication of the kind of data contained in this subfile."},
                /*256*/ new TiffTag { TagId = TiffImageWidth, TagGroup = "General", Name = "Image Width In Pixels", Description = "The number of columns in the image, i.e., the number of pixels per row."},
                /*257*/ new TiffTag { TagId = TiffImageLength, TagGroup = "General", Name = "Image Length In Pixels", Description = "The number of rows of pixels in the image."},
                /*258*/ new TiffTag { TagId = TiffBitsPerSample, TagGroup = "General", Name = "Bits Per Sample", Description = "Number of bits per component."},
                /*259*/ new TiffTag { TagId = TiffCompression, TagGroup = "General", Name = "Compression Schema", Description = "Compression scheme used on the image data."},
                /*262*/ new TiffTag { TagId = TiffPhotometricInterpretation, TagGroup = "General", Name = "Photometric Interpretation", Description = "The color space of the image data."},
                new TiffTag { TagId = 263, TagGroup = "General", Name = "Thresh Holding", Description = "For black and white TIFF files that represent shades of gray, the technique used to convert from gray to black and white pixels."},
                new TiffTag { TagId = 264, TagGroup = "General", Name = "Cell Width", Description = "The width of the dithering or halftoning matrix used to create a dithered or halftoned bilevel file."},
                new TiffTag { TagId = 265, TagGroup = "General", Name = "Cell Length", Description = "The length of the dithering or halftoning matrix used to create a dithered or halftoned bilevel file."},
                new TiffTag { TagId = 266, TagGroup = "General", Name = "Fill Order", Description = "The logical order of bits within a byte."},
                new TiffTag { TagId = 269, TagGroup = "General", Name = "Document Name", Description = "The name of the document from which this image was scanned."},
                new TiffTag { TagId = 270, TagGroup = "General", Name = "Image Description", Description = "A string that describes the subject of the image."},
                new TiffTag { TagId = 271, TagGroup = "General", Name = "Make", Description = "The scanner manufacturer."},
                new TiffTag { TagId = 272, TagGroup = "General", Name = "Model", Description = "The scanner model name or number."},
                
                /*274*/ new TiffTag { TagId = TiffOrientation, TagGroup = "General", Name = "Orientation", Description = "The orientation of the image with respect to the rows and columns."},
                /*277*/ new TiffTag { TagId = TiffSamplePerPixel, TagGroup = "General", Name = "Samples Per Pixel", Description = "Number of componets per pixel."},

                new TiffTag { TagId = 278, TagGroup = "General", Name = "Rows Per Strip", Description = "The number of rows in a strip of data."},
                new TiffTag { TagId = 279, TagGroup = "General", Name = "Strip Byte Count", Description = "The number of bytes for a strip of data"},
                new TiffTag { TagId = 280, TagGroup = "General", Name = "Minimum Sample Value", Description = "The minimum sample value."},
                new TiffTag { TagId = 281, TagGroup = "General", Name = "Maximum Sample Value", Description = "The maximum sample value."},
                /*282*/new TiffTag { TagId = TiffXResolution, TagGroup = "General", Name = "X Resolution", Description = "The horizontal resolution in pixels per resolution unit."},
                /*283*/new TiffTag { TagId = TiffYResolution, TagGroup = "General", Name = "Y Resolution", Description = "The vertical resolution in pixels per resolution unit."},
                new TiffTag { TagId = 284, TagGroup = "General", Name = "Planer Config", Description = ""},
                new TiffTag { TagId = 285, TagGroup = "General", Name = "Page Name", Description = ""},
                new TiffTag { TagId = 286, TagGroup = "General", Name = "X Position", Description = ""},
                new TiffTag { TagId = 287, TagGroup = "General", Name = "Y Position", Description = ""},
                new TiffTag { TagId = 288, TagGroup = "General", Name = "Free Offsets", Description = ""},
                new TiffTag { TagId = 289, TagGroup = "General", Name = "Free Byte Count", Description = ""},
                /*296*/new TiffTag { TagId = TiffResolutionUnit, TagGroup = "General", Name = "Resolution Unit", Description = "The unit of measurement for XResolution and YResolution." },
                new TiffTag { TagId = 305, TagGroup = "General", Name = "Software", Description = ""},
                new TiffTag { TagId = 306, TagGroup = "General", Name = "Creation Date And Time", Description = "Date and time when the file was last modified (no time zone in Exif), stored in ISO 8601 formant, not in the original Exif format."},
                new TiffTag { TagId = 315, TagGroup = "General", Name = "Artist", Description = ""},
                new TiffTag { TagId = 316, TagGroup = "General", Name = "Computer Created On", Description = ""},
                new TiffTag { TagId = 318, TagGroup = "General", Name = "White Point", Description = "Image white point."},
                new TiffTag { TagId = 320, TagGroup = "General", Name = "Color Map", Description = "RGB map for pallete image"},
                /*330*/ new TiffTag { TagId = TiffSubDirectory, TagGroup = "General", Name = "Sub Image ID", Description = "Sub Image Descriptor"},
                /*513*/ new TiffTag { TagId = 513, TagGroup = "General", Name = "JPEG Interchange Format", Description ="Old-style JPEG compression field. TechNote2 invalidates this part of the specification."},
                /*514*/ new TiffTag { TagId = 514, TagGroup = "General", Name = "JPEG Interchange Format Length", Description ="Old-style JPEG compression field. TechNote2 invalidates this part of the specification."},
                /*530*/ new TiffTag { TagId = TiffYCbCrSubSampling, TagGroup = "General", Name="YCbCr Sub Sampling", Description = "Sample ratio of chrominance components." }, 
                /*531*/ new TiffTag { TagId = TiffYCbCrPositioning, TagGroup = "General", Name = "YCbCrPositioning", Description ="Specifies the positioning of subsampled chrominance components relative to luminance samples."},

                new TiffTag { TagId = 33434, TagGroup = "Exif", Name ="Exposure Time", Description = "Exposure time, given in seconds."},
                new TiffTag { TagId = 33437, TagGroup = "Exif", Name ="F Stop", Description = "The F number"},
                new TiffTag { TagId = TiffIptcDirectory, TagGroup = "IPTC", Name = "IPTC Directory", Description ="IPTC metadata"},
                new TiffTag { TagId = TiffExifDirectory, TagGroup = "Exif", Name = "Exif TiffDirectory", Description ="A pointer to the Exif TiffDirectory"},
                /*34850*/new TiffTag { TagId = TiffExifExposureProgram, TagGroup = "Exif", Name ="Exposure Program", Description = "The class of the program used by the camera to set exposure when the picture is taken."},
                new TiffTag { TagId = 34852, TagGroup = "Exif", Name ="Spectral Sensitivity", Description = "Indicates the spectral sensitivity of each channel of the camera used."},
                /*34853*/ new TiffTag { TagId = TiffGpsDirectory, TagGroup = "Exif", Name ="GPS Group", Description = "A pointer to the Exif-related GPS Info TiffDirectory."},
                new TiffTag { TagId = 34855, TagGroup = "Exif", Name ="ISO Speed Rating", Description = "Indicates the ISO Speed and ISO Latitude of the camera or input device as specified in ISO 12232."},
                /*34864*/ new TiffTag() { TagId = TiffExifSensitivityType, Name="Sensitivity Type", Description="Indicates which one of the parameters of ISO 12232 is used for Photographic Sensitivity."},
                new TiffTag { TagId = 34856, TagGroup = "Exif", Name ="Opto-Electric Conversion Factor", Description = "Indicates the Opto-Electric Conversion Function (OECF) specified in ISO 14524."},
                /*36864*/ new TiffTag { TagId = TiffExifVersion, TagGroup = "Exif", Name ="Exif Version", Description = "The version of the supported Exif standard."},
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

                /*37500*/new TiffTag { TagId = TiffMakerNote,       TagGroup = "Exif", Name = "Maker Note", Description = "Manufacturere specific information." },
                /*37521*/new TiffTag { TagId = SubsecTimeOriginal,  TagGroup = "Exif", Name = "Subsee Time Original", Description = "A tag used to record fractions of seconds for the Date Time Original tag." },
                /*37521*/new TiffTag { TagId = SubsecTimeDigitized, TagGroup = "Exif", Name = "Subsee Time Digitized", Description = "A tag used to record fractions of seconds for the Date Time Digitized tag." },
                /*40960*/new TiffTag { TagId = FlashPixVersion,     TagGroup = "Exif", Name = "Flash Pix", Description = "The Flashpix format version supported by a FPXR file." },
                /*40961*/new TiffTag { TagId = ColorSpace,          TagGroup = "Exif", Name = "Color Space", Description = "The color space information tag is always recorded as the color space specifier." },

                new TiffTag { TagId = 40962, TagGroup = "Exif", Name = "Pixel X Dimension", Description = "Specific to compressed data; the valid width of the meaningful image." },
                new TiffTag { TagId = 40963, TagGroup = "Exif", Name = "Pixel Y Dimension", Description = "Specific to compressed data; the valid height of the meaningful image." },
                new TiffTag { TagId = 41492, TagGroup = "Exif", Name = "Subject Location", Description = "Indicates the location of the main subject in the scene." },
                /*41495*/new TiffTag { TagId = TiffSensingMethod, TagGroup = "Exif", Name = "Sensing Method", Description = "Indicates the image sensor type on the camera or input device." },
                /*41729*/new TiffTag { TagId = TiffSceneType, TagGroup = "Exif", Name = "Scene Type", Description = "Indicates the tpe of scene." },

               
                /*41985*/new TiffTag { TagId = TiffCustomRendered, TagGroup = "Exif", Name = "Custom Rendered", Description = "Inidicates the use of special processing on image data, such as rendering geared to output." },
                /*41986*/new TiffTag { TagId = TiffExposureMode, TagGroup = "Exif", Name = "Exposure Mode", Description = "Indicates the exposure mode set when the image was shot." },
                /*41987*/new TiffTag { TagId = TiffWhiteBalance, TagGroup = "Exif", Name = "White Balance", Description = "Indicates the white balance mode set when the image was shot." },
                /*41989*/new TiffTag { TagId = TiffFocalLengthIn35MmFilm, TagGroup = "Exif", Name = "Focal Length In 35mm Film", Description = "Indicates the equivalent focal length assuming a 35mm filem camera, in mm." },
                /*41990*/new TiffTag { TagId = TiffSceneCaptureType, TagGroup = "Exif", Name = "Scene Capture Type", Description = "Indicates the type of scene that was shot." },
                /*42034*/new TiffTag { TagId = TiffLensSpecification, TagGroup = "Exif", Name = "Lens Specification", Description = "Minimum focal length, maximum focal length of lens that was used in photography." },
                /*42035*/new TiffTag { TagId = TiffLensMake, TagGroup = "Exif", Name = "Lens Make", Description = "The lens manufacturer." },
                /*42036*/new TiffTag { TagId = TiffLensModel, TagGroup = "Exif", Name = "Lens Model", Description = "The lens model name and model number." },



                new TiffTag { TagId = 42016, TagGroup = "Exif", Name = "Image Unique ID", Description = "Indicates an identifier assigned uniquely to each image." }
            };

        }

        /// <summary>
        /// Image Width in Pixels
        /// </summary>
        public const ushort TiffImageWidth = 256;                        // 0x100;

        /// <summary>
        /// Image Height in Pixels
        /// </summary>
        public const ushort TiffImageLength = 257;                       // 0x101;
        
        /// <summary>
        /// Number of bits per component in each channel.
        /// <remarks>
        /// The value of this tag will be an ordered array of integers.
        /// </remarks>
        /// </summary>
        public const ushort TiffBitsPerSample = 258;                     // 0x102

        /// <summary>
        /// The compression scheme for the image in the current Tiff Directory
        /// <remarks>
        /// This is a closed choice of integers. Each integer value represents a compression scheme type.
        /// 1 - Uncompressed
        /// 5 - LZW
        /// 6 - JPEG
        /// </remarks>
        /// </summary>
        public const ushort TiffCompression = 259;                       // 0x103
        
        /// <summary>
        /// Pixel Composition
        /// <remarks>
        /// The value of this tag is a closed choice of integers. 
        /// 2 - RGB
        /// 6 = YCbCr
        /// </remarks>
        /// </summary>
        public const ushort TiffPhotometricInterpretation = 262;          // 0x106

        /// <summary>
        /// How to translate value of the TiffOrientation tag.....
        /// http://www.cipa.jp/std/documents/e/DC-010-2012_E.pdf
        /// 1 = 0th row at top, Oth column at left
        /// 2 = 0th row at top, 0th column at right
        /// 3 = 0th row at bottom, 0th column at right
        /// 4 = 0th row at bottom, 0th row at left
        /// 5 = 0th row at left, oth column at top
        /// 6 = 0th row at right, 0th column at top
        /// 7 = 0th row at right, 0th column at bottom
        /// 8 = 0th row at left, 0th column at bottom
        /// </summary>
        public const ushort TiffOrientation = 274;                   // 0x112

        /// <summary>
        /// Number of components per pixel
        /// <remarks>
        /// The value of this tag is an iteger that represents the number of componets per pixel.
        /// </remarks>
        /// </summary>
        public const ushort TiffSamplePerPixel = 277;                // 0x115

        /// <summary>
        /// Horizontal resolution in pixels per <see cref="TiffResolutionUnit"/>.
        /// <remarks>
        /// The value of this tag is a rational that crepresents the horizontal resolution in
        /// pixels per resolution unit.
        /// </remarks>
        /// </summary>
        public const ushort TiffXResolution = 282;                   // 0x11A

        /// <summary>
        /// Vertical resolution in pixels per <see cref="TiffResolutionUnit"/>.
        /// <remarks>
        /// the value of this tag is a rational that represents the vertical resolution in
        /// pixels per resolution unit.
        /// </remarks>
        /// </summary>
        public const ushort TiffYResolution = 283;                   // 0x11B

        /// <summary>
        /// The data layout of the current tiff directory
        /// <remarks>
        /// The value of this tag is a closed choice of integers.
        /// 1 = chunky
        /// 2 = planar
        /// </remarks>
        /// </summary>
        public const ushort TiffPlanarConfiguration = 284;           // 0x11C

        /// <summary>
        /// Sampling ration of chrominance components.
        /// <remarks>
        /// The value of this tag is a closed choice of ratio's. Which are represented
        /// as an array of integers.
        /// [2/1] = YCbcr4:2:2
        /// [2/2] = YCbCr4:2:0
        /// </remarks>
        /// </summary>
        public const ushort TiffYCbCrSubSampling = 530;              // 0x212

        /// <summary>
        /// Unit used for <see cref="TiffXResolution"/> and <see cref="TiffYResolution"/>.
        /// <remarks>
        /// The value of this tag is a closed choice of integers.
        /// 2 = inches
        /// 3 = centimeters
        /// </remarks>
        /// </summary>
        public const ushort TiffResolutionUnit = 296;                // 0x296

        /// <summary>
        /// Position of chrominance vs. Luminance
        /// <remarks>
        /// The value of this tag is a closed choice of integers.
        /// 1 = centered
        /// 2 = co-sited
        /// </remarks>
        /// </summary>
        public const ushort TiffYCbCrPositioning = 531;              // 0x213

        /// <summary>
        /// Exif version number.
        /// <remarks>
        /// The value of this tag is a string. 
        /// Version 2.3 is written as "0230"
        /// </remarks>
        /// </summary>
        public const ushort TiffExifVersion = 36864;                 // 0x9000

        /// <summary>
        /// Class of program used for exposure
        /// <remarks>
        /// The value of this tag is a closed choice of integers
        /// 0 - Not Defined
        /// 1 - Manual
        /// 2 - Normal Program
        /// 3 - Aperture Priority
        /// 4 - Shutter Priority
        /// 5 - Creative Program
        /// 6 - Action Program
        /// 7 - Portrait Mode
        /// 8 - Landscape Mode
        /// </remarks>
        /// </summary>
        public const ushort TiffExifExposureProgram = 34850;         // 0x829A

        /// <summary>
        /// Indicates which one of the parameters of ISO 12232 is used for Photographic Sensitivity
        /// <remarks>
        /// The value of this tag is a closed choice of integers
        /// 0 = Unknown
        /// 1 = Standard Output Sensitiviety (SOS)
        /// 2 = Recommended Exposure Index (REI)
        /// 3 - ISO Speed
        /// 4 - Standard Output Sensitivity (SOS and Recommended Exposure Index (REI)
        /// 5 - Standard Output sensitivity (SOS) and ISO Speed
        /// 6 - Recommended Exposure Index (REI) and ISO Speed
        /// 7 - Standard Output Sensitivity (SOS) and Recommended Exposure Index (REI) and ISO Speed
        /// </remarks>
        /// </summary>
        public const ushort TiffExifSensitivityType = 34864;         // 0x34864

        public const ushort TiffSubDirectory = 330;
        public const ushort TiffJpegInterchangeFormat = 513;
        public const ushort TiffJpegInterchangeFormatLength = 514;
        public const ushort TiffExifDirectory = 34665;
        public const ushort TiffGpsDirectory = 34853;
        public const ushort TiffIptcDirectory = 33723;
        public const ushort TiffMakerNote = 37500;                   // 0x027C
        public const ushort SubsecTimeOriginal = 37521;              // 0x9291
        public const ushort SubsecTimeDigitized = 37522;             // 0x9292
        public const ushort FlashPixVersion = 40960;                 // 0xA000
        public const ushort ColorSpace = 40961;                      // 0xA001
        public const ushort TiffSensingMethod = 41495;               // 0xA217
        public const ushort TiffSceneType = 41729;                   // 0xA301
        public const ushort TiffCustomRendered = 41985;              // 0xA401
        public const ushort TiffExposureMode = 41986;                // 0xA402
        public const ushort TiffWhiteBalance = 41987;                // 0xA403
        public const ushort TiffFocalLengthIn35MmFilm = 41989;       // 0xA405
        public const ushort TiffSceneCaptureType = 41990;            // 0xA406
        public const ushort TiffLensSpecification = 42034;           // 0xA432 - http://clanmills.com/exiv2/tags-xmp-exifEX.html   // An ordered array of rational
        public const ushort TiffLensMake = 42035;                    // 0xA433 - http://clanmills.com/exiv2/tags-xmp-exifEX.html   
        public const ushort TiffLensModel = 42036;                   // 0xA434 - http://clanmills.com/exiv2/tags-xmp-exifEX.html

        // unsure what this is yet. the value data for this tag is large
        // and probably a some other data stucture i think it might be comming from light room????
        // https://feedback.photoshop.com/photoshop_family/topics/does_lr5_3_have_setting_to_prevent_exporting_tiff_images_with_tag_59932
        public const ushort TiffNotSureWhatThisIsYetIgnoreIt = 59932; // also 59933....

    }
}
