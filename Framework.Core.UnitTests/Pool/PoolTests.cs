using Microsoft.VisualStudio.TestTools.UnitTesting;
using Framework.Core.Pool;
using Framework.Logger;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;
using System.Threading;
using System.Linq;

namespace Framework.Core.UnitTests.Pool
{
    [TestClass]
    public class PoolTests
    {
        private Mock<ILogger> _logger;

        [TestInitialize]
        public void Init()
        {
            _logger = new Mock<ILogger>();
        }

        [TestMethod]
        public void WhenMaxPoolSizeTenThenTenPooledItemsAreCreated()
        {
            var pool = new Pool<TestPooledItem>(_logger.Object, () => new TestPooledItem(_logger.Object), 10);
            var poolItems = new List<TestPooledItem>();

            for (var i = 0; i < 20; i++)
            {
                try
                {
                    poolItems.Add(pool.Get());
                }
                catch
                {
                }
            }

            var expectedItems = 10;
            var actualItems = poolItems.Count;

            Assert.AreEqual(expectedItems, actualItems);
        }

        [TestMethod]
        public void WhenPooledItemIsReturnedThenItIsAddedBackToPool()
        {
            var pool = new Mock<IPool<TestPooledItem>>();
            var poolItems = new List<TestPooledItem>();

            var poolItem = pool.Object.Get();
            pool.Object.Return(poolItem);

            pool.Verify(p => p.Get(), Times.Once);
            pool.Verify(p => p.Return(poolItem), Times.Once);
        }

        [TestMethod]
        public void CreateReturnOfPooledItemsIsThreadSafe()
        {
            var pool = new Pool<TestPooledItem>(_logger.Object, () => new TestPooledItem(_logger.Object), 10);
            var pooledItems = new ConcurrentQueue<TestPooledItem>();
            var enqueueItems = 0;
            var dequeueItems = 0;

            foreach (int num in Enumerable.Range(0, 100))
            {
                var thread1 = new Thread(() =>
                {
                    try
                    {
                        pooledItems.Enqueue(pool.Get());
                        Interlocked.Increment(ref enqueueItems);
                    }
                    catch
                    {
                    }
                });
                thread1.Start();

                var thread2 = new Thread(() =>
                {
                    TestPooledItem pooledItem;
                    if (pooledItems.TryDequeue(out pooledItem))
                    {
                        pool.Return(pooledItem);
                        Interlocked.Increment(ref dequeueItems);
                    }
                });
                thread2.Start();
            }

            var expectedItems = 10;
            var actualItems = pooledItems.Count;

            Assert.IsTrue(actualItems <= expectedItems);
        }

        [TestMethod]
        public void CreateOfPooledItemsIsThreadSafe()
        {
            var pool = new Pool<TestPooledItem>(_logger.Object, () => new TestPooledItem(_logger.Object), 10);
            var pooledItems = new ConcurrentQueue<TestPooledItem>();

            foreach (int num in Enumerable.Range(0, 100))
            {
                var thread1 = new Thread(() =>
                {
                    try
                    {
                        pooledItems.Enqueue(pool.Get());
                    }
                    catch
                    {
                    }
                });
                thread1.Start();
            }

            var expectedItems = 10;
            var actualItems = pooledItems.Count;

            Assert.AreEqual(expectedItems, actualItems);
        }

        [TestMethod]
        public void PooledItemRemoveTest()
        {
            var pool = new Pool<TestPooledItem>(_logger.Object, () => new TestPooledItem(_logger.Object), 10);
            var pooledItems = new ConcurrentQueue<TestPooledItem>();

            var pooledItem = pool.Get();

            pool.Remove(pooledItem);

            var expectedItems = 0;
            var actualItems = pooledItems.Count;

            Assert.AreEqual(expectedItems, actualItems);
        }
    }
}
