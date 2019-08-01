// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace ImageProcessor
{
    /// <summary>
    /// Encapsulates methods that provide information about the current computer architecture.
    /// </summary>
    public class ComputerArchitectureInfo : IComputerArchitectureInfo
    {
        /// <summary>
        /// Returns a value indicating whether the current computer architecture is little endian.
        /// </summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool IsLittleEndian() => BitConverter.IsLittleEndian;
    }
}
