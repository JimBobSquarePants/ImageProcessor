using System.Linq;

namespace ImageProcessorCore.Formats
{
    public class TiffTagValue
    {
        public TiffTag Tag { get; set; }

        public object Value { get; set; }

        public IFDEntryType ValueType { get; set; }

        public static TiffTagValue Create(TiffReader reader, IFDEntry entry)
        {
            TiffTag tag = TiffTagRegistry.Instance.Tags.SingleOrDefault(t => t.TagId == entry.TagId) 
                ?? new TiffTag
                {
                    TagId = entry.TagId,
                    TagGroup = "General",
                    Name = "Unknown",
                    Description = "This tag is not in the registry"
                };

            TiffTagValue value = new TiffTagValue{Tag = tag};

            TiffValueDecoderRegistry.Instance.DecodeValue(reader, entry, value);
           
            return value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
