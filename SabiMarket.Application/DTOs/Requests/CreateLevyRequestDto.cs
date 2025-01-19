using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.DTOs.Requests
{
    public class CreateLevyRequestDto
    {
        public string ChairmanId { get; set; } // ID of the chairman
        public string TraderId { get; set; } // ID of the trader making the payment
        public decimal Amount { get; set; } // Payment amount
        public PaymentPeriodEnum Period { get; set; } // Payment period
        public PaymentMethodEnum PaymentMethod { get; set; } // Payment method
        public string TransactionReference { get; set; } // Transaction reference
        public bool HasIncentive { get; set; } // Whether an incentive is included
        public decimal? IncentiveAmount { get; set; } // Incentive amount, if any
        public DateTime PaymentDate { get; set; } // Payment date
        public string Notes { get; set; } // Notes about the payment
        public string GoodBoyId { get; set; } // ID of the collector
        public DateTime CollectionDate { get; set; } // Collection date
        public string QRCodeScanned { get; set; } // Scanned QR code data
    }


}
