namespace ImageProcessorCore.Formats
{
    public class TiffIgnoreValueDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, IFDEntry entry, TiffTagValue value)
        {
            value.Value = null;
            return true;
        }

    }
}
