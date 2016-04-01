namespace ImageProcessorCore.Formats
{
    internal class TiffIgnoreValueDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, TiffTagValue value, int count)
        {
            value.Value = null;
            return true;
        }

    }
}
