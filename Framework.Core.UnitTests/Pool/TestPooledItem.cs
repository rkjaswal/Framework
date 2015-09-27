using Framework.Core.Pool;
using Framework.Logger;
using System;

namespace Framework.Core.UnitTests.Pool
{
    public class TestPooledItem : IPooledItem
    {
        private ILogger _logger;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestPooledItem"/> class.
        /// </summary>
        /// <param name="logger"></param>
        public TestPooledItem(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");

            _logger = logger;
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
                //Release resources here.
            }

            _disposed = true;
        }
    }
}
