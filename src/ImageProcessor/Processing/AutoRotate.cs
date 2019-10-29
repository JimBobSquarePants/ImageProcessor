// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;
using System.Drawing.Imaging;
using ImageProcessor.Metadata;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Performs auto-rotation to ensure that EXIF defined rotation is reflected in
    /// the final image. <see href="http://sylvana.net/jpegcrop/exif_orientation.html"/>.
    /// </summary>
    public class AutoRotate : IGraphicsProcessor
    {
        /// <inheritdoc/>
        public Image ProcessImageFrame(ImageFactory factory, Image frame)
        {
            const int Orientation = (int)ExifPropertyTag.Orientation;

            // Images are always rotated before and after processing if there is an
            // orientation key present. By removing the property item we prevent the reverse
            // rotation.
            if (factory.MetadataMode != MetadataMode.All
                && factory.PropertyItems.ContainsKey(Orientation))
            {
                factory.PropertyItems.TryRemove(Orientation, out PropertyItem _);
            }

            return frame;
        }
    }
}
