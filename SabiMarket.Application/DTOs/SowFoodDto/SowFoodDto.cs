using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.SowFoodDto
{
    public class UpdateSowFoodCompanyCustomerDto
    {
        public string CustomerId { get; set; }
        public string SowFoodCompanyId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
    }
    public class CreateSowFoodCompanyCustomerDto
    {
        public string SowFoodCompanyId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }

    }

    public class GetSowFoodCompanyCustomerDto
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public List<BoughtItemDto> BoughtItems { get; set; } = new List<BoughtItemDto>();
    }

    public class BoughtItemDto
    {
        public string ItemName { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal Amount { get; set; }
    }

    public class UpdateSowFoodCompanyProductionItemDto
    {
        public string ProductionItemId { get; set; }
        public string SowFoodCompanyId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
    }
    public class CreateSowFoodCompanyProductionItemDto
    {
        public string SowFoodCompanyId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
    }
    public class CreateSowFoodCompanySalesRecordDto
    {
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? SowFoodCompanyProductItemId { get; set; }
        public string? SowFoodCompanyShelfItemId { get; set; }
        public string? SowFoodCompanyCustomerId { get; set; }

    }


}
