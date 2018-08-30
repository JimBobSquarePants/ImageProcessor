// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Int32Converter.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides a way to convert integers to an array of bytes without creating multiple
//   short term arrays.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.MetaData
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// Provides a way to convert integers to an array of bytes without creating multiple
    /// short term arrays.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct Int32Converter
    {
        /// <summary>
        /// The value of the byte array as an integer.
        /// </summary>
        [FieldOffset(0)]
        public readonly int Value;

        /// <summary>
        /// The first byte.
        /// </summary>
        [FieldOffset(0)]
        public readonly byte Byte1;

        /// <summary>
        /// The second byte.
        /// </summary>
        [FieldOffset(1)]
        public readonly byte Byte2;

        /// <summary>
        /// The third byte.
        /// </summary>
        [FieldOffset(2)]
        public readonly byte Byte3;

        /// <summary>
        /// The fourth byte.
        /// </summary>
        [FieldOffset(3)]
        public readonly byte Byte4;

        /// <summary>
        /// Initializes a new instance of the <see cref="Int32Converter"/> struct.
        /// </summary>
        /// <param name="value">
        /// The value to convert from.
        /// </param>
        public Int32Converter(int value)
            : this() => this.Value = value;

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="Int32Converter"/> to a 
        /// <see cref="int"/>.
        /// </summary>
        /// <param name="value">
        /// The instance of <see cref="Int32Converter"/> to convert.
        /// </param>
        /// <returns>
        /// An instance of <see cref="int"/>.
        /// </returns>
        public static implicit operator int(Int32Converter value) => value.Value;

        /// <summary>
        /// Allows the implicit conversion of an instance of <see cref="int"/> to a 
        /// <see cref="Int32Converter"/>.
        /// </summary>
        /// <param name="value">
        /// The instance of <see cref="int"/> to convert.
        /// </param>
        /// <returns>
        /// An instance of <see cref="Int32Converter"/>.
        /// </returns>
        public static implicit operator Int32Converter(int value) => new Int32Converter(value);
    }
}
