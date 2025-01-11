using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    public class Subscription : BaseEntity
    {
        public string SubscriberId { get; set; }
        public ApplicationUser Subscriber { get; set; }
        public DateTime SubscriptionStartDate { get; set; }
        //public int DurationInMonths { get; set; }
        public DateTime SubscriptionEndDate { get; set; } //= DateTime.UtcNow.AddMonths(DurationInMonths);
        public ApplicationUser SubscriptionActivator { get; set; }
        public string SubscriptionActivatorId { get; set; }
        public decimal Amount { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
        public string PaymentMethod { get; set; }
        public string ProofOfPayment { get; set; }
    }
}
