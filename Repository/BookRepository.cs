using Microsoft.EntityFrameworkCore;
using WebAPI_tutorial_recursos.Context;
using WebAPI_tutorial_recursos.Models;
using WebAPI_tutorial_recursos.Repository.Interfaces;

namespace WebAPI_tutorial_recursos.Repository
{
    public class BookRepository : Repository<Book>, IBookRepository
    {
        /// <summary>
        /// Igual a la capa Services, pero éste hereda de interfaces (mejor)
        /// 
        /// En program.cs:
        /// builder.Services.AddScoped<IVillaRepository, VillaRepository>();
        /// </summary>
        private readonly DbContext _dbContext;

        public BookRepository(ContextDB dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Book> Update(Book entity)
        {
            //entity.Update = DateTime.Now;
            _dbContext.Update(entity);
            await Save();
            return entity;
        }
    }
}
