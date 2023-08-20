using System.ComponentModel.DataAnnotations;

namespace WebAPI_tutorial_recursos.DTOs
{
    /// <summary>
    /// El DTO genérico se usa para devolver (no requiere validaciones)
    /// </summary>
    public class AuthorDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<BookDTO> BookList { get; set; }

    }
}
