using System.Linq;
using ImageProcessorCore.Formats.Tiff.ValueDecoders;

namespace ImageProcessorCore.Formats.Tiff
{
    public class TiffTagValue
    {
        public TiffTag Tag { get; set; }

        public object Value { get; set; }

        public IFDEntryType ValueType { get; set; }

        public static TiffTagValue Create(TiffReader reader, IFDEntry entry)
        {
            var tag = TiffTagRegistry.Instance.Tags.SingleOrDefault(t => t.TagId == entry.TagId) 
                ?? new TiffTag
                {
                    TagId = entry.TagId,
                    TagGroup = "General",
                    Name = "Unknown",
                    Description = "This tag is not in the registry"
                };

            var value = new TiffTagValue{Tag = tag};

            TiffValueDecoderRegistry.Instance.DecodeValue(reader, entry, value);
           
            return value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
