using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Domain.Entities.UserManagement
{
    public class ApplicationUser : IdentityUser<string>
    {
        [PersonalData]
        [StringLength(100)]
        public string FirstName { get; set; }

        [PersonalData]
        [StringLength(100)]
        public string LastName { get; set; }

        public string? Address { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool IsBlocked { get; set; }  = false;  
        public string? Gender { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? RefreshToken { get; set; }
        public string? RefreshTokenJwtId { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public bool? IsRefreshTokenUsed { get; set; }
        public string? LocalGovernmentId { get; set; }

        public string? AdminId { get; set; }
        public virtual Admin Admin { get; set; }
        public virtual LocalGovernment LocalGovernment { get; set; }
        public virtual Chairman Chairman { get; set; }
        public virtual Trader Trader { get; set; }
        public virtual Vendor Vendor { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Caretaker Caretaker { get; set; }
        public virtual GoodBoy GoodBoy { get; set; }
        public virtual AssistCenterOfficer AssistCenterOfficer { get; set; }
    }
}
