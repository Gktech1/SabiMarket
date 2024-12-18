﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SabiMarket.Domain.Entities.OrdersAndFeedback;
using SabiMarket.Domain.Entities.Supporting;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    [Table("WaivedProducts")]
    public class WaivedProduct
    {
        public int Id { get; set; }
        public int VendorId { get; set; }

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

        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Vendor Vendor { get; set; }
        public virtual ICollection<ProductCategory> Categories { get; set; }
        public virtual ICollection<CustomerOrderItem> OrderItems { get; set; }
    }
}