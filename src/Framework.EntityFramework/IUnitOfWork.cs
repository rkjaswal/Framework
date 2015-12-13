
using System;
using System.Threading.Tasks;

namespace Framework.EntityFramework
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit(bool detachAll = true);

        /// <summary>
        ///     Commits asynchronously
        /// </summary>
        /// <param name="detachAll"></param>
        Task CommitAsync(bool detachAll = true);
        void Detach(object entity);
        void DetachAll();
    }
}
