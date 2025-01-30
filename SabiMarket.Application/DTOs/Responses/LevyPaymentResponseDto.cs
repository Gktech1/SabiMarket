using SabiMarket.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Responses
{
    public class LevyPaymentResponseDto
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? TraderName { get; set; }
        public string? CollectedBy { get; set; }    
        public PaymentStatusEnum Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

