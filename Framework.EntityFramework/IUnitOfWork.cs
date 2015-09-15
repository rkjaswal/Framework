
using System;

namespace Framework.EntityFramework
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit(bool detachAll = true);
        void Detach(object entity);
        void DetachAll();
    }
}
