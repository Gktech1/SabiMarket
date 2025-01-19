using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.DTOs.Requests
{
    public class UpdateLevyRequestDto
    {
        public decimal Amount { get; set; } // Updated payment amount
        public PaymentPeriodEnum? Period { get; set; } // Updated payment period
        public PaymentMethodEnum? PaymentMethod { get; set; } // Updated payment method
        public PaymentStatusEnum? PaymentStatus { get; set; } // Updated payment status
        public bool HasIncentive { get; set; } // Whether an incentive is included
        public decimal? IncentiveAmount { get; set; } // Updated incentive amount
        public string Notes { get; set; } // Updated notes
    }


}
