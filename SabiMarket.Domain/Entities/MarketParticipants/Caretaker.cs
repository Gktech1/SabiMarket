﻿using System.ComponentModel.DataAnnotations.Schema;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities.MarketParticipants
{

    [Table("Caretakers")]
    public class Caretaker : BaseEntity
    {
        public string UserId { get; set; }
        public string MarketId { get; set; }
        public string ChairmanId { get; set; }  
        public bool IsBlocked { get; set; } = false;
        public virtual ApplicationUser User { get; set; }
        public virtual Market Market { get; set; }

        public virtual Chairman Chairman { get; set; }
        public virtual ICollection<GoodBoy> GoodBoys { get; set; }
        public virtual ICollection<Trader> AssignedTraders { get; set; }
    }
}