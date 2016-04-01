using System.IO;

namespace ImageProcessorCore.Formats
{
    public class TiffHeader
    {
        private const int TiffIdentifier = 42;

        public BitOrder BitOrder { get; }
        public int Offset { get; private set; }
        
        public TiffHeader(TiffReader reader)
        {
            // the first 2 bytes in the header must be the bit order.
            byte low = reader.GetByte();
            byte high = reader.GetByte();

            if (low == (byte) BitOrderMask.LittleEndianLow && high == (byte) BitOrderMask.LittleEndianHigh)
            {
                BitOrder = BitOrder.LittleEndian;
            }
            else if (low == (byte) BitOrderMask.BigEndianLow && high == (byte) BitOrderMask.BigEndianHigh)
            {
                BitOrder = BitOrder.BigEndian;
            }
            else
            {
                throw new IOException("Invalid tiff format.");
            }

            // tell the reader to use this bit order going forward
            reader.BitOrder = BitOrder;

            // the next 2 bytes must be a tiff header marker. (42)
            short tiffIdentifier = reader.GetInt16();
            if (tiffIdentifier != 42)
            {
                throw new IOException("Invalid tiff identifier.");
            }

            // the next 4 bytes (int) will be an offset from the beginning
            // of the stream to the first IDF record
            Offset = reader.GetInt32();

        }
       
    }
}
