using System.ComponentModel.DataAnnotations.Schema;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities.MarketParticipants
{
    [Table("AssistCenterOfficers")]
    public class AssistCenterOfficer : BaseEntity
    {
        public string UserId { get; set; }
        public Guid LocalGovernmentId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual LocalGovernment LocalGovernment { get; set; }
    }
}