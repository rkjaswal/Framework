using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.Caching
{
    public class MemoryCacheProvider : ICacheProvider, IDisposable
    {
        private readonly MemoryCache _cache;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MemoryCacheProvider"/> class.
        /// </summary>
        public MemoryCacheProvider(string name)
        {
            _cache = new MemoryCache(name);
        }

        /// <summary>
        ///     Adds the specified key.    
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        public void Add(string key, object item)
        {
            _cache.Add(key, item, null);
        }

        /// <summary>
        ///     Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <param name="duration">The duration in milliseconds.</param>
        public void Add(string key, object item, int duration)
        {
            _cache.Add(key, item, new DateTimeOffset(DateTime.UtcNow.AddMilliseconds(duration)));
        }

        /// <summary>
        ///     Gets the specified key.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public T Get<T>(string key) where T : class
        {
            return _cache[key] as T;
        }

        /// <summary>
        ///     Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        /// <summary>
        ///     Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Dispose.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources;
        ///     <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _cache.Dispose();
            }

            _disposed = true; 
        }
    }
}
