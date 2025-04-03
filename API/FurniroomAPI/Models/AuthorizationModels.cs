using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Models
{
    public class SignInModel
    {
        [Required(ErrorMessage = "Email is required")]
        [MaxLength(254, ErrorMessage = "Email cannot exceed 254 characters")]
        [EmailAddress(ErrorMessage = "Email should be in format: example@domain.com")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MaxLength(128, ErrorMessage = "Password hash cannot exceed 128 characters")]
        public string? PasswordHash { get; set; }
    }

    public class SignUpModel
    {
        [Required(ErrorMessage = "Account ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Account ID must be a positive number")]
        public int? AccountId { get; set; }

        [Required(ErrorMessage = "Account name is required")]
        [MaxLength(50, ErrorMessage = "Account name cannot exceed 50 characters")]
        public string? AccountName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [MaxLength(254, ErrorMessage = "Email cannot exceed 254 characters")]
        [EmailAddress(ErrorMessage = "Email should be in format: example@domain.com")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MaxLength(128, ErrorMessage = "Password hash cannot exceed 128 characters")]
        public string? PasswordHash { get; set; }
    }

    public class EmailModel
    {
        [Required(ErrorMessage = "Email is required")]
        [MaxLength(254, ErrorMessage = "Email cannot exceed 254 characters")]
        [EmailAddress(ErrorMessage = "Email should be in format: example@domain.com")]
        public string? Email { get; set; }
    }
}
