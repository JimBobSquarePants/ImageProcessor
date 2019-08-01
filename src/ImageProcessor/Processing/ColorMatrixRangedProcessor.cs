// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.

namespace ImageProcessor.Processing
{
    /// <summary>
    /// A base class for processing images via color matrices.
    /// </summary>
    public abstract class ColorMatrixRangedProcessor : ColorMatrixProcessor, IGraphicsProcessor<float>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorMatrixRangedProcessor"/> class.
        /// </summary>
        /// <param name="amount">The amount by which to adjust the matrix.</param>
        protected ColorMatrixRangedProcessor(float amount)
        {
            this.GuardRange(amount);
            this.Options = amount;
        }

        /// <inheritdoc/>
        public float Options { get; }

        /// <summary>
        /// A method for protecting input thresholds.
        /// </summary>
        /// <param name="amount">The amount to check.</param>
        /// <exception cref="ImageProcessingException">
        /// Thrown if the range is outith the acceptable threshold.
        /// </exception>
        protected virtual void GuardRange(float amount)
        {
            // Default to no-op.
        }
    }
}
