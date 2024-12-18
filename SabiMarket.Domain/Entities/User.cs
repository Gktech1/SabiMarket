using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Domain.Entities
{
    public class ApplicationUser : IdentityUser, IAuditable
    {
        [PersonalData]
        [StringLength(100)]
        public string FirstName { get; set; }

        [PersonalData]
        [StringLength(100)]
        public string LastName { get; set; }

        public string ProfileImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int? LocalGovernmentId { get; set; }

        public virtual LocalGovernment LocalGovernment { get; set; }
        public virtual Trader Trader { get; set; }
        public virtual Vendor Vendor { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Caretaker Caretaker { get; set; }
        public virtual GoodBoy GoodBoy { get; set; }
        public virtual AssistCenterOfficer AssistCenterOfficer { get; set; }
    }
}
