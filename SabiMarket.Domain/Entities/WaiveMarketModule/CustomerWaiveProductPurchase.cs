using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    public class CustomerWaiveProductPurchase : BaseEntity
    {
        public string CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public string WaivedProductId { get; set; }
        public virtual WaivedProduct WaivedProduct { get; set; }

        public string? DeliveryAddress { get; set; }
    }
}
