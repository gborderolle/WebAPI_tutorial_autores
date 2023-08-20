using System.ComponentModel.DataAnnotations;
using WebAPI_tutorial_recursos.Models;
using WebAPI_tutorial_recursos.Validations;

namespace WebAPI_tutorial_recursos.DTOs
{
    public class ReviewCreateDTO
    {
        public string Content { get; set; }
    }
}
