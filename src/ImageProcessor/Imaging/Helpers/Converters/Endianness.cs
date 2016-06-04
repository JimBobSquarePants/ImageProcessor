// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Endianness.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Endianness of a converter
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging.Helpers
{
    /// <summary>
    /// Endianness of a converter
    /// </summary>
    internal enum Endianness
    {
        /// <summary>
        /// Little endian - least significant byte first
        /// </summary>
        LittleEndian,

        /// <summary>
        /// Big endian - most significant byte first
        /// </summary>
        BigEndian
    }
}
