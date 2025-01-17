using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    public class SubscriptionPlan : BaseEntity
    {
        public string Frequency { get; set; }
        public decimal Amount { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; }
    }
}
