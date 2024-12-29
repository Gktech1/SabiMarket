using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities
{
    public class Chairman : BaseEntity
    {
        public string UserId { get; set; }
        public string LocalGovernmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public string Office { get; set; }
        public DateTime TermStart { get; set; }
        public DateTime? TermEnd { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual LocalGovernment LocalGovernment { get; set; }
    }

}