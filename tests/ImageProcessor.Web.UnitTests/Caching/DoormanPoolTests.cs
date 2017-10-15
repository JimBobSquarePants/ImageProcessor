using System;
using ImageProcessor.Web.Caching;
using NUnit.Framework;

namespace ImageProcessor.Web.UnitTests.Caching
{
    public class DoormanPoolTests
    {
        [Test]
        public void RentingGivesDifferentInstances()
        {
            Doorman first = DoormanPool.Rent();
            Doorman second = DoormanPool.Rent();

            Assert.AreNotSame(first, second);

            DoormanPool.Return(first);
            DoormanPool.Return(second);
        }

        [Test]
        public void DoormanPoolReusesItems()
        {
            int initialCount = DoormanPool.Count();
            Doorman first = DoormanPool.Rent();

            int currentCount = DoormanPool.Count();
            if (currentCount > 0)
            {
                Assert.AreEqual(initialCount - 1, currentCount);
                DoormanPool.Return(first);
                Assert.AreEqual(initialCount, DoormanPool.Count());
            }
            else
            {
                Assert.AreEqual(0, currentCount);
                DoormanPool.Return(first);
                Assert.AreEqual(initialCount + 1, DoormanPool.Count());
            }
        }

        [Test]
        public void CallingReturnWithNullThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                DoormanPool.Return(null);
            });
        }
    }
}