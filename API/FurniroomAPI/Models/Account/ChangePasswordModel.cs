using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Models.Account
{
    public class ChangePasswordModel
    {
        [Required]
        [MaxLength(128)]
        public string? OldPasswordHash { get; set; }
        [Required]
        [MaxLength(128)]
        public string? NewPasswordHash { get; set; }

    }
}
