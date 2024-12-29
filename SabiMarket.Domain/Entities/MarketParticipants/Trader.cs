using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities.MarketParticipants
{
    [Table("Traders")]
    public class Trader : BaseEntity
    {
        public string UserId { get; set; }
        public string MarketId { get; set; }
        public string? SectionId { get; set; }
        public string CaretakerId { get; set; }

        [Required]
        [StringLength(50)]
        public string? TIN { get; set; }

        [Required]
        public string? BusinessName { get; set; }
        public string BusinessType { get; set; }
        public string? QRCode { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Market Market { get; set; }
        public virtual MarketSection Section { get; set; }
        public virtual Caretaker Caretaker { get; set; }
        public virtual ICollection<LevyPayment> LevyPayments { get; set; }
    }
}