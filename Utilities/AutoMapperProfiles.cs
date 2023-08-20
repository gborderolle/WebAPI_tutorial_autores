using AutoMapper;
using WebAPI_tutorial_recursos.Models;
using WebAPI_tutorial_recursos.DTOs;

namespace WebAPI_tutorial_recursos.Utilities
{
    public class AutoMapperProfiles : Profile
    {       
        public AutoMapperProfiles()
        {
            CreateMap<Author, AuthorDTO>().ReverseMap();
            CreateMap<Author, AuthorCreateDTO>().ReverseMap();
            CreateMap<Author, AuthorUpdateDTO>().ReverseMap();

            //

            CreateMap<Book, BookDTO>().ReverseMap();
            CreateMap<Book, BookCreateDTO>().ReverseMap()
                .ForMember(v => v.AuthorsBooks, options => options.MapFrom(MapAuthorsBooks));
            CreateMap<Book, BookUpdateDTO>().ReverseMap();

            //

            CreateMap<Review, ReviewDTO>().ReverseMap();
            CreateMap<Review, ReviewCreateDTO>().ReverseMap();
            CreateMap<Review, ReviewUpdateDTO>().ReverseMap();

        }
        /// <summary>
        /// Para la relación n..n
        /// Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/26946920#notes
        /// </summary>
        /// <param name="bookCreateDTO"></param>
        /// <param name="book"></param>
        /// <returns></returns>
        private List<AuthorBook> MapAuthorsBooks(BookCreateDTO bookCreateDTO, Book book)
        {
            var result = new List<AuthorBook>();
            if (bookCreateDTO.AuthorsIds == null)
            {
                return result;
            }
            foreach (var authorId in bookCreateDTO.AuthorsIds)
            {
                result.Add(new AuthorBook() { AuthorId = authorId });
            }
            return result;
        }
    }
}