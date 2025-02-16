using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.OrdersAndFeedback;
using SabiMarket.Domain.Entities.Supporting;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    [Table("WaivedProducts")]
    public class WaivedProduct : BaseEntity
    {
        [Required]
        public string VendorId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string ProductCode { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal WaivedPrice { get; set; }

        public bool IsAvailbleForUrgentPurchase { get; set; }
        public int StockQuantity { get; set; }

        [ForeignKey("VendorId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Vendor Vendor { get; set; }

        public virtual ICollection<ProductCategory> Categories { get; set; } = new List<ProductCategory>();
        public virtual ICollection<CustomerOrderItem> OrderItems { get; set; } = new List<CustomerOrderItem>();
    }
}
