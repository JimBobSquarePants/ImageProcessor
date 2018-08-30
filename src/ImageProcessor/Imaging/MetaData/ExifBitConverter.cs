// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExifBitConverter.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The exif bit converter. Converts based on the endianness of the current machine.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.MetaData
{
    using System;
    using System.Text;

    using ImageProcessor.Imaging.Helpers;

    /// <summary>
    /// The exif bit converter. Converts based on the endianness of the current machine.
    /// </summary>
    internal sealed class ExifBitConverter : EndianBitConverter
    {
        /// <summary>
        /// The computer architecture info.
        /// </summary>
        private readonly IComputerArchitectureInfo computerArchitectureInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExifBitConverter"/> class.
        /// </summary>
        /// <param name="computerArchitectureInfo">
        /// The computer architecture info.
        /// </param>
        public ExifBitConverter(IComputerArchitectureInfo computerArchitectureInfo) => this.computerArchitectureInfo = computerArchitectureInfo;

        /// <summary>
        /// Indicates the byte order ("endianness") in which data is converted using this class.
        /// </summary>
        public override Endianness Endianness => this.IsLittleEndian() ? Endianness.LittleEndian : Endianness.BigEndian;

        /// <summary>
        /// Indicates the byte order ("endianness") in which data is converted using this class.
        /// </summary>
        /// <remarks>
        /// Different computer architectures store data using different byte orders. "Big-endian"
        /// means the most significant byte is on the left end of a word. "Little-endian" means the 
        /// most significant byte is on the right end of a word.
        /// </remarks>
        /// <returns>true if this converter is little-endian, false otherwise.</returns>
        public override bool IsLittleEndian() => this.computerArchitectureInfo.IsLittleEndian();

        /// <summary>
        /// Converts the given ascii string to an array of bytes optionally adding a null terminator.
        /// </summary>
        /// <param name="value">The string to convert</param>
        /// <param name="addnull">Whether to add a null terminator to the end of the string.</param>
        /// <returns>
        /// The <see cref="T:byte[]"/>.
        /// </returns>
        public byte[] GetBytes(string value, bool addnull)
        {
            if (addnull)
            {
                value += '\0';
            }

            byte[] bytes = Encoding.ASCII.GetBytes(value);

            if (!this.IsLittleEndian())
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        /// Converts the given ascii string to an array of bytes without adding a null terminator.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>
        /// The <see cref="T:byte[]"/>.
        /// </returns>
        public byte[] GetBytes(string value) => this.GetBytes(value, false);

        /// <summary>
        /// Converts the given unsigned rational number to an array of bytes.
        /// </summary>
        /// <param name="value">
        /// The <see cref="T:Rational{int}"/> containing the numerator and denominator.
        /// </param>
        /// <returns>
        /// The <see cref="T:byte[]"/>.
        /// </returns>
        public byte[] GetBytes(Rational<uint> value)
        {
            byte[] num = this.GetBytes(value.Numerator);
            byte[] den = this.GetBytes(value.Denominator);
            byte[] data = new byte[8];
            Array.Copy(num, 0, data, 0, 4);
            Array.Copy(den, 0, data, 4, 4);
            return data;
        }

        /// <summary>
        /// Converts the given signed rational number to an array of bytes.
        /// </summary>
        /// <param name="value">
        /// The <see cref="T:Rational{int}"/> containing the numerator and denominator.
        /// </param>
        /// <returns>
        /// The <see cref="T:byte[]"/>.
        /// </returns>
        public byte[] GetBytes(Rational<int> value)
        {
            byte[] num = this.GetBytes(value.Numerator);
            byte[] den = this.GetBytes(value.Denominator);
            byte[] data = new byte[8];
            Array.Copy(num, 0, data, 0, 4);
            Array.Copy(den, 0, data, 4, 4);
            return data;
        }

        /// <summary>
        /// Convert the given number of bytes from the given array, from the given start
        /// position, into a long, using the bytes as the least significant part of the long.
        /// By the time this is called, the arguments have been checked for validity.
        /// </summary>
        /// <param name="value">The bytes to convert</param>
        /// <param name="startIndex">The index of the first byte to convert</param>
        /// <param name="bytesToConvert">The number of bytes to use in the conversion</param>
        /// <returns>The converted number</returns>
        protected internal override long FromBytes(byte[] value, int startIndex, int bytesToConvert)
        {
            if (this.IsLittleEndian())
            {
                return Little.FromBytes(value, startIndex, bytesToConvert);
            }

            return Big.FromBytes(value, startIndex, bytesToConvert);
        }

        /// <summary>
        /// Copies the given number of bytes from the least-specific
        /// end of the specified value into the specified byte array, beginning
        /// at the specified index.
        /// This must be implemented in concrete derived classes, but the implementation
        /// may assume that the value will fit into the buffer.
        /// </summary>
        /// <param name="value">The value to copy bytes for</param>
        /// <param name="bytes">The number of significant bytes to copy</param>
        /// <param name="buffer">The byte array to copy the bytes into</param>
        /// <param name="index">The first index into the array to copy the bytes into</param>
        protected internal override void CopyBytesImpl(long value, int bytes, byte[] buffer, int index)
        {
            if (this.IsLittleEndian())
            {
                Little.CopyBytesImpl(value, bytes, buffer, index);
            }
            else
            {
                Big.CopyBytesImpl(value, bytes, buffer, index);
            }
        }
    }
}
