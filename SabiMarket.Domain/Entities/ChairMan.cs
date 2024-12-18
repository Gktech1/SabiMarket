using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.UserManagement;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Domain.Entities
{
    public class Chairman : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid LocalGovernmentId { get; set; }

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