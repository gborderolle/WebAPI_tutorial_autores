namespace WebAPI_tutorial_recursos.Models
{
    /// <summary>
    /// Navegación n..n
    /// </summary>
    public class AuthorBook
    {
        public int AuthorId { get; set; }
        public int BookId { get; set; }
        public int Order { get; set; }
        public Author Author { get; set; } 
        public Book Book { get; set; }

    }
}
