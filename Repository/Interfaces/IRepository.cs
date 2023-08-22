using System.Linq.Expressions;
using WebAPI_tutorial_recursos.DTOs;

namespace WebAPI_tutorial_recursos.Repository.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task Create(T entity);
        Task<List<T>> GetAll(Expression<Func<T, bool>>? filter = null, params Expression<Func<T, object>>[] includes);
        Task<List<T>> GetAllIncluding(Expression<Func<T, bool>>? filter = null, Expression<Func<T, object>>? orderBy = null, bool ascendingOrder = true, PaginationDTO paginationDTO = null, HttpContext httpContext = null, params Expression<Func<T, object>>[] includes);
        Task<T> Get(Expression<Func<T, bool>>? filter = null, bool tracked = true, IEnumerable<IncludePropertyConfiguration<T>> includes = null, IEnumerable<ThenIncludePropertyConfiguration<T>> thenIncludes = null);
        Task Remove(T entity);
        Task Save();
    }
}
