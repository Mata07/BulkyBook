using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    // Create generic repository of unknown type <T>, where T will be class
    public interface IRepository<T> where T : class
    {
        // Based on id retrieve category from database
        T Get(int id);

        // List of categories based on number of requirements
        // 1.param using Expression<T> to be generic
        // Func<input, output type>, filter = if it's null get all
        // 2. param - if we want orderBy
        // 3. includeProperties - for eager loading when we have foreign key references
        IEnumerable<T> GetAll(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null
            );

        // Same thing as Get
        T GetFirstOrDefault(
            Expression<Func<T, bool>> filter = null,            
            string includeProperties = null
            );

        // Add entity
        void Add(T entity);

        // Remove methods - does not need all of them, just for example
        // Remove by id
        void Remove(int id);

        // Remove by complete entitx
        void Remove(T entity);

        // Remove Range of entites
        void RemoveRange(IEnumerable<T> entity);

        // Save in UnitOfWork
    }
}
