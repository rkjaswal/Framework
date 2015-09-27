using System;

namespace Framework.Core.Pool
{
    /// <summary>
    ///     Pooled Item
    /// </summary>
    public abstract class PooledItem : IPooledItem
    {
        private DateTime _createDateTime;
        private Guid _guid;
        private int _lifeTime;
        private bool _disposed;

        /// <summary>
        ///     Gets the pooled item create datetime.
        /// </summary>
        public DateTime CreateDateTime
        {
            get { return _createDateTime; }
        }

        /// <summary>
        ///     Gets the pooled item guid
        /// </summary>
        public Guid Guid
        {
            get { return _guid; }
        }

        /// <summary>
        ///     Gets or sets the pooled item lifetime
        /// </summary>
        public int LifeTime
        {
            get { return _lifeTime; }
            set { _lifeTime = value; }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PooledItem"/> class.
        /// </summary>
        /// <param name="lifeTime">Lifetime in minutes. Default is 10 minutes</param>
        public PooledItem(int lifeTime = 10)
        {
            _createDateTime = DateTime.Now;
            _guid = Guid.NewGuid();
            _lifeTime = lifeTime;
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
            }

            _disposed = true;
        }
    }
}
