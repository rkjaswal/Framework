using System;

namespace Framework.Core.Pool
{
    public abstract class PooledItem : IPooledItem
    {
        private bool _disposed;
        private DateTime _createDateTime;
        private int _lifeTime;

        public DateTime CreateDateTime
        {
            get { return _createDateTime; }
        }

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
