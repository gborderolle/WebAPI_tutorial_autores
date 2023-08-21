using System.ComponentModel.DataAnnotations;
using WebAPI_tutorial_recursos.Utilities.HATEOAS;

namespace WebAPI_tutorial_recursos.DTOs
{
    /// <summary>
    /// El DTO genérico se usa para devolver (no requiere validaciones)
    /// </summary>
    public class AuthorDTO : Resource
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }
}
