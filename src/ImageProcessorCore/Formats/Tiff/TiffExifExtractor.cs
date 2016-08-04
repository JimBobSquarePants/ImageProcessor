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

        private IList<ImageProperty> _properties;

        public TiffExifExtractor(IList<ImageProperty> properties )
        {
            _properties = properties;
            
            // i don't think this is needed as we can use the Visit(TiffDirectory)
            // to determin the exif and gps tags...
            _exifTags = TiffTagRegistry.Instance.Tags
                .Where( i => i.TagGroup == "Exif" ||
                       i.TagGroup == "GPS"
                       // add a few other tags that are not technically exif tags but could be usefull for processing the image
                       || i.TagId == 271 // need constant
                       || i.TagId == 272 // need constant
                       || i.TagId == 315 // need constant
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
                    ImageProperty imageProperty = new ImageProperty(property.Tag.Name, property.Value.ToString());
                    _properties.Add(imageProperty);
                }
            }
        }

        public void Visit(IptcProperty property)
        {
            // until the image property can hold more than just a string value....
            _properties.Add( new ImageProperty(property.Tag.Name, property.Value.ToString()));
        }

        public void Visit(TiffDirectory directory)
        {
            // not sure we care about the directories
        }
        
    }
}
