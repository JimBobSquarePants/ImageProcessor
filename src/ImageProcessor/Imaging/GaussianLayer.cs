// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GaussianLayer.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   A Gaussian layer for applying sharpening and blurring methods to an image.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Imaging
{
    using System;

    /// <summary>
    /// A Gaussian layer for applying sharpening and blurring methods to an image.
    /// </summary>
    public class GaussianLayer : IEquatable<GaussianLayer>
    {
        /// <summary>
        /// The size.
        /// </summary>
        private int size;

        /// <summary>
        /// The sigma.
        /// </summary>
        private double sigma;

        /// <summary>
        /// The threshold.
        /// </summary>
        private int threshold;

        /// <summary>
        /// Initializes a new instance of the <see cref="GaussianLayer"/> class.
        /// </summary>
        public GaussianLayer()
        {
            this.Size = 3;
            this.Sigma = 1.4;
            this.Threshold = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GaussianLayer"/> class.
        /// </summary>
        /// <param name="size">
        /// The size to set the Gaussian kernel to.
        /// </param>
        /// <param name="sigma">
        /// The Sigma value (standard deviation) for Gaussian function used to calculate the kernel.
        /// </param>
        /// <param name="threshold">
        /// The threshold value, which is added to each weighted sum of pixels.
        /// </param>
        public GaussianLayer(int size, double sigma = 1.4, int threshold = 0)
        {
            this.Size = size;
            this.Sigma = sigma;
            this.Threshold = threshold;
        }

        /// <summary>
        /// Gets or sets the size of the Gaussian kernel.
        /// <remarks>
        /// <para>
        /// If set to a value below 0, the property will be set to 0.
        /// </para>
        /// </remarks>
        /// </summary>
        public int Size
        {
            get => this.size;

            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                this.size = value;
            }
        }

        /// <summary>
        /// Gets or sets the sigma value (standard deviation) for Gaussian function used to calculate the kernel.
        /// <remarks>
        /// <para>
        /// If set to a value below 0, the property will be set to 0.
        /// </para>
        /// </remarks>
        /// </summary>
        public double Sigma
        {
            get => this.sigma;

            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                this.sigma = value;
            }
        }

        /// <summary>
        /// Gets or sets the threshold value, which is added to each weighted sum of pixels.
        /// <remarks>
        /// <para>
        /// If set to a value below 0, the property will be set to 0.
        /// </para>
        /// </remarks>
        /// </summary>
        public int Threshold
        {
            get => this.threshold;

            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                this.threshold = value;
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is GaussianLayer gaussianLayer && this.Equals(gaussianLayer);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(GaussianLayer other) => other != null
            && this.Size == other.Size
            && this.Sigma == other.Sigma
            && this.Threshold == other.Threshold;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => (this.Size, this.Sigma, this.Threshold).GetHashCode();
    }
}
