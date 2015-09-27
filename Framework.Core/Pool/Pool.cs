using Framework.Logger;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Framework.Core.Pool
{
    /// <summary>
    ///     Pool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Pool<T> : IPool<T> where T : PooledItem
    {
        private readonly ILogger _logger;
        private readonly int _maxPoolSize;
        private readonly Func<T> _pooledItemFactory = null;
        private int _poolSize;
        private bool _disposed;
        private static object _lock = new object();

        private ConcurrentQueue<T> PooledItems { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Pool"/> class.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="pooledItemFactory"></param>
        /// <param name="maxPoolSize"></param>
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
                _logger.Debug(string.Format("Got an existing pooled item. Guid is {0}, Pool size is {1}", pooledItem.Guid, _poolSize));
                Interlocked.Decrement(ref _poolSize);
                return pooledItem;
            }
            else
            {
                lock (_lock)
                {
                    if (_poolSize >= _maxPoolSize) throw new Exception("Maximum pool size limit reached");
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
                if (IsPooledItemExpired(pooledItem))
                {
                    Remove(pooledItem);
                }
                else
                {
                    PooledItems.Enqueue(pooledItem);
                    _logger.Debug(string.Format("Returned pooled item back to pool. Guid is {0}, Pool size is {1}", pooledItem.Guid, _poolSize));
                }
            }
            catch
            {
                _logger.Error(string.Format("Failed to return pooled item back to pool. Guid is {0}, Pool size is {1}", pooledItem.Guid, _poolSize));
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
            _logger.Debug(string.Format("Removed pooled item from pool. Guid is {0}, Pool size is {1}", pooledItem.Guid, _poolSize));
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
                var pooledItem = (T)Activator.CreateInstance(typeof(T), _logger);
                Interlocked.Increment(ref _poolSize);
                _logger.Debug(string.Format("Created new pooled item. Guid is {0}, Pool size is {1}", pooledItem.Guid, _poolSize));
                return pooledItem;
            }
            catch
            {
                throw;
            }
        }

        private bool IsPooledItemExpired(T pooledItem)
        {
            var timespan = DateTime.Now - pooledItem.CreateDateTime;
            return timespan.Minutes >= pooledItem.LifeTime; 
        }

        /// <summary>
        ///     Removes expired pooled items
        /// </summary>
        private void RemoveExpiredPooledItems()
        {
            foreach(var pooledItem in PooledItems)
            {
                if (IsPooledItemExpired(pooledItem))
                {
                    Remove(pooledItem);
                }
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
