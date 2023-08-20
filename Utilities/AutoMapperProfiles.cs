using AutoMapper;
using WebAPI_tutorial_recursos.Models;
using WebAPI_tutorial_recursos.DTOs;

namespace WebAPI_tutorial_recursos.Utilities
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AuthorDTO, Author>().ReverseMap()
                .ForMember(v => v.BookList, options => options.MapFrom(MapAuthorDTOBooks));

            CreateMap<Author, AuthorCreateDTO>().ReverseMap();
            CreateMap<Author, AuthorUpdateDTO>().ReverseMap();

            //

            CreateMap<BookDTO, Book>().ReverseMap()
                .ForMember(v=>v.AuthorList, options => options.MapFrom(MapBookDTOAuthors));

            CreateMap<Book, BookCreateDTO>().ReverseMap()
                .ForMember(v => v.AuthorBookList, options => options.MapFrom(MapAuthorsBooks));
            
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

        /// <summary>
        /// Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/26946922#notes
        /// </summary>
        /// <param name="bookCreateDTO"></param>
        /// <param name="book"></param>
        /// <returns></returns>
        private List<AuthorDTO> MapBookDTOAuthors(Book book, BookDTO bookDTO)
        {
            var result = new List<AuthorDTO>();
            if (book.AuthorBookList == null)
            {
                return result;
            }
            foreach (var authorBook in book.AuthorBookList)
            {
                result.Add(new AuthorDTO()
                {
                    Id = authorBook.AuthorId,
                    Name = authorBook.Author.Name
                });
            }
            return result;
        }

        private List<BookDTO> MapAuthorDTOBooks(Author author, AuthorDTO authorDTO)
        {
            var result = new List<BookDTO>();
            if (author.AuthorsBooks == null)
            {
                return result;
            }
            foreach (var authorBook in author.AuthorsBooks)
            {
                result.Add(new BookDTO()
                {
                    Id = authorBook.BookId,
                    Title = authorBook.Book.Title
                });
            }
            return result;
        }

    }
}