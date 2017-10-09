using System.Threading.Tasks;
using ImageProcessor.Web.Helpers;
using NUnit.Framework;

namespace ImageProcessor.Web.UnitTests.Helpers
{
    public class AsyncDuplicateLockTests
    {
        [Test]
        public void CorrectlyLocksOnKey()
        {
            var locker = new AsyncDuplicateLock();
            string key = "TESTKEY";
            bool working;

            Parallel.For(0, 10L, async i =>
            {
                using (await locker.LockAsync(key))
                {
                    working = true;
                    await Task.Delay(50);
                    Assert.True(working);
                    working = false;
                }
            });
        }
    }
}