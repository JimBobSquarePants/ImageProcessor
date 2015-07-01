// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsyncLock.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides a mechanism by which a lock can be placed around asynchronous code.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Helpers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a mechanism by which a lock can be placed around asynchronous code.
    /// </summary>
    internal sealed class AsyncLock
    {
        /// <summary>
        /// The disposable releaser task.
        /// </summary>
        private readonly Task<IDisposable> releaserTask;

        /// <summary>
        /// The semaphore that limits the number of threads.
        /// </summary>
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// The disposable releaser.
        /// </summary>
        private readonly IDisposable releaser;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLock"/> class.
        /// </summary>
        public AsyncLock()
        {
            this.releaser = new Releaser(this.semaphore);
            this.releaserTask = Task.FromResult(this.releaser);
        }

        /// <summary>
        /// Locks the current thread.
        /// </summary>
        /// <returns>
        /// The <see cref="IDisposable"/> that will release the lock.
        /// </returns>
        public IDisposable Lock()
        {
            this.semaphore.Wait();
            return this.releaser;
        }

        /// <summary>
        /// Locks the current thread asynchronously.
        /// </summary>
        /// <returns>
        /// The <see cref="Task{IDisposable}"/> that will release the lock.
        /// </returns>
        public Task<IDisposable> LockAsync()
        {
            Task waitTask = this.semaphore.WaitAsync();
            return waitTask.IsCompleted
                ? this.releaserTask
                       : waitTask.ContinueWith(
                           (_, r) => (IDisposable)r,
                           this.releaser,
                           CancellationToken.None,
                           TaskContinuationOptions.ExecuteSynchronously,
                           TaskScheduler.Default);
        }

        /// <summary>
        /// The disposable releaser tasked with releasing the semaphore.
        /// </summary>
        private class Releaser : IDisposable
        {
            /// <summary>
            /// The semaphore that limits the number of threads.
            /// </summary>
            private readonly SemaphoreSlim semaphore;

            /// <summary>
            /// A value indicating whether this instance of the given entity has been disposed.
            /// </summary>
            /// <value><see langword="true"/> if this instance has been disposed; otherwise, <see langword="false"/>.</value>
            /// <remarks>
            /// If the entity is disposed, it must not be disposed a second
            /// time. The isDisposed field is set the first time the entity
            /// is disposed. If the isDisposed field is true, then the Dispose()
            /// method will not dispose again. This help not to prolong the entity's
            /// life in the Garbage Collector.
            /// </remarks>
            private bool isDisposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="Releaser"/> class.
            /// </summary>
            /// <param name="semaphore">
            /// The semaphore that limits the number of threads.
            /// </param>
            public Releaser(SemaphoreSlim semaphore)
            {
                this.semaphore = semaphore;
            }

            /// <summary>
            /// Finalizes an instance of the <see cref="Releaser"/> class. 
            /// </summary>
            ~Releaser()
            {
                // Do not re-create Dispose clean-up code here.
                // Calling Dispose(false) is optimal in terms of
                // readability and maintainability.
                this.Dispose(false);
            }

            /// <summary>
            /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
            /// </summary>
            public void Dispose()
            {
                this.Dispose(true);

                // This object will be cleaned up by the Dispose method.
                // Therefore, you should call GC.SuppressFinalize to
                // take this object off the finalization queue
                // and prevent finalization code for this object
                // from executing a second time.
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Disposes the object and frees resources for the Garbage Collector.
            /// </summary>
            /// <param name="disposing">
            /// If true, the object gets disposed.
            /// </param>
            private void Dispose(bool disposing)
            {
                if (this.isDisposed)
                {
                    return;
                }

                if (disposing)
                {
                    this.semaphore.Release();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // Note disposing is done.
                this.isDisposed = true;
            }
        }
    }
}
