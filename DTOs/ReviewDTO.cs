using System.ComponentModel.DataAnnotations;
using WebAPI_tutorial_recursos.Models;

namespace WebAPI_tutorial_recursos.DTOs
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public string Content { get; set; }

    }
}
