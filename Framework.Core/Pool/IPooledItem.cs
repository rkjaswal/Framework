using System;

namespace Framework.Core.Pool
{
    /// <summary>
    ///     Pool item contract.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPooledItem : IDisposable
    {
    }
}
