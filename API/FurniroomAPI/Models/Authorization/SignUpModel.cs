using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Models.Authorization
{
    public class SignUpModel
    {
        [Required]
        public int? AccountId { get; set; }
        [Required]
        [MaxLength(50)]
        public string? AccountName { get; set; }
        [Required]
        [MaxLength(254)]
        public string? Email { get; set; }
        [Required]
        [MaxLength(128)]
        public string? PasswordHash { get; set; }

    }
}