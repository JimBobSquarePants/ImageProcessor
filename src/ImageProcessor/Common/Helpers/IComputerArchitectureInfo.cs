// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

namespace ImageProcessor
{
    /// <summary>
    /// Encapsulates methods that provide information about the current computer architecture.
    /// </summary>
    public interface IComputerArchitectureInfo
    {
        /// <summary>
        /// Returns a value indicating whether the current computer architecture is little endian.
        /// </summary>
        /// <returns>The <see cref="bool"/>.</returns>
        bool IsLittleEndian();
    }
}
