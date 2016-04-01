using System.Collections.Generic;
using System.IO;

namespace ImageProcessorCore.Formats
{
    internal class TiffDecoderCore
    {
        private TiffReader _tiffReader;
        private TiffHeader _header;
        private List<IFD> _directories;

        /// <summary>
        /// Checks the stream to see if its sitting at a valid TIFF stream.
        /// </summary>
        /// <param name="stream">The stream to check for a valid TIFF image.</param>
        /// <returns>True if the stream is sitting at a valid Tiff image; False otherwise.</returns>
        public static bool IsValidTiff(Stream stream)
        {
            TiffReader reader = new TiffReader(stream);
            return TiffHeader.IsValidHeader(reader);
        }

        /// <summary>
        /// Decodes the given stream into a TIFF
        /// </summary>
        /// <remarks>
        /// To read a TIFF, the stream must be seekable.
        /// </remarks>
        /// <exception cref="IOException"></exception>
        /// <param name="stream">The stream that contains the TIFF Image</param>
        public void Decode(Stream stream)
        {
            _tiffReader = new TiffReader(stream);
            _header = new TiffHeader(_tiffReader);
               
            _directories = new List<IFD>();
            int nextOffset = _header.Offset;
            do
            {
                _tiffReader.Seek(nextOffset);

                IFD directory = new IFD(_tiffReader);
                _directories.Add(directory);


                nextOffset = _tiffReader.GetInt32();

            } while (nextOffset != 0);
            
        }

    }
}
