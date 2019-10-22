// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Detects edges within an image using RobertsCross operators.
    /// <see href="http://en.wikipedia.org/wiki/Roberts_cross"/>.
    /// </summary>
    public class RobertsCross : EdgeDetection2DProcessor
    {
        private static readonly double[,] KernelX = new double[,]
        {
            { 1, 0 },
            { 0, -1 }
        };

        private static readonly double[,] KernelY = new double[,]
        {
            { 0, 1 },
            { -1, 0 }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="RobertsCross"/> class.
        /// </summary>
        /// <param name="grayscale">Whether to convert the image to grascale before processing.</param>
        public RobertsCross(bool grayscale)
            : base(new KernelPair(KernelX, KernelY), grayscale)
        {
        }
    }
}
