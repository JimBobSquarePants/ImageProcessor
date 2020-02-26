// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Drawing;

namespace ImageProcessor.Processing
{
    /// <summary>
    /// Provides methods to help calculate the target rectangle when resizing using the
    /// <see cref="ResizeMode"/> enumeration.
    /// </summary>
    internal static class ResizeHelper
    {
        /// <summary>
        /// Calculates the target location and bounds to perform the resize operation against.
        /// </summary>
        /// <param name="sourceSize">The source image size.</param>
        /// <param name="options">The resize options.</param>
        /// <param name="width">The target width.</param>
        /// <param name="height">The target height.</param>
        /// <returns>
        /// The tuple representing the location and the bounds.
        /// </returns>
        public static (Size, Rectangle) CalculateTargetLocationAndBounds(
            Size sourceSize,
            ResizeOptions options,
            int width,
            int height)
        {
            if (width <= 0 && height <= 0)
            {
                ThrowInvalid($"Target width {width} and height {height} must be greater than zero.");
            }

            switch (options.ResizeMode)
            {
                case ResizeMode.Crop:
                    return CalculateCropRectangle(sourceSize, options, width, height);
                case ResizeMode.Pad:
                    return CalculatePadRectangle(sourceSize, options, width, height);
                case ResizeMode.BoxPad:
                    return CalculateBoxPadRectangle(sourceSize, options, width, height);
                case ResizeMode.Max:
                    return CalculateMaxRectangle(sourceSize, width, height);
                case ResizeMode.Min:
                    return CalculateMinRectangle(sourceSize, width, height);

                // Last case ResizeMode.Stretch:
                default:
                    return (new Size(width, height), new Rectangle(0, 0, width, height));
            }
        }

        private static (Size, Rectangle) CalculateBoxPadRectangle(
            Size source,
            ResizeOptions options,
            int width,
            int height)
        {
            if (width <= 0 || height <= 0)
            {
                return (new Size(source.Width, source.Height), new Rectangle(0, 0, source.Width, source.Height));
            }

            int sourceWidth = source.Width;
            int sourceHeight = source.Height;

            // Fractional variants for preserving aspect ratio.
            float percentHeight = Math.Abs(height / (float)sourceHeight);
            float percentWidth = Math.Abs(width / (float)sourceWidth);

            int boxPadHeight = height > 0 ? height : (int)Math.Round(sourceHeight * percentWidth);
            int boxPadWidth = width > 0 ? width : (int)Math.Round(sourceWidth * percentHeight);

            // Only calculate if upscaling.
            if (sourceWidth < boxPadWidth && sourceHeight < boxPadHeight)
            {
                int destinationX;
                int destinationY;
                int destinationWidth = sourceWidth;
                int destinationHeight = sourceHeight;
                width = boxPadWidth;
                height = boxPadHeight;

                switch (options.Position)
                {
                    case AnchorPositionMode.Left:
                        destinationY = (height - sourceHeight) / 2;
                        destinationX = 0;
                        break;
                    case AnchorPositionMode.Right:
                        destinationY = (height - sourceHeight) / 2;
                        destinationX = width - sourceWidth;
                        break;
                    case AnchorPositionMode.TopRight:
                        destinationY = 0;
                        destinationX = width - sourceWidth;
                        break;
                    case AnchorPositionMode.Top:
                        destinationY = 0;
                        destinationX = (width - sourceWidth) / 2;
                        break;
                    case AnchorPositionMode.TopLeft:
                        destinationY = 0;
                        destinationX = 0;
                        break;
                    case AnchorPositionMode.BottomRight:
                        destinationY = height - sourceHeight;
                        destinationX = width - sourceWidth;
                        break;
                    case AnchorPositionMode.Bottom:
                        destinationY = height - sourceHeight;
                        destinationX = (width - sourceWidth) / 2;
                        break;
                    case AnchorPositionMode.BottomLeft:
                        destinationY = height - sourceHeight;
                        destinationX = 0;
                        break;
                    default:
                        destinationY = (height - sourceHeight) / 2;
                        destinationX = (width - sourceWidth) / 2;
                        break;
                }

                return (new Size(width, height),
                           new Rectangle(destinationX, destinationY, destinationWidth, destinationHeight));
            }

            // Switch to pad mode to downscale and calculate from there.
            return CalculatePadRectangle(source, options, width, height);
        }

