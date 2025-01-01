using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    public class Subscription : BaseEntity
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime SubscriptionStartDate { get; set; }
        //public int DurationInMonths { get; set; }
        public DateTime SubscriptionEndDate { get; set; } //= DateTime.UtcNow.AddMonths(DurationInMonths);
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
    }
}
