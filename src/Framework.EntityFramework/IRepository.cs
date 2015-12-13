
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Framework.EntityFramework
{
    public interface IRepository<T> where T : class
    {
        T GetById(object id);

        Task<T> GetByIdAsync(object id);

        IQueryable<T> GetAll();

        Task<IQueryable<T>> GetAllAsync();

        IQueryable<T> Query(Expression<Func<T, bool>> filter);

        void Add(T entity);

        void Remove(T entity);

        void Attach(T entity);
    }
}
