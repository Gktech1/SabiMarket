using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities.MarketParticipants
{
    [Table("Traders")]
    public class Trader : BaseEntity
    {
        public string UserId { get; set; }
        public Guid MarketId { get; set; }
        public Guid? SectionId { get; set; }
        public Guid CaretakerId { get; set; }

        [Required]
        [StringLength(50)]
        public string TIN { get; set; }

        [Required]
        public string BusinessName { get; set; }
        public string BusinessType { get; set; }
        public string QRCode { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Market Market { get; set; }
        public virtual MarketSection Section { get; set; }
        public virtual Caretaker Caretaker { get; set; }
        public virtual ICollection<LevyPayment> LevyPayments { get; set; }
    }
}