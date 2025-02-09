using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Models.Authorization
{
    public class SignInModel
    {
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? PasswordHash { get; set; }
    }
}
