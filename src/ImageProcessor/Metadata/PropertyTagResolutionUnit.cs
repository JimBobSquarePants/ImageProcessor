// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

namespace ImageProcessor.Metadata
{
    /// <summary>
    /// The following enum gives the unit of measure for the horizontal resolution and the vertical resolution
    /// supported by Windows GDI+.
    /// <see href="https://msdn.microsoft.com/en-us/library/ms534416(v=vs.85).aspx#_gdiplus_constant_propertytagresolutionunit"/>.
    /// </summary>
    public enum PropertyTagResolutionUnit : ushort
    {
        /// <summary>
        /// The resolution is measured in pixels per inch.
        /// </summary>
        Inch = 2,

        /// <summary>
        /// The resolution is measured in pixels per centimeter.
        /// </summary>
        Cm = 3
    }
}
