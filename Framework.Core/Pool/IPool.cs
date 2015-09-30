using System;

namespace Framework.Core.Pool
{
    public interface IPool<T> : IDisposable
    {
        /// <summary>
        ///     Gets or sets the pool size.
        /// </summary>
        int Size { get; set; }

        /// <summary>
        ///     Gets the pooled items count.
        /// </summary>
        int PooledItemCount { get; }

        /// <summary>
        ///     Gets a pooled item from the pool.
        /// </summary>
        /// <returns></returns>
        T Get();

        /// <summary>
        ///     Returns a pooled item back to pool.
        /// </summary>
        /// <param name="pooledItem">The pooled item.</param>
        void Return(T pooledItem);

        /// <summary>
        ///     Removes a pooled item from the pool.
        /// </summary>
        /// <param name="pooledItem">The pooled item.</param>
        /// <returns></returns>
        void Remove(T pooledItem);
    }
}