        private static (Size, Rectangle) CalculateCropRectangle(
            Size source,
            ResizeOptions options,
            int width,
            int height)
        {
            float ratio;
            int sourceWidth = source.Width;
            int sourceHeight = source.Height;

            int targetX = 0;
            int targetY = 0;
            int targetWidth = width;
            int targetHeight = height;

            // Fractional variants for preserving aspect ratio.
            float percentHeight = Math.Abs(height / (float)sourceHeight);
            float percentWidth = Math.Abs(width / (float)sourceWidth);

            if (percentHeight < percentWidth)
            {
                ratio = percentWidth;

                if (options.CenterCoordinates.HasValue)
                {
                    float center = -(ratio * sourceHeight) * options.CenterCoordinates.Value.Y;
                    targetY = (int)Math.Round(center + (height / 2F));

                    if (targetY > 0)
                    {
                        targetY = 0;
                    }

                    if (targetY < (int)Math.Round(height - (sourceHeight * ratio)))
                    {
                        targetY = (int)Math.Round(height - (sourceHeight * ratio));
                    }
                }
                else
                {
                    switch (options.Position)
                    {
                        case AnchorPositionMode.Top:
                        case AnchorPositionMode.TopLeft:
                        case AnchorPositionMode.TopRight:
                            targetY = 0;
                            break;
                        case AnchorPositionMode.Bottom:
                        case AnchorPositionMode.BottomLeft:
                        case AnchorPositionMode.BottomRight:
                            targetY = (int)Math.Round(height - (sourceHeight * ratio));
                            break;
                        default:
                            targetY = (int)Math.Round((height - (sourceHeight * ratio)) / 2F);
                            break;
                    }
                }

                targetHeight = (int)Math.Ceiling(sourceHeight * percentWidth);
            }
            else
            {
                ratio = percentHeight;

                if (options.CenterCoordinates.HasValue)
                {
                    float center = -(ratio * sourceWidth) * options.CenterCoordinates.Value.X;
                    targetX = (int)Math.Round(center + (width / 2F));

                    if (targetX > 0)
                    {
                        targetX = 0;
                    }

                    if (targetX < (int)Math.Round(width - (sourceWidth * ratio)))
                    {
                        targetX = (int)Math.Round(width - (sourceWidth * ratio));
                    }
                }
                else
                {
                    switch (options.Position)
                    {
                        case AnchorPositionMode.Left:
                        case AnchorPositionMode.TopLeft:
                        case AnchorPositionMode.BottomLeft:
                            targetX = 0;
                            break;
                        case AnchorPositionMode.Right:
                        case AnchorPositionMode.TopRight:
                        case AnchorPositionMode.BottomRight:
                            targetX = (int)Math.Round(width - (sourceWidth * ratio));
                            break;
                        default:
                            targetX = (int)Math.Round((width - (sourceWidth * ratio)) / 2F);
                            break;
                    }
                }

                targetWidth = (int)Math.Ceiling(sourceWidth * percentHeight);
            }

            // Target image width and height can be different to the rectangle width and height.
            return (new Size(width, height), new Rectangle(targetX, targetY, targetWidth, targetHeight));
        }

        private static (Size, Rectangle) CalculateMaxRectangle(
            Size source,
            int width,
            int height)
        {
            int targetWidth = width;
            int targetHeight = height;

            // Fractional variants for preserving aspect ratio.
            float percentHeight = Math.Abs(height / (float)source.Height);
            float percentWidth = Math.Abs(width / (float)source.Width);

            // Integers must be cast to floats to get needed precision
            float ratio = height / (float)width;
            float sourceRatio = source.Height / (float)source.Width;

            if (sourceRatio < ratio)
            {
                targetHeight = (int)Math.Round(source.Height * percentWidth);
            }
            else
            {
                targetWidth = (int)Math.Round(source.Width * percentHeight);
            }

            // Replace the size to match the rectangle.
            return (new Size(targetWidth, targetHeight), new Rectangle(0, 0, targetWidth, targetHeight));
        }

