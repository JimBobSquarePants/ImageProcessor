using System.Collections.Generic;
using System.IO;

namespace ImageProcessorCore.Formats
{
    /// <summary>
    /// Decodes a stream of bytes according to the TIFF 6.0 specification
    /// http://partners.adobe.com/public/developer/en/tiff/TIFF6.pdf
    /// </summary>
    internal class TiffDecoderCore
    {
        private readonly TiffReader _tiffReader;
        private readonly List<IFD> _directories;

        private TiffDecoderCore(TiffReader reader)
        {
            _tiffReader = reader;
            _directories = new List<IFD>();
        }

        /// <summary>
        /// Checks the stream to see if its sitting at a valid TIFF stream.
        /// </summary>
        /// <param name="stream">The stream to check for a valid TIFF image.</param>
        /// <returns>True if the stream is sitting at a valid Tiff image; False otherwise.</returns>
        public static TiffDecoderCore Create(Stream stream)
        {
            TiffReader reader = stream.ToTiffReader();
            if (null == reader)
            {
                // not a valid tiff stream.
                return null;
            }

            return new TiffDecoderCore(reader);

        }

        /// <summary>
        /// Decodes the given stream into a TIFF
        /// </summary>
        /// <remarks>
        /// To read a TIFF, the stream must be seekable.
        /// </remarks>
        /// <exception cref="IOException"></exception>
        public void Decode()
        {
            
            // The tiff reader should be sitting at the first offset that contains the
            // location of the first directory.
            int nextOffset = _tiffReader.ReadInt32();

            // Keep reading directories until there are no more
            do
            {
                // move to the directory location in the file.
                _tiffReader.Seek(nextOffset, SeekOrigin.Begin);

                // process the directory
                IFD directory = new IFD(_tiffReader);
                _directories.Add(directory);

                // get the next directory location in the file if we are at the 
                // last directory in the file, the offset will be 0
                nextOffset = _tiffReader.ReadInt32();

            } while (nextOffset != 0);
            
        }

    }
}
