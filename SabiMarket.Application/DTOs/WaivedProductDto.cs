using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.DTOs
{
    public class CreateWaivedProductDto
    {
        public string VendorId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailbleForUrgentPurchase { get; set; }
        public string Category { get; set; }
        public CurrencyTypeEnum CurrencyType { get; set; }

        //public decimal OriginalPrice { get; set; }
        //public decimal WaivedPrice { get; set; }
        //public string Description { get; set; }
        //public int StockQuantity { get; set; }
    }
    public class ProductDetailsDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public bool IsAvailbleForUrgentPurchase { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public CurrencyTypeEnum CurrencyType { get; set; }
        public decimal Price { get; set; }

        //public string Description { get; set; }
        //public decimal OriginalPrice { get; set; }
        //public decimal WaivedPrice { get; set; }
    }
    public class UpdateWaivedProductDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public bool IsAvailbleForUrgentPurchase { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public CurrencyTypeEnum CurrencyType { get; set; }
        public decimal Price { get; set; }
        //public string Description { get; set; }
        //public decimal OriginalPrice { get; set; }
        //public decimal WaivedPrice { get; set; }
    }
}
