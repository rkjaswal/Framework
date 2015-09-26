using System;

namespace Framework.Core.Pool
{
    public interface IPool<T> : IDisposable
    {
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
