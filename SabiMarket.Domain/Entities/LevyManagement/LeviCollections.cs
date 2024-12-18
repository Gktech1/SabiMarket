using System.ComponentModel.DataAnnotations.Schema;
using SabiMarket.Domain.Entities.MarketParticipants;

namespace SabiMarket.Domain.Entities.LevyManagement
{
    [Table("LevyCollections")]
    public class LevyCollections : BaseEntity
    {
        public Guid GoodBoyId { get; set; }
        public Guid TraderId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime CollectionDate { get; set; }
        public string QRCodeScanned { get; set; }
        public string Notes { get; set; }

        public virtual GoodBoy GoodBoy { get; set; }
        public virtual Trader Trader { get; set; }
    }

}
