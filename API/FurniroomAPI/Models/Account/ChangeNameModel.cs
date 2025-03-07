﻿using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Models.Account
{
    public class ChangeNameModel
    {
        [Required]
        [MaxLength(50)]
        public string? OldName { get; set; }
        [Required]
        [MaxLength(50)]
        public string? NewName { get; set; }

    }
}
