
using System;

namespace Framework.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit(bool detachAll = true);
        void Detach(object entity);
        void DetachAll();
    }
}
