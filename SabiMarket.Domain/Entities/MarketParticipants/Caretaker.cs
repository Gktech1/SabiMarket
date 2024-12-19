using System.ComponentModel.DataAnnotations.Schema;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities.MarketParticipants
{

    [Table("Caretakers")]
    public class Caretaker : BaseEntity
    {
        public string UserId { get; set; }
        public Guid MarketId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Market Market { get; set; }
        public virtual ICollection<GoodBoy> GoodBoys { get; set; }
        public virtual ICollection<Trader> AssignedTraders { get; set; }
    }
}