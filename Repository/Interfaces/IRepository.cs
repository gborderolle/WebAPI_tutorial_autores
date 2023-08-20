using System.Linq.Expressions;

namespace WebAPI_tutorial_recursos.Repository.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task Create(T entity);
        Task<List<T>> GetAll(Expression<Func<T, bool>>? filter = null, params Expression<Func<T, object>>[] includes);
        Task<List<T>> GetAllIncluding(Expression<Func<T, bool>>? filter = null, params Expression<Func<T, object>>[] includes);
        Task<T> Get(Expression<Func<T, bool>>? filter = null, bool tracked = true, params Expression<Func<T, object>>[] includes);
        Task Remove(T entity);
        Task Save();
    }
}
