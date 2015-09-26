using Framework.Logger;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Framework.Core.Pool
{
    public class Pool<T> : IPool<T> where T : IPooledItem
    {
        private readonly ILogger _logger;
        private readonly int _maxPoolSize;
        private readonly Func<T> _pooledItemFactory = null;
        private int _poolSize;
        private bool _disposed;
        private static object _lock = new object();

        private ConcurrentQueue<T> PooledItems { get; set; }

        public Pool(ILogger logger, Func<T> pooledItemFactory, int maxPoolSize = 5)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (pooledItemFactory == null) throw new ArgumentNullException("pooledItemFactory");

            _logger = logger;
            _pooledItemFactory = pooledItemFactory;
            _maxPoolSize = maxPoolSize;

            PooledItems = new ConcurrentQueue<T>();
        }

        /// <summary>
        ///     Gets a pooled item from the pool
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            T pooledItem = default(T);
            if (PooledItems.TryDequeue(out pooledItem))
            {
                _logger.Info("Getting an existing pooled item.");
                Interlocked.Decrement(ref _poolSize);
                return pooledItem;
            }
            else
            {
                lock (_lock)
                {
                    if (_poolSize >= _maxPoolSize) throw new Exception("Maximum pool size limit reached");
                    _logger.Info("Creating new pooled item.");
                    return CreatePooledItem();
                }
            }
        }

        /// <summary>
        ///     Returns a pooled item back to pool
        /// </summary>
        /// <param name="item">The pooled item.</param>
        public void Return(T pooledItem)
        {
            try
            {
                PooledItems.Enqueue(pooledItem);
                Interlocked.Increment(ref _poolSize);
                _logger.Info("Successfully returning pooled item back to pool.");
            }
            catch
            {
                _logger.Info("Failed to return pooled item back to pool.");
                Interlocked.Decrement(ref _poolSize);
                pooledItem.Dispose();
            }
        }

        /// <summary>
        ///     Removes a pooled item from the pool.
        /// </summary>
        /// <param name="pooledItem">The pooled item.</param>
        /// <returns></returns>
        public void Remove(T pooledItem)
        {
            Interlocked.Decrement(ref _poolSize);
            pooledItem.Dispose();
        }

        /// <summary>
        ///     Created a new instance of pooled item
        /// </summary>
        /// <returns></returns>
        private T CreatePooledItem()
        {
            try
            {
                Interlocked.Increment(ref _poolSize);
                return (T)Activator.CreateInstance(typeof(T));
            }
            catch
            {
                Interlocked.Decrement(ref _poolSize);
                throw;
            }
        }

        /// <summary>
        ///  Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        /// <param name="disposing">The disposing flag.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                foreach(var pooledItem in PooledItems)
                {
                    Remove(pooledItem);
                }

                PooledItems = null;
            }

            _disposed = true;
        }
    }
}
