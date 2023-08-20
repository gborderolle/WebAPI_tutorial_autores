using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebAPI_tutorial_recursos.Models
{
    public class Book
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres")]
        public string Title { get; set; }

        public List<Review> Reviews { get; set;} // 0..n (0=no existe Review sin Book)

        public List<AuthorBook> AuthorsBooks { get; set; } // n..n

    }
}
