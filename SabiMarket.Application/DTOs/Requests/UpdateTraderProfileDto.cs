using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Requests
{
    public class UpdateTraderProfileDto
    {
        [Required]
        [StringLength(100)]
        public string BusinessName { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        public string? SectionId { get; set; }

        [StringLength(500)]
        public string? BusinessDescription { get; set; }
    }

    public class CreateTraderRequestDto
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string MarketId { get; set; }

        [Required]
        public string CaretakerId { get; set; }

        [Required]
        [StringLength(100)]
        public string BusinessName { get; set; }

        [Required]
        public string BusinessType { get; set; }
    }

    public class TraderFilterRequestDto
    {
        public string? MarketId { get; set; }
        public string? CaretakerId { get; set; }
        public string? BusinessType { get; set; }
        public string? SearchTerm { get; set; }
    }
}
