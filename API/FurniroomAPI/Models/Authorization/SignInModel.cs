using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Models.Authorization
{
    public class SignInModel
    {
        [Required]
        [MaxLength(254)]
        public string? Email { get; set; }
        [Required]
        [MaxLength(128)]
        public string? PasswordHash { get; set; }
    }
}