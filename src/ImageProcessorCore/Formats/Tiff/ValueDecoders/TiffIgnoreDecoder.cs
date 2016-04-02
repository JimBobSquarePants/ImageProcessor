using ImageProcessorCore.IO;

namespace ImageProcessorCore.Formats
{
    internal class TiffIgnoreDecoder : ITiffValueDecoder
    {
        public bool DecodeValue(TiffReader reader, TiffProperty value, int count)
        {
            value.Value = null;
            return true;
        }

    }
}
