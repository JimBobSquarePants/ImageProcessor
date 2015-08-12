// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageExtensions.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates a series of time saving extension methods to the <see cref="Image" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Common.Extensions
{
    using System.Drawing;
    using System.Drawing.Imaging;

    using ImageProcessor.Imaging.Formats;

    /// <summary>
    /// Encapsulates a series of time saving extension methods to the <see cref="Image"/> class.
    /// </summary>
    internal static class ImageExtensions
    {
        /// <summary>
        /// Creates a copy of an image allowing you to set the pixel format.
        /// Disposing of the original is the responsibility of the user.
        /// <remarks>
        /// Unlike the native <see cref="Image.Clone"/> method this also copies animation frames.
        /// </remarks>
        /// </summary>
        /// <param name="source">The source image to copy.</param>
        /// <param name="format">The <see cref="PixelFormat"/> to set the copied image to.</param>
        /// <returns>
        /// The <see cref="Image"/>.
        /// </returns>
        public static Image Copy(this Image source, PixelFormat format = PixelFormat.Format32bppPArgb)
        {
            if (FormatUtilities.IsAnimated(source))
            {
                // TODO: Ensure that we handle other animated types.
                GifDecoder decoder = new GifDecoder(source);
                GifEncoder encoder = new GifEncoder(null, null, decoder.LoopCount);
                foreach (GifFrame frame in decoder.GifFrames)
                {
                    frame.Image = ((Bitmap)frame.Image).Clone(new Rectangle(0, 0, frame.Image.Width, frame.Image.Height), format);
                    ((Bitmap)frame.Image).SetResolution(source.HorizontalResolution, source.VerticalResolution);
                    encoder.AddFrame(frame);
                }

                return encoder.Save();
            }

            Bitmap copy = ((Bitmap)source).Clone(new Rectangle(0, 0, source.Width, source.Height), format);
            copy.SetResolution(source.HorizontalResolution, source.VerticalResolution);
            return copy;
        }
    }
}
