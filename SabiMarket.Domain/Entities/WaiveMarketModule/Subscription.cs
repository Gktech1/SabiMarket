using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    public class Subscription : BaseEntity
    {
        public string SGId { get; set; } //System GeneratedId
        public string SubscriberId { get; set; }
        public ApplicationUser Subscriber { get; set; }
        public DateTime SubscriptionStartDate { get; set; } = DateTime.UtcNow;
        //public int DurationInMonths { get; set; }
        public DateTime SubscriptionEndDate { get; set; } = DateTime.UtcNow.AddMonths(1);
        public ApplicationUser SubscriptionActivator { get; set; }
        public string SubscriptionActivatorId { get; set; }
        public decimal Amount { get; set; }
        //public SubscriptionType SubscriptionType { get; set; }
        //public string PaymentMethod { get; set; }
        public string ProofOfPayment { get; set; }
        public bool IsSubscriberConfirmPayment { get; set; }
        public bool IsAdminConfirmPayment { get; set; }
    }
}
