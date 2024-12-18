using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.OrdersAndFeedback;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    [Table("Customers")]
    public class Customer
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int LocalGovernmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        public DateTime SubscriptionEndDate { get; set; }
        public bool IsSubscriptionActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual LocalGovernment LocalGovernment { get; set; }
        public virtual ICollection<CustomerOrder> Orders { get; set; }
        public virtual ICollection<CustomerFeedback> Feedbacks { get; set; }
    }
}
