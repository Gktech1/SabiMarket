namespace SabiMarket.Domain.Entities.SowFoodLinkUp
{
    public class SowFoodCompanySalesRecord : BaseEntity
    {
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
        public string? SowFoodCompanyProductItemId { get; set; }
        public SowFoodCompanyProductionItem? SowFoodCompanyProductItem { get; set; }
        public string? SowFoodCompanyShelfItemId { get; set; }
        public SowFoodCompanyShelfItem? SowFoodCompanyShelfItem { get; set; }
        public string? SowFoodCompanyCustomerId { get; set; }
        public string SowFoodCompanyStaffId { get; set; }
        public SowFoodCompanyCustomer SowFoodCompanyCustomer { get; set; }
        public SowFoodCompanyStaff SowFoodCompanyStaff { get; set; }
    }
}