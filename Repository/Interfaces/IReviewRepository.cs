using WebAPI_tutorial_recursos.Models;

namespace WebAPI_tutorial_recursos.Repository.Interfaces
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<Review> Update(Review entity);
    }
}
