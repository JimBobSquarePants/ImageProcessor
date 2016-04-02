using ImageProcessorCore.IO;

namespace ImageProcessorCore.Formats
{
    internal interface ITiffValueDecoder
    {
        bool DecodeValue(TiffReader reader, TiffProperty value, int count);
    }
}
