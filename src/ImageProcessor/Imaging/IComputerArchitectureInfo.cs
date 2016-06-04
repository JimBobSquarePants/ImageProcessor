// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IComputerArchitectureInfo.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates methods that provide information about the current computer architecture.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging
{
    /// <summary>
    /// Encapsulates methods that provide information about the current computer architecture.
    /// </summary>
    public interface IComputerArchitectureInfo
    {
        /// <summary>
        /// Returns a value indicating whether the current computer architecture is little endian. 
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        bool IsLittleEndian();
    }
}