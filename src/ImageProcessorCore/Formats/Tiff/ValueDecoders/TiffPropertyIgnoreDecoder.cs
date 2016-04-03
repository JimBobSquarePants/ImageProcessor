using ImageProcessorCore.IO;

namespace ImageProcessorCore.Formats
{
    /// <summary>
    /// This is the last decoder in the chain of decoders. It simple ignores the property value
    /// by setting it to null.
    /// </summary>
    internal class TiffPropertyIgnoreDecoder : ITiffPropertyDecoder
    {
        public bool Decode(TiffReader reader, TiffProperty property, int count)
        {
            property.Value = null;
            return true;
        }

    }
}
