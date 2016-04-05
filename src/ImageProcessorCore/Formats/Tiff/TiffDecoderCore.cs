using System;
using System.Collections.Generic;
using System.IO;

namespace ImageProcessorCore.Formats
{
    /// <summary>
    /// Decodes a stream of bytes according to the TIFF 6.0 specification
    /// http://partners.adobe.com/public/developer/en/tiff/TIFF6.pdf
    /// </summary>
    internal class TiffDecoderCore : IDisposable
    {
        private readonly TiffReader _reader;
        private readonly List<TiffDirectory> _directories;

        private TiffDecoderCore(TiffReader reader)
        {
            _reader = reader;
            _directories = new List<TiffDirectory>();
        }

        public List<TiffDirectory> Directories => _directories;

        /// <summary>
        /// Checks the stream to see if its sitting at a valid TIFF stream.
        /// </summary>
        /// <param name="stream">The stream to check for a valid TIFF image.</param>
        /// <returns>Returns a <see cref="TiffDecoderCore"/> if the stream is at a valid tiff image. Null otherwise.</returns>
        public static TiffDecoderCore Create(Stream stream)
        {
            TiffReader reader = stream.ToBinaryReaderFromTiffStream();
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
            
            // The reader should be sitting at the first offset that contains the
            // location of the first directory.
            int nextOffset = _reader.ReadInt32();

            // Keep reading directories until there are no more
            do
            {
                // move to the directory location in the file.
                _reader.Seek(nextOffset, SeekOrigin.Begin);

                // process the directory
                TiffDirectory directory = new TiffDirectory(_reader);
                _directories.Add(directory);

                // get the next directory location in the file. If we are at the 
                // last directory in the file, the offset will be 0
                nextOffset = _reader.ReadInt32();

            } while (nextOffset != 0);
            
        }

        /// <summary>
        /// Fills a list with exif properties after the decoder has decoded
        /// the tiff file.
        /// </summary>
        /// <param name="properties"></param>
        public void FillExifProperties( IList<ImageProperty> properties)
        {
            TiffExifExtractor extractor = new TiffExifExtractor(properties);

            foreach (var directory in Directories)
            {
                directory.Accept(extractor);
            }

        } 

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}
