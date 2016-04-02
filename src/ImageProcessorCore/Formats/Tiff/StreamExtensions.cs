using System;
using System.IO;
using ImageProcessorCore.IO;

namespace ImageProcessorCore.Formats
{   
    internal static class StreamExtensions
    {
        // using const here instead of an enum to avoid casting to an enum to check the values
        private const byte LittleEndian = 0x49; // "I"
        private const byte BigEndian = 0x4D;    // "M"

        /// <summary>
        /// This is a well known constant defined in the spec, used to check for a valid tiff file.
        /// </summary>
        private const short TiffHeaderId = 42;

        /// <summary>
        /// The first two bytes in a TIFF file describes the byte order used to encode / decode 
        /// the actual bytes in the file. Legal values are "II" (0x4949) or "MM" (0x4D4D)
        /// <remarks>
        /// The "II" format, byte order is always from the least significat byte to the most significat byte, for both
        /// 16-bit and 32-bit integers. This is called little-endian byte order. aka (Intel)
        /// The "MM" format, byte order is always from most significat to least significat, for both 16-bit and 32-bit
        /// integers. This is called big-endian byte order. aka (Motorolla)
        /// </remarks>
        /// </summary>
        /// <param name="stream">The stream that contains the Tiff image. The stream must
        /// be at the beginning (byte 0) of the Tiff image.</param>
        /// <returns><see cref="EndianBitConverter"/> that can be used to read the stream.</returns>
        public static EndianBitConverter TiffBitConverter(this Stream stream)
        {
            var high = stream.ReadByte();
            var low = stream.ReadByte();

            if (high == LittleEndian && low == LittleEndian)
                return EndianBitConverter.Little;

            if (high == BigEndian && low == BigEndian)
                return EndianBitConverter.Big;

            // not a valid tiff stream ( or we are not at the start of the tiff stream )
            return null;
        }

        /// <summary>
        /// Builds a <see cref="EndianBinaryReader"/> from a stream.
        /// This method should not throw an exception if the stream is not located at
        /// a valid tiff location.
        /// </summary>
        /// <param name="stream">The stream you want to create the reader from.</param>
        /// <returns>
        /// <see cref="EndianBinaryReader"/> or null if the stream is not positioned at 
        /// a valid tiff image. If the stream is not a valid Tiff image, the current
        /// position in the stream will not change.
        /// </returns>
        public static TiffReader ToBinaryReaderFromTiffStream(this Stream stream)
        {
            // remember our position. We cannot assume beginning of file because
            // tiff images can be a segment in another file with an offset that does
            // not start at the beginning of the file.
            int streamPosistion = (int) stream.Position;

            try
            {
                EndianBitConverter byteConverter = stream.TiffBitConverter();
                if (null == byteConverter)
                    throw new IOException("Failed to identify the byte order.");

                EndianBinaryReader reader = new EndianBinaryReader(byteConverter, stream);
                if (TiffHeaderId != reader.ReadInt16()) 
                    throw new IOException("Failed to read the tiff header id.");

                return new TiffReader(reader, streamPosistion);
            }
            catch (Exception)
            {
                // this is not a valid tiff file. Make sure we go back to
                // our original position.
                stream.Seek(streamPosistion, SeekOrigin.Begin);
                return null;
            }
           
        }

    }

}