        private static (Size, Rectangle) CalculateMinRectangle(
            Size source,
            int width,
            int height)
        {
            int sourceWidth = source.Width;
            int sourceHeight = source.Height;
            int targetWidth = width;
            int targetHeight = height;

            // Don't upscale
            if (width > sourceWidth || height > sourceHeight)
            {
                return (new Size(sourceWidth, sourceHeight), new Rectangle(0, 0, sourceWidth, sourceHeight));
            }

            // Find the shortest distance to go.
            int widthDiff = sourceWidth - width;
            int heightDiff = sourceHeight - height;

            if (widthDiff < heightDiff)
            {
                float sourceRatio = (float)sourceHeight / sourceWidth;
                targetHeight = (int)Math.Round(width * sourceRatio);
            }
            else if (widthDiff > heightDiff)
            {
                float sourceRatioInverse = (float)sourceWidth / sourceHeight;
                targetWidth = (int)Math.Round(height * sourceRatioInverse);
            }
            else
            {
                if (height > width)
                {
                    float percentWidth = Math.Abs(width / (float)sourceWidth);
                    targetHeight = (int)Math.Round(sourceHeight * percentWidth);
                }
                else
                {
                    float percentHeight = Math.Abs(height / (float)sourceHeight);
                    targetWidth = (int)Math.Round(sourceWidth * percentHeight);
                }
            }

            // Replace the size to match the rectangle.
            return (new Size(targetWidth, targetHeight), new Rectangle(0, 0, targetWidth, targetHeight));
        }

        private static (Size, Rectangle) CalculatePadRectangle(
            Size sourceSize,
            ResizeOptions options,
            int width,
            int height)
        {
            float ratio;
            int sourceWidth = sourceSize.Width;
            int sourceHeight = sourceSize.Height;

            int targetX = 0;
            int targetY = 0;
            int targetWidth = width;
            int targetHeight = height;

            // Fractional variants for preserving aspect ratio.
            float percentHeight = Math.Abs(height / (float)sourceHeight);
            float percentWidth = Math.Abs(width / (float)sourceWidth);

            if (percentHeight < percentWidth)
            {
                ratio = percentHeight;
                targetWidth = (int)Math.Round(sourceWidth * percentHeight);

                switch (options.Position)
                {
                    case AnchorPositionMode.Left:
                    case AnchorPositionMode.TopLeft:
                    case AnchorPositionMode.BottomLeft:
                        targetX = 0;
                        break;
                    case AnchorPositionMode.Right:
                    case AnchorPositionMode.TopRight:
                    case AnchorPositionMode.BottomRight:
                        targetX = (int)Math.Round(width - (sourceWidth * ratio));
                        break;
                    default:
                        targetX = (int)Math.Round((width - (sourceWidth * ratio)) / 2F);
                        break;
                }
            }
            else
            {
                ratio = percentWidth;
                targetHeight = (int)Math.Round(sourceHeight * percentWidth);

                switch (options.Position)
                {
                    case AnchorPositionMode.Top:
                    case AnchorPositionMode.TopLeft:
                    case AnchorPositionMode.TopRight:
                        targetY = 0;
                        break;
                    case AnchorPositionMode.Bottom:
                    case AnchorPositionMode.BottomLeft:
                    case AnchorPositionMode.BottomRight:
                        targetY = (int)Math.Round(height - (sourceHeight * ratio));
                        break;
                    default:
                        targetY = (int)Math.Round((height - (sourceHeight * ratio)) / 2F);
                        break;
                }
            }

            // Target image width and height can be different to the rectangle width and height.
            return (new Size(width, height), new Rectangle(targetX, targetY, targetWidth, targetHeight));
        }

        private static void ThrowInvalid(string message) => throw new InvalidOperationException(message);
    }
}
