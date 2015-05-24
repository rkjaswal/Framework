
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Framework.Repository
{
    public interface IRepository<T> where T : class
    {
        T GetById(object id);

        IQueryable<T> GetAll();

        IQueryable<T> Query(Expression<Func<T, bool>> filter);

        void Add(T entity);

        void Remove(T entity);

        void Attach(T entity);
    }
}
