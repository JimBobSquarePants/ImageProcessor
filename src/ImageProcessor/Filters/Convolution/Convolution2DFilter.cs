﻿// <copyright file="Convolution2DFilter.cs" company="James South">
// Copyright (c) James South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace ImageProcessor.Filters
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines a filter that uses a matrix to perform convolution across two dimensions against an image.
    /// </summary>
    public abstract class Convolution2DFilter : ParallelImageProcessor
    {
        /// <summary>
        /// Gets the horizontal gradient operator.
        /// </summary>
        public abstract float[,] KernelX { get; }

        /// <summary>
        /// Gets the vertical gradient operator.
        /// </summary>
        public abstract float[,] KernelY { get; }

        /// <inheritdoc/>
        protected override void Apply(ImageBase target, ImageBase source, Rectangle targetRectangle, Rectangle sourceRectangle, int startY, int endY)
        {
            float[,] kernelX = this.KernelX;
            float[,] kernelY = this.KernelY;
            int kernelLength = kernelX.GetLength(0);
            int radius = kernelLength >> 1;

            int sourceY = sourceRectangle.Y;
            int sourceBottom = sourceRectangle.Bottom;
            int startX = sourceRectangle.X;
            int endX = sourceRectangle.Right;
            int maxY = sourceBottom - 1;
            int maxX = endX - 1;

            Parallel.For(
                startY,
                endY,
                y =>
                {
                    if (y >= sourceY && y < sourceBottom)
                    {
                        for (int x = startX; x < endX; x++)
                        {
                            float rX = 0;
                            float gX = 0;
                            float bX = 0;
                            float rY = 0;
                            float gY = 0;
                            float bY = 0;

                            // Apply each matrix multiplier to the color components for each pixel.
                            for (int fy = 0; fy < kernelLength; fy++)
                            {
                                int fyr = fy - radius;
                                int offsetY = y + fyr;

                                offsetY = offsetY.Clamp(0, maxY);

                                for (int fx = 0; fx < kernelLength; fx++)
                                {
                                    int fxr = fx - radius;
                                    int offsetX = x + fxr;

                                    offsetX = offsetX.Clamp(0, maxX);

                                    Color currentColor = source[offsetX, offsetY];
                                    float r = currentColor.R;
                                    float g = currentColor.G;
                                    float b = currentColor.B;

                                    rX += kernelX[fy, fx] * r;
                                    gX += kernelX[fy, fx] * g;
                                    bX += kernelX[fy, fx] * b;

                                    rY += kernelY[fy, fx] * r;
                                    gY += kernelY[fy, fx] * g;
                                    bY += kernelY[fy, fx] * b;
                                }
                            }

                            float red = (float)Math.Sqrt((rX * rX) + (rY * rY));
                            float green = (float)Math.Sqrt((gX * gX) + (gY * gY));
                            float blue = (float)Math.Sqrt((bX * bX) + (bY * bY));

                            target[x, y] = new Color(red, green, blue);
                        }
                    }
                });
        }
    }
}