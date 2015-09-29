using Framework.Logger;
using System;
using System.Collections.Concurrent;
using System.Linq;
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
        private bool _disposed;
        private static object Lock = new object();

        private ConcurrentDictionary<Guid, T> _pooledItems { get; set; }

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

            _pooledItems = new ConcurrentDictionary<Guid, T>();
        }

        /// <summary>
        ///     Gets a pooled item from the pool
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            RemoveExpiredPooledItems();

            var existingItem = _pooledItems.FirstOrDefault(p => p.Value.Status == PooledItemStatus.Available);

            var availableItems = _pooledItems.Count(p => p.Value.Status == PooledItemStatus.Available);
            var inUseItems = _pooledItems.Count(p => p.Value.Status == PooledItemStatus.InUse);

            if (existingItem.Value != null)
            {
                var pItem = existingItem.Value;

                _logger.Debug(string.Format("Got existing pooled item from pool. Guid is {0}. Pool size is {1}. Available {2}. InUse {3}."
                    , pItem, _pooledItems.Count, availableItems, inUseItems));
                return _pooledItems.AddOrUpdate(pItem.Guid, pItem,
                        (key, existingVal) =>
                        {
                            existingVal.Status = PooledItemStatus.InUse;
                            return existingVal;
                        });
            }
            else
            {
                var newPooledItem = default(T);

                lock (Lock)
                {
                    if (_pooledItems.Count >= _maxPoolSize) throw new Exception("Maximum pool size limit reached.");
                    newPooledItem = CreatePooledItem();
                    newPooledItem.Status = PooledItemStatus.InUse;
                }

                var pItem = _pooledItems.AddOrUpdate(newPooledItem.Guid, newPooledItem,
                        (key, existingVal) =>
                        {
                            existingVal.Status = PooledItemStatus.InUse;
                            return existingVal;
                        });

                _logger.Debug(string.Format("Created new pooled item. Guid is {0}. Pool size is {1}.", pItem.Guid, _pooledItems.Count));
                return pItem;
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
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Failed to remove expired pooled item. Guid is {0}. Pool size is {1}.", pooledItem.Guid, _pooledItems.Count), ex);
                    throw;
                }
            }
            else
            {
                _pooledItems.AddOrUpdate(pooledItem.Guid, pooledItem,
                        (key, existingVal) =>
                        {
                            existingVal.Status = PooledItemStatus.Available;
                            return existingVal;
                        });

                _logger.Debug(string.Format("Returned pooled item back to pool. Guid is {0}. Pool size is {1}.", pooledItem.Guid, _pooledItems.Count));
            }
        }

        /// <summary>
        ///     Removes a pooled item from the pool.
        /// </summary>
        /// <param name="pooledItem">The pooled item.</param>
        /// <returns></returns>
        public void Remove(T pooledItem)
        {
            var pItem = default(T);
            _pooledItems.TryRemove(pooledItem.Guid, out pItem);
            _logger.Debug(string.Format("Removed pooled item from pool. Guid is {0}. Pool size is {1}.", pooledItem.Guid, _pooledItems.Count));
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
                return _pooledItemFactory();
            }
            catch (Exception ex)
            {
                _logger.Error("Error occured while creating new pooled item.", ex);
                throw;
            }
        }

        private bool IsExpired(T pooledItem)
        {
            lock (Lock)
            {
                var timespan = DateTime.Now - pooledItem.CreateDateTime;
                return timespan.TotalSeconds >= pooledItem.LifeTime;
            }
        }

        /// <summary>
        ///     Removes expired pooled items
        /// </summary>
        private void RemoveExpiredPooledItems()
        {
            lock(Lock)
            {
                foreach (var pooledItem in _pooledItems)
                {
                    var pItem = pooledItem.Value;

                    if (IsExpired(pItem))
                    {
                        try
                        {
                            Remove(pItem);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(string.Format("Failed to remove expired pooled item. Guid is {0}. Pool size is {1}.", pItem, _pooledItems.Count), ex);
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
                foreach(var pooledItem in _pooledItems)
                {
                    Remove(pooledItem.Value);
                }

                _pooledItems = null;
            }

            _disposed = true;
        }
    }
}
