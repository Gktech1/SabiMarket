using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Domain.Entities.MarketParticipants
{
    [Table("Traders")]
    public class Trader : BaseEntity
    {
        // User Related Information
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        // Location Information
        public string MarketId { get; set; }
        public virtual Market Market { get; set; }

        public string? SectionId { get; set; }
        public virtual MarketSection Section { get; set; }

        public string CaretakerId { get; set; }
        public virtual Caretaker Caretaker { get; set; }

        // Business Information
        [Required]
        [StringLength(50)]
        public string? TIN { get; set; }

        [Required]
        public string? BusinessName { get; set; }

        [Required]
        public string BusinessType { get; set; }  // e.g., "Open Space", etc.

        [Required]
        public PaymentFrequencyEnum PaymentFrequency { get; set; }  // Daily/Weekly/Monthly

        public decimal PaymentAmount { get; set; }  // Amount to pay based on frequency
        public TraderStatusEnum Status { get; set; } = TraderStatusEnum.Active;  // Active/Inactive/Suspended

        // QR Code Information
        public string? QRCode { get; set; }

        // Navigation Properties
        public virtual ICollection<LevyPayment> LevyPayments { get; set; }

        // Additional Properties for Business Logic
        [NotMapped]
        public DateTime? LastPaymentDate => LevyPayments?.OrderByDescending(p => p.PaymentDate).FirstOrDefault()?.PaymentDate;

        [NotMapped]
        public bool HasOutstandingPayment { get; set; }
    }
}
