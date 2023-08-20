using System.ComponentModel.DataAnnotations;
using WebAPI_tutorial_recursos.Models;
using WebAPI_tutorial_recursos.Validations;

namespace WebAPI_tutorial_recursos.DTOs
{
    public class ReviewUpdateDTO
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres")]
        public string Content { get; set; }
        public int BookId { get; set; } // n..0
        public Book Book { get; set; } // n..0 (0=no existe Review sin Book)

    }
}
