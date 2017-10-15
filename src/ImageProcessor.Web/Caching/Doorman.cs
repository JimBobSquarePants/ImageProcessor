// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Doorman.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Threading;

namespace ImageProcessor.Web.Caching
{
    /// <summary>
    /// A wrapper around <see cref="SemaphoreSlim"/> that operates a one-in-one out policy
    /// </summary>
    internal sealed class Doorman : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Doorman"/> class.
        /// </summary>
        public Doorman()
        {
            this.Semaphore = new SemaphoreSlim(1, 1);
            this.RefCount = 1;
        }

        /// <summary>
        /// Gets the SemaphoreSlim that performs the limiting
        /// </summary>
        public SemaphoreSlim Semaphore { get; }

        /// <summary>
        /// Gets or sets the number of references to this doorman.
        /// </summary>
        public int RefCount { get; set; }

        public void Reset()
        {
            this.RefCount = 1;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.RefCount = 1;
            this.Semaphore.Dispose();
        }
    }
}
