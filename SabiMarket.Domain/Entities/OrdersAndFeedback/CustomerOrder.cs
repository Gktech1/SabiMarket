using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Domain.Entities.OrdersAndFeedback
{
    [Table("CustomerOrders")]
    public class CustomerOrder : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Guid VendorId { get; set; }
        public DateTime OrderDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public OrderStatusEnum Status { get; set; }
        public string DeliveryAddress { get; set; }
        public string Notes { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Vendor Vendor { get; set; }
        public virtual ICollection<CustomerOrderItem> OrderItems { get; set; }
    }

}
