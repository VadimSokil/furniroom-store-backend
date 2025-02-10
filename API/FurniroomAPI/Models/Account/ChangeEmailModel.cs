using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Models.Account
{
    public class ChangeEmailModel
    {
        [Required]
        public string? OldEmail { get; set; }
        [Required]
        public string? NewEmail { get; set; }

    }
}
