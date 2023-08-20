using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebAPI_tutorial_recursos.Models;

namespace WebAPI_tutorial_recursos.DTOs
{
    /// <summary>
    /// El DTO genérico se usa para devolver (no requiere validaciones)
    /// </summary>
    public class BookDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres")]
        public string Title { get; set; }

        public List<ReviewDTO> Reviews { get; set; } // 0..n (0=no existe Review sin Book)

    }
}
