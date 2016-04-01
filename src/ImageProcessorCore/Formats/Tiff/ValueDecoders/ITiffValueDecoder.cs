namespace ImageProcessorCore.Formats
{
    public interface ITiffValueDecoder
    {
   
        bool DecodeValue(TiffReader reader, IFDEntry entry, TiffTagValue value);
    }
}
