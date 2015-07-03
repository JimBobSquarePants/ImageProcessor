// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsyncDuplicateLock.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The async duplicate lock prevents multiple asynchronous threads acting upon the same object
//   with the given key at the same time. It is designed so that it does not block unique requests
//   allowing a high throughput.
//   <see href="http://stackoverflow.com/a/31194647/427899" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImageProcessor.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The async duplicate lock prevents multiple asynchronous threads acting upon the same object
    /// with the given key at the same time. It is designed so that it does not block unique requests
    /// allowing a high throughput.
    /// <see href="http://stackoverflow.com/a/31194647/427899"/>
    /// </summary>
    internal sealed class AsyncDuplicateLock
    {
        /// <summary>
        /// A collection of reference counters used for tracking references to the same object.
        /// </summary>
        private static readonly Dictionary<object, RefCounted<SemaphoreSlim>> SemaphoreSlims
                              = new Dictionary<object, RefCounted<SemaphoreSlim>>();

        /// <summary>
        /// Locks the current thread asynchronously.
        /// </summary>
        /// <param name="key">
        /// The key identifying the specific object to lock against.
        /// </param>
        /// <returns>
        /// The <see cref="IDisposable"/> that will release the lock.
        /// </returns>
        public IDisposable Lock(object key)
        {
            GetOrCreate(key).Wait();
            return new Releaser(key);
        }

        /// <summary>
        /// Locks the current thread asynchronously.
        /// </summary>
        /// <param name="key">
        /// The key identifying the specific object to lock against.
        /// </param>
        /// <returns>
        /// The <see cref="Task{IDisposable}"/> that will release the lock.
        /// </returns>
        public async Task<IDisposable> LockAsync(object key)
        {
            await GetOrCreate(key).WaitAsync().ConfigureAwait(false);
            return new Releaser(key);
        }

        /// <summary>
        /// Returns a <see cref="SemaphoreSlim"/> matching on the given key
        ///  or a new one if none is found.
        /// </summary>
        /// <param name="key">
        /// The key identifying the semaphore.
        /// </param>
        /// <returns>
        /// The <see cref="SemaphoreSlim"/>.
        /// </returns>
        private static SemaphoreSlim GetOrCreate(object key)
        {
            RefCounted<SemaphoreSlim> item;
            lock (SemaphoreSlims)
            {
                if (SemaphoreSlims.TryGetValue(key, out item))
                {
                    ++item.RefCount;
                }
                else
                {
                    item = new RefCounted<SemaphoreSlim>(new SemaphoreSlim(1, 1));
                    SemaphoreSlims[key] = item;
                }
            }

            return item.Value;
        }

        /// <summary>
        /// Tracks the number of references made against the given object.
        /// </summary>
        /// <typeparam name="T">
        /// The object to count references against.
        /// </typeparam>
        private sealed class RefCounted<T>
        {
            /// <summary>
            /// The object to count references against.
            /// </summary>
            private readonly T value;

            /// <summary>
            /// Initializes a new instance of the <see cref="RefCounted{T}"/> class.
            /// </summary>
            /// <param name="value">
            /// The object to count references against.
            /// </param>
            public RefCounted(T value)
            {
                this.RefCount = 1;
                this.value = value;
            }

            /// <summary>
            /// Gets or sets the number of references.
            /// </summary>
            public int RefCount { get; set; }

            /// <summary>
            /// Gets the object to count references against.
            /// </summary>
            public T Value
            {
                get
                {
                    return this.value;
                }
            }
        }

        /// <summary>
        /// The disposable releaser tasked with releasing the semaphore.
        /// </summary>
        private sealed class Releaser : IDisposable
        {
            /// <summary>
            /// The key identifying the semaphore that limits the number of threads.
            /// </summary>
            private readonly object key;

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
            /// <param name="key">
            /// The key identifying the semaphore that limits the number of threads.
            /// </param>
            public Releaser(object key)
            {
                this.key = key;
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
                    RefCounted<SemaphoreSlim> item;
                    lock (SemaphoreSlims)
                    {
                        item = SemaphoreSlims[this.key];
                        --item.RefCount;
                        if (item.RefCount == 0)
                        {
                            SemaphoreSlims.Remove(this.key);
                        }
                    }

                    item.Value.Release();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // Note disposing is done.
                this.isDisposed = true;
            }
        }
    }
}
