using System.ComponentModel.DataAnnotations;

namespace FurniroomAPI.Models.Orders
{
    public class OrderModel
    {
        [Required]
        public int? OrderId { get; set; }
        [Required]
        [MaxLength(20)]
        public string? OrderDate { get; set; }
        [Required]
        public int? AccountId { get; set; }
        [Required]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        [Required]
        [MaxLength(100)]
        public string? Country { get; set; }
        [Required]
        [MaxLength(100)]
        public string? Region { get; set; }
        [Required]
        [MaxLength(100)]
        public string? District { get; set; }
        [Required]
        [MaxLength(100)]
        public string? City { get; set; }
        [Required]
        [MaxLength(100)]
        public string? Village { get; set; }
        [Required]
        [MaxLength(100)]
        public string? Street { get; set; }
        [Required]
        [MaxLength(20)]
        public string? HouseNumber { get; set; }
        [Required]
        [MaxLength(20)]
        public string? ApartmentNumber { get; set; }
        [Required]
        [MaxLength(5000)]
        public string? OrderText { get; set; }
        [Required]
        [MaxLength(20)]
        public string? DeliveryType { get; set; }
    }
}
