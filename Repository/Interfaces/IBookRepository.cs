using WebAPI_tutorial_recursos.Models;

namespace WebAPI_tutorial_recursos.Repository.Interfaces
{
    public interface IBookRepository : IRepository<Book>
    {
        Task<Book> Update(Book entity);
    }
}
