﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SabiMarket.Domain.Entities.MarketParticipants;

namespace SabiMarket.Domain.Entities.LocalGovernmentAndMArket
{
    [Table("Markets")]
    public class Market : BaseEntity
    {
        public string LocalGovernmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string Location { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        public string MarketName { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PaymentTransactions { get; set; }
        public string LocalGovernmentName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalTraders { get; set; }
        public int MarketCapacity { get; set; }
        public decimal OccupancyRate { get; set; }

        public virtual LocalGovernment LocalGovernment { get; set; }
        public virtual ICollection<Trader> Traders { get; set; }
        public virtual ICollection<Caretaker> Caretakers { get; set; }
        public virtual ICollection<MarketSection> Sections { get; set; }
    }
}
