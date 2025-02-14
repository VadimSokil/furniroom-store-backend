using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Models.Orders
{
    public class QuestionModel
    {
        [Required]
        public int? QuestionId { get; set; }
        [Required]
        [MaxLength(20)]
        public string? QuestionDate { get; set; }
        [Required]
        [MaxLength(50)]
        public string? UserName { get; set; }
        [Required]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        [Required]
        [MaxLength(254)]
        public string? Email { get; set; }
        [Required]
        [MaxLength(5000)]
        public string? QuestionText { get; set; }
    }
}
