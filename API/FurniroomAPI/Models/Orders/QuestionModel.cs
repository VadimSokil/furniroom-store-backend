using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Models.Orders
{
    public class QuestionModel
    {
        [Required]
        public int? QuestionId { get; set; }
        [Required]
        public string? QuestionDate { get; set; }
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? PhoneNumber { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? QuestionText { get; set; }
    }
}
