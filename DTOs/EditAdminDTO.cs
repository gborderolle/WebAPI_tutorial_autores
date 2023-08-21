using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI_tutorial_recursos.DTOs
{
    public class EditAdminDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
