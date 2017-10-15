using ImageProcessor.Web.Caching;
using NUnit.Framework;

namespace ImageProcessor.Web.UnitTests.Caching
{
    public class DoormanTests
    {
        [Test]
        public void DoormanInitializesSemaphoreSlim()
        {
            var doorman = new Doorman();
            Assert.NotNull(doorman.Semaphore);
            Assert.AreEqual(1, doorman.Semaphore.CurrentCount);
        }

        [Test]
        public void DoormanResetsRefCounter()
        {
            var doorman = new Doorman();
            Assert.AreEqual(1, doorman.RefCount);
            doorman.RefCount--;

            Assert.AreEqual(0, doorman.RefCount);

            doorman.Reset();
            Assert.AreEqual(1, doorman.RefCount);
        }
    }
}