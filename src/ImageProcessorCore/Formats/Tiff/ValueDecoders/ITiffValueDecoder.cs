namespace ImageProcessorCore.Formats.Tiff.ValueDecoders
{
    public interface ITiffValueDecoder
    {
   
        bool DecodeValue(TiffReader reader, IFDEntry entry, TiffTagValue value);
    }
}
