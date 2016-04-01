namespace ImageProcessorCore.Formats
{
    internal interface ITiffValueDecoder
    {

        bool DecodeValue(TiffReader reader, TiffTagValue value, int count);
    }
}
