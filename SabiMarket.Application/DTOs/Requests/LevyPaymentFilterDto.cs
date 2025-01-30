using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.DTOs.Requests
{
    public class LevyPaymentFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public PaymentStatusEnum? Status { get; set; }
    }
}
