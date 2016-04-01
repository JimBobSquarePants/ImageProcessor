using System;
using System.IO;
using System.Text;
using ImageProcessorCore.IO;

namespace ImageProcessorCore.Formats
{
    /// <summary>
    /// Encapsulates a seekable stream of TIFF bytes.
    /// <remarks>
    /// Thre <see cref="TiffReader"/> class requires the stream to be
    /// positioned at the start of the Tiff image. 
    /// </remarks>
    /// </summary>
    internal class TiffReader : EndianBinaryReader
    {
        private readonly Stream _stream;
        private readonly TiffReaderSnapshot _begin;

        /// <summary>
        /// Initializes a new instance of the <see cref="TiffReader"/> class 
        /// with the given bit converter, reading from the stream using UTF-8 encoding.
        /// </summary>
        /// <param name="byteConverter">Converter to use when writing data. This should be obtained
        /// using the <see cref="TiffEndianByteOrder"/> stream extension class.</param>
        /// <param name="stream">Stream to read data from</param>
        public TiffReader(Stream stream, EndianBitConverter byteConverter )
            : base(byteConverter, stream)
        {

            // parsing a tiff stream requires the stream to be seekable.
            if (!stream.CanSeek)
            {
                throw new ArgumentException("Stream must be capable of seeking.", nameof(stream));
            }

            _stream = stream;

            // because we support parsing a tiff image from in the middle of a jpeg image
            // we need to remember the start of the tiff image in the underlining stream.
            _begin = Snapshot();
            _begin.Position -= 2; // account for the 2 bytes of bit order that was read earlier

        }


        /// <summary>
        /// Seeks within the stream.
        /// </summary>
        /// <param name="offset">Offset to seek to.</param>
        /// <param name="origin">Origin of seek operation.</param>
        public override void Seek(int offset, SeekOrigin origin)
        {
            // The begining of the tiff file might be an offset into a different image file
            // in which case seeking to the beginning would be very bad.
            if (origin == SeekOrigin.Begin)
            {
                base.Seek( _begin.Position + offset, SeekOrigin.Begin);
                return;
            }

            base.Seek(offset, origin);
        }

        /// <summary>
        /// Takes a snapshot of the stream and any extra data the caller
        /// would like to remember.
        /// </summary>
        /// <param name="context">Any data you would like to remember from this point.</param>
        /// <example>
        /// 
        /// var snapshot = reader.Snapshot( new { 
        ///                                   name = "I'm a silly monkey."
        ///                                 });
        /// </example>
        /// <returns><see cref="TiffReaderSnapshot"/></returns>
        public TiffReaderSnapshot Snapshot(object context = null)
        {
            return new TiffReaderSnapshot {Position = (int)_stream.Position, Context = context};
        }

        /// <summary>
        /// Applys the snapshot to the stream as it was when the caller took
        /// the snapshot.
        /// </summary>
        /// <param name="snapshot">The <see cref="TiffReaderSnapshot"/> returned from a call to <see cref="Snapshot"/></param>
        /// <returns>The context object supplied to the <see cref="Snapshot"/></returns>
        public object Remember(TiffReaderSnapshot snapshot)
        {
            _stream.Seek(snapshot.Position, SeekOrigin.Begin);

            return snapshot.Context;
        }
        
    }
}
