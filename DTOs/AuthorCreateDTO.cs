using System.ComponentModel.DataAnnotations;
using WebAPI_tutorial_recursos.Validations;

namespace WebAPI_tutorial_recursos.DTOs
{
    public class AuthorCreateDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres")]
        [FirstCharCapitalValidation]
        public string Name { get; set; }

    }
}
