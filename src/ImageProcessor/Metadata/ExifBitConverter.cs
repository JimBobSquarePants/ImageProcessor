// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Text;

namespace ImageProcessor.Metadata
{
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

        /// <inheritdoc/>
        public override Endianness Endianness => this.IsLittleEndian() ? Endianness.LittleEndian : Endianness.BigEndian;

        /// <inheritdoc/>
        public override bool IsLittleEndian() => this.computerArchitectureInfo.IsLittleEndian();

        /// <summary>
        /// Converts the given ascii string to an array of bytes optionally adding a null terminator.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="addTerminator">Whether to add a null terminator to the end of the string.</param>
        /// <returns>The <see cref="T:byte[]"/>.</returns>
        public byte[] GetBytes(string value, bool addTerminator)
        {
            if (addTerminator)
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
        /// <returns>The <see cref="T:byte[]"/>.</returns>
        public byte[] GetBytes(string value) => this.GetBytes(value, false);

        /// <summary>
        /// Converts the given unsigned rational number to an array of bytes.
        /// </summary>
        /// <param name="value">
        /// The <see cref="T:Rational{int}"/> containing the numerator and denominator.
        /// </param>
        /// <returns>The <see cref="T:byte[]"/>.</returns>
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
        /// <returns>The <see cref="T:byte[]"/>.</returns>
        public byte[] GetBytes(Rational<int> value)
        {
            byte[] num = this.GetBytes(value.Numerator);
            byte[] den = this.GetBytes(value.Denominator);
            byte[] data = new byte[8];
            Array.Copy(num, 0, data, 0, 4);
            Array.Copy(den, 0, data, 4, 4);
            return data;
        }

        /// <inheritdoc/>
        protected internal override long FromBytes(byte[] value, int startIndex, int bytesToConvert)
        {
            if (this.IsLittleEndian())
            {
                return Little.FromBytes(value, startIndex, bytesToConvert);
            }

            return Big.FromBytes(value, startIndex, bytesToConvert);
        }

        /// <inheritdoc/>
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
