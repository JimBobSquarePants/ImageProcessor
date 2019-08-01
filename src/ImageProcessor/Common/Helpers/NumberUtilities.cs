// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Runtime.CompilerServices;

namespace ImageProcessor
{
    /// <summary>
    /// Reusable numeric utility methods.
    /// </summary>
    public static class NumberUtilities
    {
        /// <summary>
        /// Restricts a <see cref="byte"/> to be within a specified range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value. If value is less than min, min will be returned.</param>
        /// <param name="max">The maximum value. If value is greater than max, max will be returned.</param>
        /// <returns>
        /// The <see cref="byte"/> representing the clamped value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Clamp(byte value, byte min, byte max)
        {
            // Order is important here as someone might set min to higher than max.
            if (value >= max)
            {
                return max;
            }

            if (value <= min)
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Restricts a <see cref="uint"/> to be within a specified range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value. If value is less than min, min will be returned.</param>
        /// <param name="max">The maximum value. If value is greater than max, max will be returned.</param>
        /// <returns>
        /// The <see cref="int"/> representing the clamped value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Clamp(uint value, uint min, uint max)
        {
            if (value >= max)
            {
                return max;
            }

            if (value <= min)
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Restricts a <see cref="int"/> to be within a specified range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value. If value is less than min, min will be returned.</param>
        /// <param name="max">The maximum value. If value is greater than max, max will be returned.</param>
        /// <returns>
        /// The <see cref="int"/> representing the clamped value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
        {
            if (value >= max)
            {
                return max;
            }

            if (value <= min)
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Restricts a <see cref="float"/> to be within a specified range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value. If value is less than min, min will be returned.</param>
        /// <param name="max">The maximum value. If value is greater than max, max will be returned.</param>
        /// <returns>
        /// The <see cref="float"/> representing the clamped value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
            if (value >= max)
            {
                return max;
            }

            if (value <= min)
            {
                return min;
            }

            return value;
        }

        /// <summary>
        /// Restricts a <see cref="double"/> to be within a specified range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value. If value is less than min, min will be returned.</param>
        /// <param name="max">The maximum value. If value is greater than max, max will be returned.</param>
        /// <returns>
        /// The <see cref="double"/> representing the clamped value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double value, double min, double max)
        {
            if (value >= max)
            {
                return max;
            }

            if (value <= min)
            {
                return min;
            }

            return value;
        }
    }
}
