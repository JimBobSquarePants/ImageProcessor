// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsyncKeyLock.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ImageProcessor.Web.Caching
{
    /// <summary>
    /// The async key lock prevents multiple asynchronous threads acting upon the same object with the given key at the same time.
    /// It is designed so that it does not block unique requests allowing a high throughput.
    /// </summary>
    internal sealed class AsyncKeyLock
    {
        /// <summary>
        /// A collection of doorman counters used for tracking references to the same key.
        /// </summary>
        private static readonly Dictionary<string, Doorman> Keys = new Dictionary<string, Doorman>();

        /// <summary>
        /// Locks the current thread asynchronously.
        /// </summary>
        /// <param name="key">The key identifying the specific object to lock against.</param>
        /// <returns>
        /// The <see cref="Task{IDisposable}"/> that will release the lock.
        /// </returns>
        public async Task<IDisposable> LockAsync(string key)
        {
            string lowerKey = key.ToLowerInvariant();
            await GetOrCreate(lowerKey).WaitAsync().ConfigureAwait(false);
            return new Releaser(lowerKey);
        }

        /// <summary>
        /// Returns a <see cref="SemaphoreSlim"/> matching on the given key
        ///  or a new one if none is found.
        /// </summary>
        /// <param name="key">The key identifying the semaphore.</param>
        /// <returns>
        /// The <see cref="SemaphoreSlim"/>.
        /// </returns>
        private static SemaphoreSlim GetOrCreate(string key)
        {
            Doorman item;
            lock (Keys)
            {
                if (Keys.TryGetValue(key, out item))
                {
                    ++item.RefCount;
                }
                else
                {
                    item = DoormanPool.Rent();
                    Keys[key] = item;
                }
            }

            return item.Semaphore;
        }

        /// <summary>
        /// The disposable releaser tasked with releasing the semaphore.
        /// </summary>
        private sealed class Releaser : IDisposable
        {
            /// <summary>
            /// The key identifying the <see cref="Doorman"/> that limits the number of threads.
            /// </summary>
            private readonly string key;

            /// <summary>
            /// Initializes a new instance of the <see cref="Releaser"/> class.
            /// </summary>
            /// <param name="key">The key identifying the doorman that limits the number of threads.</param>
            public Releaser(string key) => this.key = key;

            /// <inheritdoc />
            public void Dispose()
            {
                lock (Keys)
                {
                    Doorman doorman = Keys[this.key];
                    --doorman.RefCount;
                    if (doorman.RefCount == 0)
                    {
                        Keys.Remove(this.key);
                        doorman.Reset();
                        DoormanPool.Return(doorman);
                    }

                    doorman.Semaphore.Release();
                }
            }
        }
    }
}