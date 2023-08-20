using System.ComponentModel.DataAnnotations;

namespace WebAPI_tutorial_recursos.DTOs
{
    public class AuthorUpdateDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; }

        public DateTime Creation { get; set; }

        public DateTime Update { get; set; }
    }
}