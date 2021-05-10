using BulkyBook.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace BulkyBook.DataAccess.Repository
{
    // Class that implements IRepository
    public class Repository<T> : IRepository<T> where T : class
    {
        // Get dbContext
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public T Get(int id)
        {
            return dbSet.Find(id);
        }

        // to pass filter - o => o.OrderId == id, 
        // to pass orderBy - orderBy: query => query.OrderByDescending(o => o.Name)
        // to pass includeProperites - includeProperties: "Product"
        // GetAll(o => o.OrderId == id, query => query.OrderByDescending(o => o.Name), includeProperties: "Product")

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = null)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            // for eager loading to include subcategories
            if (includeProperties != null)
            {
                // pass table names for Include in a string separated by , (includeProperties)
                // get those table names by Spliting a string in foreach by splitting it in a char[]                
                foreach (var includeProp in includeProperties.Split(new char[] { ','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    // include in query, one by one
                    query = query.Include(includeProp);
                }
            }

            // orderBy
            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }

            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter = null, string includeProperties = null)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            // for eager loading to include subcategories
            if (includeProperties != null)
            {
                // pass table names for Include in a string separated by , (includeProperties)
                // get those table names by Spliting a string in foreach by splitting it in a char[]                
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    // include in query, one by one
                    query = query.Include(includeProp);
                }
            }          
            return query.FirstOrDefault();
        }

        public void Remove(int id)
        {
            T entity = dbSet.Find(id);
            Remove(entity);
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }
    }
}
