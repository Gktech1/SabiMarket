using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;

namespace SabiMarket.Domain.Entities
{
    [Table("Reports")]
    public class Report : BaseEntity
    {
        [Required]
        public int MarketCount { get; set; }

        [Required]
        public decimal TotalRevenueGenerated { get; set; }

        public DateTime ReportDate { get; set; }

        // For levy payments breakdown
        public string MarketId { get; set; }
        public string MarketName { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        // For compliance rates
        public decimal ComplianceRate { get; set; }
        public int TotalTraders { get; set; }
        public int CompliantTraders { get; set; }

        // For levy collection per market
        public decimal TotalLevyCollected { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PaymentTransactions { get; set; }
        public int ActiveMarkets { get; set; }
        public int NewTradersCount { get; set; }
        public bool IsDaily { get; set; }
        public int TotalCaretakers { get; set; }

        // Navigation property
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Market Market { get; set; }
    }
}