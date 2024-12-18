using System.ComponentModel.DataAnnotations.Schema;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Domain.Entities.OrdersAndFeedback
{

    [Table("CustomerOrderItems")]
    public class CustomerOrderItem : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public virtual CustomerOrder Order { get; set; }
        public virtual WaivedProduct Product { get; set; }
    }

}
