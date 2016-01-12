// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComputerArchitectureInfo.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates methods that provide information about the current computer architecture.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging
{
    using System;

    /// <summary>
    /// Encapsulates methods that provide information about the current computer architecture.
    /// </summary>
    public class ComputerArchitectureInfo : IComputerArchitectureInfo
    {
        /// <summary>
        /// Returns a value indicating whether the current computer archtecture is little endian. 
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public bool IsLittleEndian()
        {
            return BitConverter.IsLittleEndian;
        }
    }
}