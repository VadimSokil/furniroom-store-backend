using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Models.Account
{
    public class ChangeNameModel
    {
        [Required]
        public string? OldName { get; set; }
        [Required]
        public string? NewName { get; set; }

    }
}
