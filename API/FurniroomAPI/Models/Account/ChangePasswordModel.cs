using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Models.Account
{
    public class ChangePasswordModel
    {
        [Required]
        public string? OldPasswordHash { get; set; }
        [Required]
        public string? NewPasswordHash { get; set; }

    }
}
