// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BigEndianBitConverter.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Implementation of EndianBitConverter which converts to/from big-endian
//   byte arrays.
//   <remarks>
//   Adapted from Miscellaneous Utility Library <see href="http://jonskeet.uk/csharp/miscutil/" />
//   This product includes software developed by Jon Skeet and Marc Gravell. Contact <see href="mailto:skeet@pobox.com" />, or see
//   <see href="http://www.pobox.com/~skeet/" />.
//   </remarks>
// </summary>

namespace ImageProcessor.Imaging.Helpers
{
    /// <summary>
    ///   Implementation of EndianBitConverter which converts to/from big-endian
    ///   byte arrays.
    ///   <remarks>
    ///   Adapted from Miscellaneous Utility Library <see href="http://jonskeet.uk/csharp/miscutil/" />
    ///   This product includes software developed by Jon Skeet and Marc Gravell. Contact <see href="mailto:skeet@pobox.com" />, or see
    ///   <see href="http://www.pobox.com/~skeet/" />.
    ///   </remarks>
    /// </summary>
    internal sealed class BigEndianBitConverter : EndianBitConverter
    {
        /// <summary>
        /// Indicates the byte order ("endianness") in which data is converted using this class.
        /// </summary>
        public override Endianness Endianness => Endianness.BigEndian;

        /// <summary>
        /// Indicates the byte order ("endianness") in which data is converted using this class.
        /// </summary>
        /// <remarks>
        /// Different computer architectures store data using different byte orders. "Big-endian"
        /// means the most significant byte is on the left end of a word. "Little-endian" means the 
        /// most significant byte is on the right end of a word.
        /// </remarks>
        /// <returns>true if this converter is little-endian, false otherwise.</returns>
        public override bool IsLittleEndian() => false;

        /// <summary>
        /// Copies the specified number of bytes from value to buffer, starting at index.
        /// </summary>
        /// <param name="value">The value to copy</param>
        /// <param name="bytes">The number of bytes to copy</param>
        /// <param name="buffer">The buffer to copy the bytes into</param>
        /// <param name="index">The index to start at</param>
        protected internal override void CopyBytesImpl(long value, int bytes, byte[] buffer, int index)
        {
            int endOffset = index + bytes - 1;
            for (int i = 0; i < bytes; i++)
            {
                buffer[endOffset - i] = unchecked((byte)(value & 0xff));
                value >>= 8;
            }
        }

        /// <summary>
        /// Returns a value built from the specified number of bytes from the given buffer,
        /// starting at index.
        /// </summary>
        /// <param name="value">The data in byte array format</param>
        /// <param name="startIndex">The first index to use</param>
        /// <param name="bytesToConvert">The number of bytes to use</param>
        /// <returns>The value built from the given bytes</returns>
        protected internal override long FromBytes(byte[] value, int startIndex, int bytesToConvert)
        {
            long ret = 0;
            for (int i = 0; i < bytesToConvert; i++)
            {
                ret = unchecked((ret << 8) | value[startIndex + i]);
            }

            return ret;
        }
    }
}
