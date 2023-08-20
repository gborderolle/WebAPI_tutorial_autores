using WebAPI_tutorial_recursos.Models;

namespace WebAPI_tutorial_recursos.Repository.Interfaces
{
    public interface IAuthorRepository : IRepository<Author>
    {
        Task<Author> Update(Author entity);
    }
}
