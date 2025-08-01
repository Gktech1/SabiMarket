﻿using System;
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
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailbleForUrgentPurchase { get; set; }
        public string CategoryId { get; set; }
        public CurrencyTypeEnum CurrencyType { get; set; }

    }
    public class ProductDetailsDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string VendorId { get; set; }
        public bool IsAvailbleForUrgentPurchase { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public CurrencyTypeEnum CurrencyType { get; set; }
        public decimal Price { get; set; }
        public bool IsVerifiedVendor { get; set; }
        public List<CustomerWaivedProductPurchaseDto> Customers { get; set; }

    }
    public class UpdateWaivedProductDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public bool IsAvailbleForUrgentPurchase { get; set; }
        public string CategoryId { get; set; }
        public string ImageUrl { get; set; }
        public CurrencyTypeEnum CurrencyType { get; set; }
        public decimal Price { get; set; }
        //public string Description { get; set; }
        //public decimal OriginalPrice { get; set; }
        //public decimal WaivedPrice { get; set; }
    }

    public class VendorDto
    {
        public string Id { get; set; }
        public string BusinessName { get; set; }
        public string VendorName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string LGA { get; set; }
        public string UserAddress { get; set; }
        public string BusinessAddress { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public CurrencyTypeEnum? VendorCurrencyType { get; set; }

        // Prevent circular references by not including the full User object
        public List<ProductDto> Products { get; set; } = new();
    }

    public class ProductDto
    {
        public string Id { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
    }

    public class CustomerInterstForUrgentPurchase
    {
        public string VendorId { get; set; }
        public string ProductId { get; set; }
    }

    public class CustomerWaivedProductPurchaseDto
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string? DeliveryAddress { get; set; }
    }

    public class WaivedProductDto
    {
        public string Id { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string VendorName { get; set; }
        public bool IsAvailbleForUrgentPurchase { get; set; }
        public List<CustomerWaivedProductPurchaseDto> Purchases { get; set; }
    }

    public class UrgentPurchaseDto
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string DeliveryAddress { get; set; }
        public DateTime DateCreated { get; set; }
        public string ProductName { get; set; }
        public string VendorId { get; set; }
        public string ProductImage { get; set; }
        public decimal ProductPrice { get; set; }
    }
    public class UrgentPurchaseFilter : PaginationFilter
    {
        public string? SearchTerm { get; set; } // Search by customer name or product name
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? VendorId { get; set; }
    }

}
