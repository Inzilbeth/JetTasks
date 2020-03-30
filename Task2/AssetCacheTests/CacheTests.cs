using NUnit.Framework;
using Task2;
using System.Collections.Generic;

namespace AssetCacheTests
{
    public class CacheTests
    {
        Cache cache;

        [SetUp]
        public void Setup()
        {
            cache = new Cache();
        }

        [Test]
        public void AddAnchorUsageTest()
        {
            cache.AddAnchorUsage(125);
            Assert.AreEqual(cache.GetAnchorUsages(125), 1);
        }

        [Test]
        public void AddAnchorUsageTwiceTest()
        {
            cache.AddAnchorUsage(125);
            cache.AddAnchorUsage(125);

            Assert.AreEqual(cache.GetAnchorUsages(125), 2);
        }

        [Test]
        public void AddResourceUsageTest()
        {
            cache.AddResourceUsage("010ABC");
            Assert.AreEqual(cache.GetResourceUsgaes("010ABC"), 1);
        }

        [Test]
        public void AddResourceUsageTwiceTest()
        {
            cache.AddResourceUsage("foo");
            cache.AddResourceUsage("foo");

            Assert.AreEqual(cache.GetResourceUsgaes("foo"), 2);
        }

        [Test]
        public void GetUnaddedAnchorUsagesTest()
        {
            Assert.AreEqual(cache.GetAnchorUsages(777), 0);
        }

        [Test]
        public void GetUNaddedResourceUsagesTest()
        {
            Assert.AreEqual(cache.GetResourceUsgaes("oof"), 0);
        }

        [Test]
        public void GetAnchorComponentsTest()
        {
            cache.AddAnchorComponent(1, 100);
            var components = new List<ulong>();
            components.Add(100);

            Assert.AreEqual(cache.GetAnchorComponents(1), components);
        }

    }
}