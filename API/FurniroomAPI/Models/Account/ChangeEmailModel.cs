using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Models.Account
{
    public class ChangeEmailModel
    {
        [Required]
        public int? AccountId { get; set; }
        [Required]
        [MaxLength(254)]
        public string? OldEmail { get; set; }
        [Required]
        [MaxLength(254)]
        public string? NewEmail { get; set; }

    }
}
