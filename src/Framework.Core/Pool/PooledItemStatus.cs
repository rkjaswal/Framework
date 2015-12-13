namespace Framework.Core.Pool
{
    /// <summary>
    ///     Pooled item status
    /// </summary>
    public enum PooledItemStatus
    {
        /// <summary>
        ///     Available
        /// </summary>
        Available,
        /// <summary>
        ///     In use.
        /// </summary>
        InUse,
        /// <summary>
        ///     In error.
        /// </summary>
        InError
    }
}
