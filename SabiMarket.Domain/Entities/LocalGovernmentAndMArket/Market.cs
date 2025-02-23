using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.MarketParticipants;

namespace SabiMarket.Domain.Entities.LocalGovernmentAndMArket
{
    [Table("Markets")]
    public class Market : BaseEntity
    {
        [Required]
        public string LocalGovernmentId { get; set; }

        [Required]
        [StringLength(50)]
        public string MarketType { get; set; }

        public string CaretakerId { get; set; }

        [Required]
        public string ChairmanId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string Location { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        public string MarketName { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalRevenue { get; set; }

        public int PaymentTransactions { get; set; }
        public string LocalGovernmentName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalTraders { get; set; }
        public int MarketCapacity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OccupancyRate { get; set; }

        // New compliance-related properties
        [Column(TypeName = "decimal(18,2)")]
        public decimal ComplianceRate { get; set; }
        public int CompliantTraders { get; set; }
        public int NonCompliantTraders { get; set; }

        // Navigation properties
        [ForeignKey("ChairmanId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Chairman Chairman { get; set; }

        [ForeignKey("LocalGovernmentId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual LocalGovernment LocalGovernment { get; set; }
        
        [ForeignKey("CaretakerId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Caretaker Caretaker { get; set; }

        public virtual ICollection<Trader> Traders { get; set; } = new List<Trader>();
        public virtual ICollection<Caretaker> Caretakers { get; set; } = new List<Caretaker>();
        public virtual ICollection<MarketSection> Sections { get; set; } = new List<MarketSection>();
    }
}