using SabiMarket.Domain.Entities.AdvertisementModule;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SabiMarket.Domain.Enum;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.OrdersAndFeedback;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    [Table("Vendors")]
    public class Vendor : BaseEntity
    {
        public string UserId { get; set; }
        public Guid LocalGovernmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string BusinessName { get; set; }

        [Required]
        [StringLength(200)]
        public string BusinessAddress { get; set; }

        [Required]
        public string VendorCode { get; set; }

        public string BusinessDescription { get; set; }
        public VendorTypeEnum Type { get; set; }
        public bool IsVerified { get; set; }
        public DateTime SubscriptionEndDate { get; set; }
        public bool IsSubscriptionActive { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual LocalGovernment LocalGovernment { get; set; }
        public virtual ICollection<WaivedProduct> Products { get; set; }
        public virtual ICollection<CustomerOrder> Orders { get; set; }
        public virtual ICollection<CustomerFeedback> Feedbacks { get; set; }
        public virtual ICollection<Advertisement> Advertisements { get; set; }
    }

}
