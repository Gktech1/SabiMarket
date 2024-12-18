using System.ComponentModel.DataAnnotations.Schema;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Domain.Entities.OrdersAndFeedback
{

    [Table("CustomerFeedbacks")]
    public class CustomerFeedback : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Guid VendorId { get; set; }
        public string VendorCode { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Vendor Vendor { get; set; }
    }
}
