using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;

namespace Framework.EntityFramework
{
    public abstract class UnitOfWork : IUnitOfWork
    {
        private DbContext _context;
        private bool _disposed;

        protected internal UnitOfWork()
        {
        }

        private ObjectContext ObjectContext
        {
            get
            {
                return ((IObjectContextAdapter) _context).ObjectContext;
            }
        }

        protected internal DbContext Context
        {
            get { return _context; }
        }

        public void Commit(bool detachAll = true)
        {
            CheckContextIsNotNull();

            _context.SaveChanges();

            if (detachAll) DetachAll();
        }

        public async Task CommitAsync(bool detachAll = true)
        {
            CheckContextIsNotNull();

            await _context.SaveChangesAsync();

            if (detachAll) DetachAll();
        }

        public void Detach(object entity)
        {
            if (entity != null) ObjectContext.Detach(entity);
        }

        public void DetachAll()
        {
            var entries = ObjectContext.ObjectStateManager
                .GetObjectStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Modified | EntityState.Unchanged);

            foreach (var entry in entries)
            {
                Detach(entry.Entity);
            }
        }

        protected void SetContext(DbContext context)
        {
            _context = context;
        }

        private void CheckContextIsNotNull()
        {
            if (_context == null) throw new ArgumentNullException("context was not suppied.");
        }

        protected T CreateRepository<T>(ref T repository, Func<DbContext, T> callback)
        {
            CheckContextIsNotNull();
            if (repository == null) repository = callback(_context);

            return repository;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                if (_context != null) _context.Dispose();
            }

            _disposed = true;
        }
    }
}
