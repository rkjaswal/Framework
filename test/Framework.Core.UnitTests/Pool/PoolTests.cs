﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        private static readonly object Lock = new object(); 

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
            var actualItems = pool.PooledItemCount;

            Assert.AreEqual(expectedItems, actualItems);
        }

        [TestMethod]
        public void WhenPooledItemIsReturnedThenReturnIsCalled()
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

            foreach (int num in Enumerable.Range(0, 1000))
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
            var actualItems = pool.PooledItemCount;

            Assert.IsTrue(actualItems >= 0 && actualItems <= expectedItems);
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
            var actualItems = pool.PooledItemCount;

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
            var actualItems = pool.PooledItemCount;

            Assert.AreEqual(expectedItems, actualItems);
        }

        [TestMethod]
        public void CreateReturnOfPooledItemsIsParallelInvokeSafe()
        {
            var pool = new Pool<TestPooledItem>(_logger.Object, () => new TestPooledItem(_logger.Object), 10);
            var pooledItems = new ConcurrentQueue<TestPooledItem>();
            var enqueueItems = 0;
            var dequeueItems = 0;

            Action Get = () =>
            {
                for (var i = 0; i < 1000; i++)
                {
                    try
                    {
                        pooledItems.Enqueue(pool.Get());
                        enqueueItems++;
                    }
                    catch
                    {
                    }
                }
            };

            Action Return = () =>
            {
                Thread.Sleep(10);
                for (var i = 0; i < 500; i++)
                {
                    TestPooledItem pooledItem = null;
                    if (pooledItems.TryDequeue(out pooledItem))
                    {
                        dequeueItems++;
                        pool.Return(pooledItem);
                    }
                }
            };

            Action Remove = () =>
            {
                Thread.Sleep(40);
                for (var i = 0; i < 500; i++)
                {
                    TestPooledItem pooledItem = null;
                    if (pooledItems.TryDequeue(out pooledItem))
                    {
                        dequeueItems++;
                        pool.Remove(pooledItem);
                    }
                }
            };

            Parallel.Invoke(Get, Return, Remove);

            var expectedItems = 10;
            var actualItems = pool.PooledItemCount;

            Assert.IsTrue(actualItems >= 0 && actualItems <= expectedItems);
        }

        [TestMethod]
        public void ReturnSamePooledItemTwiceTest()
        {
            var pool = new Pool<TestPooledItem>(_logger.Object, () => new TestPooledItem(_logger.Object), 10);
            var pooledItems = new ConcurrentQueue<TestPooledItem>();

            var pooledItem = pool.Get();

            pool.Return(pooledItem);
            pool.Return(pooledItem);

            var expectedItems = 1;
            var actualItems = pool.PooledItemCount;

            Assert.AreEqual(expectedItems, actualItems);
        }
    }
}
