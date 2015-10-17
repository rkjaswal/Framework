using System;
using System.Web.Caching;
using System.Web;

namespace Framework.Core.Caching
{
    public class AspNetCacheProvider : ICacheProvider
    {
        private readonly Cache _cache;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AspNetCacheProvider"/> class.
        /// </summary>
        public AspNetCacheProvider()
        {
            _cache = HttpRuntime.Cache;
        }

        /// <summary>
        ///     Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        public void Add(string key, object item)
        {
            _cache.Insert(key, item);
        }

        /// <summary>
        ///     Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <param name="duration">The duration.</param>
        public void Add(string key, object item, int duration)
        {
            _cache.Insert(key, item, null, DateTime.UtcNow.AddMilliseconds(duration), Cache.NoSlidingExpiration);
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
    }
}
