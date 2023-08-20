namespace WebAPI_tutorial_recursos.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int BookId { get; set; } // n..0
        public Book Book { get; set; } // n..0 (0=no existe Review sin Book)
    }
}
