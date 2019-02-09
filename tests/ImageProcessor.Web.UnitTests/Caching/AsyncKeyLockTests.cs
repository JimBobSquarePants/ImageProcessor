using System.Linq;
using System.Threading.Tasks;
using ImageProcessor.Web.Caching;
using NUnit.Framework;

namespace ImageProcessor.Web.UnitTests.Caching
{
    public class AsyncKeyLockTests
    {
        private readonly AsyncKeyLock asyncKeyLock = new AsyncKeyLock();
        private const string AsyncKey = "ASYNC_KEY";
        private const string AsyncKey1 = "ASYNC_KEY1";
        private const string AsyncKey2 = "ASYNC_KEY2";

        [Test]
        public void AsyncLockCanLockByKey()
        {
            bool zeroEntered = false;
            bool entered = false;
            int index = 0;
            Task[] tasks = Enumerable.Range(0, 5).Select(i => Task.Run(async () =>
            {
                using (await this.asyncKeyLock.WriterLockAsync(AsyncKey).ConfigureAwait(false))
                {
                    if (i == 0)
                    {
                        entered = true;
                        zeroEntered = true;
                        await Task.Delay(3000).ConfigureAwait(false);
                        entered = false;
                    }
                    else if (zeroEntered)
                    {
                        Assert.False(entered);
                    }

                    index++;
                }

            })).ToArray();

            Task.WaitAll(tasks);
            Assert.AreEqual(5, index);
        }

        [Test]
        public void AsyncLockAllowsDifferentKeysToRun()
        {
            bool zeroEntered = false;
            bool entered = false;
            int index = 0;
            Task[] tasks = Enumerable.Range(0, 5).Select(i => Task.Run(async () =>
            {
                using (await this.asyncKeyLock.WriterLockAsync(i > 0 ? AsyncKey2 : AsyncKey1).ConfigureAwait(false))
                {
                    if (i == 0)
                    {
                        entered = true;
                        zeroEntered = true;
                        await Task.Delay(2000).ConfigureAwait(false);
                        entered = false;
                    }
                    else if (zeroEntered)
                    {
                        Assert.True(entered);
                    }

                    index++;
                }

            })).ToArray();

            Task.WaitAll(tasks);
            Assert.AreEqual(5, index);
        }
    }
}