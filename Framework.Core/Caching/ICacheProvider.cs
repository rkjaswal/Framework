namespace Framework.Core.Caching
{
    public interface ICacheProvider
    {
        /// <summary>
        ///     Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        void Add(string key, object item);

        /// <summary>
        ///     Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <param name="duration">The duration.</param>
        void Add(string key, object item, int duration);

        /// <summary>
        ///     Gets the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        T Get<T>(string key) where T : class;

        /// <summary>
        ///     Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        void Remove(string key);
    }
}
