using System.ComponentModel.DataAnnotations;

namespace WebAPI_tutorial_recursos.DTOs
{
    public class BookUpdateDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Title { get; set; }

    }
}