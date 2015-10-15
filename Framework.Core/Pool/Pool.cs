using Framework.Logger;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Framework.Core.Pool
{
    /// <summary>
    ///     Pool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Pool<T> : IPool<T> where T : PooledItem
    {
        private readonly ILogger _logger;
        private readonly Func<T> _pooledItemFactory = null;
        private bool _disposed;
        private static object Lock = new object();

        private ConcurrentDictionary<Guid, T> _pooledItems;

        /// <summary>
        ///     Gets or sets the pool size.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        ///     Gets the pooled items count.
        /// </summary>
        public int PooledItemCount
        {
            get { return _pooledItems.Count; }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Pool"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="pooledItemFactory">The pooled item factory delegate that creates a new instance of pooled item.</param>
        /// <param name="poolSize">The pool size. Default is 5.</param>
        public Pool(ILogger logger, Func<T> pooledItemFactory, int poolSize = 5)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (pooledItemFactory == null) throw new ArgumentNullException("pooledItemFactory");

            _logger = logger;
            _pooledItemFactory = pooledItemFactory;
            Size = poolSize;

            _pooledItems = new ConcurrentDictionary<Guid, T>();
        }

        /// <summary>
        ///     Gets a pooled item from the pool
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            RemoveExpiredErroredPooledItems();

            lock (Lock)
            {
                var existingItem = _pooledItems.FirstOrDefault(p => p.Value.Status == PooledItemStatus.Available);

                var availableItems = _pooledItems.Count(p => p.Value.Status == PooledItemStatus.Available);
                var inUseItems = _pooledItems.Count(p => p.Value.Status == PooledItemStatus.InUse);
                var inErrorItems = _pooledItems.Count(p => p.Value.Status == PooledItemStatus.InError);

                if (existingItem.Value != null)
                {
                    var pItem = default(T);

                    pItem = existingItem.Value;

                    try
                    {
                        pItem = _pooledItems.AddOrUpdate(pItem.Guid, pItem,
                                (key, existingVal) =>
                                {
                                    existingVal.Status = PooledItemStatus.InUse;
                                    return existingVal;
                                });

                        _logger.Debug(string.Format("Got existing pooled item from pool. Guid is {0}. Pooled items {1}. Available {2}. InUse {3}. InError {4}."
                            , pItem.Guid, _pooledItems.Count, availableItems - 1, inUseItems + 1, inErrorItems));

                        return pItem;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(string.Format("Failed to AddOrUpdate existing pooled item. Guid is {0}.", pItem.Guid), ex);
                        throw;
                    }
                }
                else
                {
                    var newPooledItem = default(T);

                    if (_pooledItems.Count >= Size) throw new Exception("Maximum pool size limit reached.");
                    newPooledItem = CreatePooledItem();
                    newPooledItem.Status = PooledItemStatus.InUse;

                    try
                    {
                        var pItem = _pooledItems.AddOrUpdate(newPooledItem.Guid, newPooledItem,
                                (key, existingVal) =>
                                {
                                    existingVal.Status = PooledItemStatus.InUse;
                                    return existingVal;
                                });

                        _logger.Info(string.Format("Created new pooled item. Guid is {0}. Pooled items {1}. Available {2}. InUse {3}. InError {4}."
                            , pItem.Guid, _pooledItems.Count, availableItems, inUseItems + 1, inErrorItems));

                        return pItem;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(string.Format("Failed to AddOrUpdate new pooled item. Guid is {0}.", newPooledItem.Guid), ex);
                        throw;
                    }
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
                var pItem = _pooledItems.AddOrUpdate(pooledItem.Guid, pooledItem,
                        (key, existingVal) =>
                        {
                            existingVal.Status = PooledItemStatus.Available;
                            return existingVal;
                        });

                _logger.Debug(string.Format("Returned pooled item back to pool. Guid is {0}. Pooled items {1}.", pItem.Guid, _pooledItems.Count));
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Failed to return pooled item back to pool. Guid is {0}.", pooledItem.Guid), ex);
                throw;
            }

            if (IsExpired(pooledItem))
            {
                Remove(pooledItem);
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
            if (_pooledItems.TryRemove(pooledItem.Guid, out pItem))
            {
                _logger.Debug(string.Format("Removed pooled item from pool. Guid is {0}. Pooled items {1}.", pooledItem.Guid, _pooledItems.Count));
            }
            else
            {
                _logger.Error(string.Format("Failed to remove pooled item. Guid is {0}.", pooledItem.Guid));
                pooledItem.Status = PooledItemStatus.InError;
            }
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

        /// <summary>
        ///     Checks if pooled item is expired.
        /// </summary>
        /// <param name="pooledItem">The pooled item.</param>
        /// <returns></returns>
        private bool IsExpired(T pooledItem)
        {
            lock (Lock)
            {
                if (pooledItem.Status == PooledItemStatus.InUse) return false;

                var timespan = DateTime.Now - pooledItem.CreateDateTime;
                return timespan.TotalSeconds >= pooledItem.LifeTime;
            }
        }

        /// <summary>
        ///     Checks if pooled item is in error.    
        /// </summary>
        /// <param name="pooledItem">The pooled item. </param>
        /// <returns></returns>
        private bool IsErrored(T pooledItem)
        {
            lock (Lock)
            {
                return (pooledItem.Status == PooledItemStatus.InError);
            }
        }

        /// <summary>
        ///     Removes expired pooled items
        /// </summary>
        private void RemoveExpiredErroredPooledItems()
        {
            lock(Lock)
            {
                foreach (var pooledItem in _pooledItems)
                {
                    var pItem = pooledItem.Value;

                    if (IsExpired(pItem) || IsErrored(pItem))
                    {
                        Remove(pItem);    
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
            }

            _pooledItems = null;

            _disposed = true;
        }
    }
}
