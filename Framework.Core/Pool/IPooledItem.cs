using System;

namespace Framework.Core.Pool
{
    /// <summary>
    ///     Pool item contract.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPooledItem : IDisposable
    {
        /// <summary>
        ///     Gets the pooled item create datetime.
        /// </summary>
        DateTime CreateDateTime { get; }

        /// <summary>
        ///     Gets the pooled item guid.
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        ///     Gets or sets the pooled item lifetime.
        /// </summary>
        int LifeTime { get; set; }

        /// <summary>
        ///     Gets or sets the pooled item status.
        /// </summary>
        PooledItemStatus Status { get; set; }
    }
}
