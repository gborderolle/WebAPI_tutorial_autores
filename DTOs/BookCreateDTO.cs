using WebAPI_tutorial_recursos.Validations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebAPI_tutorial_recursos.DTOs
{
    public class BookCreateDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres")]
        [FirstCharCapitalValidation]
        public string Title { get; set; }

        public List<int> AuthorsIds { get; set; } // relación n..n

    }
}

