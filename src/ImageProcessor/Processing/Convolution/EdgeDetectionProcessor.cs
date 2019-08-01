// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// A base class for processing images using edge detection.
    /// </summary>
    public abstract class EdgeDetectionProcessor : IGraphicsProcessor<double[,]>, IGraphicsProcessor<bool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeDetectionProcessor"/> class.
        /// </summary>
        /// <param name="kernel">The kernel operator.</param>
        /// <param name="grayscale">Whether to convert the image to grascale before processing.</param>
        public EdgeDetectionProcessor(double[,] kernel, bool grayscale)
        {
            this.Kernel = kernel;
            this.Grayscale = grayscale;
        }

        /// <summary>
        /// Gets the kernel operator.
        /// </summary>
        public double[,] Kernel { get; }

        /// <summary>
        /// Gets a value indicating whether to convert the image to grascale before processing.
        /// </summary>
        public bool Grayscale { get; }

        /// <inheritdoc/>
        double[,] IGraphicsProcessor<double[,]>.Options
        {
            get { return this.Kernel; }
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
            Image result = new ConvolutionProcessor(this.Kernel).ProcessImageFrame(factory, frame);

            frame.Dispose();
            return result;
        }
    }
}
