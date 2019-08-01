// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// A base class for processing images using edge detection.
    /// </summary>
    public abstract class EdgeDetection2DProcessor : IGraphicsProcessor<KernelPair>, IGraphicsProcessor<bool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeDetection2DProcessor"/> class.
        /// </summary>
        /// <param name="kernels">The horizontal and vertical kernel operators.</param>
        /// <param name="grayscale">Whether to convert the image to grascale before processing.</param>
        public EdgeDetection2DProcessor(KernelPair kernels, bool grayscale)
        {
            this.Kernels = kernels;
            this.Grayscale = grayscale;
        }

        /// <summary>
        /// Gets the kernel operator pair.
        /// </summary>
        public KernelPair Kernels { get; }

        /// <summary>
        /// Gets a value indicating whether to convert the image to grascale before processing.
        /// </summary>
        public bool Grayscale { get; }

        /// <inheritdoc/>
        KernelPair IGraphicsProcessor<KernelPair>.Options
        {
            get { return this.Kernels; }
        }

        /// <inheritdoc/>
        bool IGraphicsProcessor<bool>.Options
        {
            get { return this.Grayscale; }
        }

        /// <inheritdoc/>
        public Image ProcessImageFrame(ImageFactory factory, Image frame)
        {
            if (this.Grayscale)
            {
                // Grayscale is not destructive and will return the same image.
                frame = new Grayscale(100F).ProcessImageFrame(factory, frame);
            }

            // Convolution is destructive and will create a new image.
            Image result = new Convolution2DProcessor(this.Kernels).ProcessImageFrame(factory, frame);

            frame.Dispose();
            return result;
        }
    }
}
