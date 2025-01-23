using System.ComponentModel.DataAnnotations.Schema;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities.MarketParticipants
{
    [Table("AssistCenterOfficers")]
    public class AssistCenterOfficer : BaseEntity
    {
        public string UserId { get; set; }
        public string ChairmanId { get; set; }
        public string? MarketId { get; set; }    

        public string? UserLevel { get; set; }

        public string LocalGovernmentId { get; set; }
        public bool IsBlocked { get; set; } = false;    
        public virtual Market Market { get; set; }
        public virtual Chairman Chairman { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual LocalGovernment LocalGovernment { get; set; }
    }
}