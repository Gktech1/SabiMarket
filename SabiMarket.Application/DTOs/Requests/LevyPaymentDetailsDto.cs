using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.DTOs.Responses
{
    public class LevyPaymentDetailsDto
    {
        /// <summary>
        /// Unique identifier for the levy payment
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Full name of the trader
        /// </summary>
        public string TraderName { get; set; }

        /// <summary>
        /// Name of the market where payment was collected
        /// </summary>
        public string Market { get; set; }

        /// <summary>
        /// Amount paid
        /// </summary>
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string CollectedBy { get; set; }
        public PaymentStatusEnum Status { get; set; }
    }
}