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
            
            _exifTags = TiffTagRegistry.Instance.Tags
                .Where( i => i.TagGroup == "Exif")
                .ToDictionary(i => i.TagId);

        }

        public void Visit(TiffProperty property)
        {
            if (_exifTags.ContainsKey(property.Tag.TagId))
            {
                // until the image property can hold more than just a string value....
                ImageProperty imageProperty = new ImageProperty(property.Tag.Name, property.Value.ToString());
                Properties.Add(imageProperty);
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
