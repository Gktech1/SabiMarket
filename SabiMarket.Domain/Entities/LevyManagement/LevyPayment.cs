using System.ComponentModel.DataAnnotations.Schema;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Domain.Entities.LevyManagement
{
    [Table("LevyPayments")]
    public class LevyPayment : BaseEntity
    {
        public Guid TraderId { get; set; }
        public Guid GoodBoyId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public PaymentPeriodEnum Period { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        public string TransactionReference { get; set; }
        public bool HasIncentive { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? IncentiveAmount { get; set; }
        public string QRCodeScanned { get; set; }
        public string Notes { get; set; }
        public virtual GoodBoy GoodBoy { get; set; }
        public virtual Trader Trader { get; set; }

        public DateTime PaymentDate { get; set; }
    }
}
