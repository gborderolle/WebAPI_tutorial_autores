using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;
using WebAPI_tutorial_recursos.Context;
using WebAPI_tutorial_recursos.Repository.Interfaces;
using WebAPI_tutorial_recursos.Utilities;
using WebAPI_tutorial_recursos.DTOs;

namespace WebAPI_tutorial_recursos.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ContextDB _dbContext;
        internal DbSet<T> dbSet;

        public Repository(ContextDB db)
        {
            _dbContext = db;
            this.dbSet = _dbContext.Set<T>();
        }

        public async Task Create(T entity)
        {
            await dbSet.AddAsync(entity);
            await Save();
        }

        public async Task<T> Get(Expression<Func<T, bool>>? filter = null, bool tracked = true, IEnumerable<IncludePropertyConfiguration<T>> includes = null, IEnumerable<ThenIncludePropertyConfiguration<T>> thenIncludes = null)
        {
            IQueryable<T> query = dbSet;
            if (includes != null)
            {
                foreach (var includeConfig in includes)
                {
                    query = query.Include(includeConfig.IncludeExpression);
                }
            }

            if (thenIncludes != null)
            {
                foreach (var thenIncludeConfig in thenIncludes)
                {
                    query = query.Include(thenIncludeConfig.IncludeExpression).ThenInclude(thenIncludeConfig.ThenIncludeExpression);
                }
            }

            if (!tracked)
            {
                query = query.AsNoTracking();
            }
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<T>> GetAll(Expression<Func<T, bool>>? filter = null, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = dbSet;
            if (includes != null)
            {
                foreach (var includeProperty in includes)
                {
                    query = query.Include(includeProperty);
                }
            }
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.ToListAsync();
        }

        public async Task<List<T>> GetAllIncluding(Expression<Func<T, bool>>? filter = null, Expression<Func<T, object>>? orderBy = null, bool ascendingOrder = true, PaginationDTO paginationDTO = null, HttpContext httpContext = null, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = dbSet;
            foreach (var includeProperty in includes)
            {
                query = query.Include(includeProperty);
            }
            if (orderBy != null)
            {
                query = ascendingOrder ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            }
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (httpContext != null && paginationDTO != null)
            {
                await httpContext.InsertParamPaginationHeader(query);
                query = query.DoPagination(paginationDTO);
            }
            return await query.ToListAsync();
        }

        public async Task Remove(T entity)
        {
            dbSet.Remove(entity);
            await Save();
        }

        public async Task Save()
        {
            await _dbContext.SaveChangesAsync();
        }

    }
}

public class IncludePropertyConfiguration<T>
{
    public Expression<Func<T, object>> IncludeExpression { get; set; }
    public Expression<Func<object, object>> ThenIncludeExpression { get; set; }
}

public class ThenIncludePropertyConfiguration<T>
{
    public Expression<Func<T, object>> IncludeExpression { get; set; }
    public Expression<Func<object, object>> ThenIncludeExpression { get; set; }
}