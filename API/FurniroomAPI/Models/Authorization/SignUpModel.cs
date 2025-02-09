using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Models.Authorization
{
    public class SignUpModel
    {
        [Required]
        public int? AccountId { get; set; }
        [Required]
        public string? AccountName { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? PasswordHash { get; set; }

    }
}
