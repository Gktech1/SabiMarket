using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Application.DTOs
{
    public class CreateWaivedProductDto
    {
        public string VendorId { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal WaivedPrice { get; set; }
        public bool IsAvailbleForUrgentPurchase { get; set; }
        public int StockQuantity { get; set; }
    }
    public class ProductDetailsDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal WaivedPrice { get; set; }
        public bool IsAvailbleForUrgentPurchase { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }
    public class UpdateWaivedProductDto
    {
        public string ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal WaivedPrice { get; set; }
        public bool IsAvailbleForUrgentPurchase { get; set; }
        public int StockQuantity { get; set; }
    }
}
