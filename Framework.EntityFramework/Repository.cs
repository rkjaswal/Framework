﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Framework.EntityFramework
{
    public abstract class Repository<T> : IRepository<T> where T : class
    {
        private readonly DbSet<T> _entitySet;

        protected internal Repository(DbContext context)
        {
            if (context == null) throw new ArgumentNullException("No context was supplied");

            _entitySet = context.Set<T>();
        }

        public DbSet<T> EntitySet
        {
            get { return _entitySet; }
        }

        public virtual T GetById(object id)
        {
            return _entitySet.Find(id);
        }

        public virtual async Task<T> GetByIdAsync(object id)
        {
            return await _entitySet.FindAsync(id);
        }

        public IQueryable<T> GetAll()
        {
            return _entitySet;
        }

        public async Task<IQueryable<T>> GetAllAsync()
        {
            var result = await _entitySet.ToListAsync();
            return result.AsQueryable();
        }

        public virtual IQueryable<T> Query(Expression<Func<T, bool>> filter)
        {
            return _entitySet.Where(filter);
        }

        public void Add(T entity)
        {
            ThrowExceptionIfEntityNull(entity);
            _entitySet.Add(entity);
        }

        public void Remove(T entity)
        {
            ThrowExceptionIfEntityNull(entity);
            _entitySet.Remove(entity);
        }

        public void Attach(T entity)
        {
            ThrowExceptionIfEntityNull(entity);
            _entitySet.Attach(entity);
        }

        private static void ThrowExceptionIfEntityNull(T entity)
        {
            if (entity == null) throw new ArgumentNullException("No entity was supplied.");
        }
    }
}
