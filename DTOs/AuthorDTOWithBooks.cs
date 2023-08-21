namespace WebAPI_tutorial_recursos.DTOs
{
    public class AuthorDTOWithBooks : AuthorDTO
    {
        public List<BookDTO> BookList { get; set; }
    }
}
