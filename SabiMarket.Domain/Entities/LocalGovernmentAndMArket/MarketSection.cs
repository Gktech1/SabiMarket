using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Domain.Entities.MarketParticipants;

namespace SabiMarket.Domain.Entities.LocalGovernmentAndMArket
{
    [Table("MarketSections")]
    public class MarketSection : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }

        public virtual Market Market { get; set; }
        public virtual ICollection<Trader> Traders { get; set; }
    }
}
