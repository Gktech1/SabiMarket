using SabiMarket.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Requests
{
    public class UpdateLevyPaymentDto
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment status is required")]
        public PaymentStatusEnum Status { get; set; }

        // Optional properties that might be useful
        public string? UpdateReason { get; set; }

        public string? UpdatedBy { get; set; }
    }

}
