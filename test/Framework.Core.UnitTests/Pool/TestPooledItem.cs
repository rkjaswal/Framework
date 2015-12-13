using Framework.Core.Pool;
using Framework.Logger;
using System;

namespace Framework.Core.UnitTests.Pool
{
    public class TestPooledItem : PooledItem
    {
        private ILogger _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestPooledItem"/> class.
        /// </summary>
        /// <param name="logger"></param>
        public TestPooledItem(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");

            _logger = logger;

            LifeTime = 60;
        }

        /// <summary>
        ///     Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            //Release resources here
        }
    }
}
