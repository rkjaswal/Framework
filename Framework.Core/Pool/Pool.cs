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
        private static object Lock = new object();

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
            RemoveExpired();

            var pooledItem = default(T);
            
            if (PooledItems.TryDequeue(out pooledItem))
            {
                _logger.Debug(string.Format("Got an existing pooled item. Guid is {0}. Pool size is {1}.", pooledItem.Guid, _poolSize));
                return pooledItem;
            }
            else
            {
                lock (Lock)
                {
                    if (_poolSize >= _maxPoolSize) throw new Exception("Maximum pool size limit reached.");
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
            if (IsExpired(pooledItem))
            {
                try
                {
                    Remove(pooledItem);
                }
                catch(Exception ex)
                {
                    _logger.Error(string.Format("Failed to remove expired pooled item. Guid is {0}. Pool size is {1}.", pooledItem.Guid, _poolSize), ex);
                    throw;
                }
            }
            else
            {
                try
                {
                    PooledItems.Enqueue(pooledItem);
                    _logger.Debug(string.Format("Returned pooled item back to pool. Guid is {0}. Pool size is {1}.", pooledItem.Guid, _poolSize));
                }
                catch(Exception ex)
                {
                    _logger.Error(string.Format("Failed to return pooled item back to pool. Guid is {0}. Pool size is {1}.", pooledItem.Guid, _poolSize), ex);
                    try
                    {
                        Remove(pooledItem);
                    }
                    catch(Exception e)
                    {
                        _logger.Error(string.Format("Failed to remove pooled item. Guid is {0}. Pool size is {1}.", pooledItem.Guid, _poolSize), e);
                        throw;
                    }
                }
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
            _logger.Debug(string.Format("Removed pooled item from pool. Guid is {0}. Pool size is {1}.", pooledItem.Guid, _poolSize));
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
                var pooledItem = _pooledItemFactory();
                Interlocked.Increment(ref _poolSize);
                _logger.Debug(string.Format("Created new pooled item. Guid is {0}. Pool size is {1}.", pooledItem.Guid, _poolSize));
                return pooledItem;
            }
            catch (Exception ex)
            {
                _logger.Error("Error occured while creating new pooled item.", ex);
                throw;
            }
        }

        private bool IsExpired(T pooledItem)
        {
            var timespan = DateTime.Now - pooledItem.CreateDateTime;
            return timespan.Minutes >= pooledItem.LifeTime; 
        }

        /// <summary>
        ///     Removes expired pooled items
        /// </summary>
        private void RemoveExpired()
        {
            lock(Lock)
            {
                foreach (var pooledItem in PooledItems)
                {
                    if (IsExpired(pooledItem))
                    {
                        try
                        {
                            Remove(pooledItem);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(string.Format("Failed to remove expired pooled item. Guid is {0}. Pool size is {1}.", pooledItem.Guid, _poolSize), ex);
                            throw;
                        }
                    }
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
