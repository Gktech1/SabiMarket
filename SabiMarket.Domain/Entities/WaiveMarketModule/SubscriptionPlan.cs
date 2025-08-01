﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    public class SubscriptionPlan : BaseEntity
    {
        public string Frequency { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public CurrencyTypeEnum Currency { get; set; }
        public decimal Amount { get; set; }
        public string UserType { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; }
    }
}
