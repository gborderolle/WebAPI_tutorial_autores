using Microsoft.EntityFrameworkCore;
using WebAPI_tutorial_recursos.Context;
using WebAPI_tutorial_recursos.Models;
using WebAPI_tutorial_recursos.Repository.Interfaces;

namespace WebAPI_tutorial_recursos.Repository
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        private readonly DbContext _dbContext;

        public ReviewRepository(ContextDB dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Review> Update(Review entity)
        {
            _dbContext.Update(entity);
            await Save();
            return entity;
        }
    }
}
