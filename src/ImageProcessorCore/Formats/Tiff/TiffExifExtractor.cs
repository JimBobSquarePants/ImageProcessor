using System.Collections.Generic;
using System.Linq;

namespace ImageProcessorCore.Formats
{
    /// <summary>
    ///
    /// </summary>
    internal class TiffExifExtractor : ITiffVisitor
    {
        private readonly Dictionary<ushort, TiffTag> _exifTags; 

        public List<ImageProperty> Properties { get; private set; }

        public TiffExifExtractor()
        {
            Properties = new List<ImageProperty>();
            
            // i don't think this is needed as we can use the Visit(TiffDirectory)
            // to determin the exif and gps tags...
            _exifTags = TiffTagRegistry.Instance.Tags
                .Where( i => i.TagGroup == "Exif" ||
                       i.TagGroup == "GPS"
                       // add a few other tags that are not technically exif tags but could be usefull for processing the image
                       || i.TagId == TiffTagRegistry.TiffImageWidth
                       || i.TagId == TiffTagRegistry.TiffImageLength
                       || i.TagId == TiffTagRegistry.TiffXResolution
                       || i.TagId == TiffTagRegistry.TiffYResolution
                       || i.TagId == TiffTagRegistry.TiffResolutionUnit
                       )
                .ToDictionary(i => i.TagId);

        }

        public void Visit(TiffProperty property)
        {
            if (_exifTags.ContainsKey(property.Tag.TagId))
            {
                if (property.Value != null)
                {
                    ImageProperty imageProperty = new ImageProperty(property.Tag.Name, property.Value);
                    Properties.Add(imageProperty);
                }
            }
        }

        public void Visit(IptcProperty property)
        {
            // until the image property can hold more than just a string value....
            Properties.Add( new ImageProperty(property.Tag.Name, property.Value.ToString()));
        }

        public void Visit(TiffDirectory directory)
        {
            // not sure we care about the directories
        }
        
    }
}
