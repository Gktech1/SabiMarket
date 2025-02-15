using SabiMarket.Domain.Entities;

public class SowFoodCompanySalesRecord : BaseEntity
{
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    public string? SowFoodCompanyProductItemId { get; set; }
    public string? SowFoodCompanyShelfItemId { get; set; }
    public string? SowFoodCompanyCustomerId { get; set; }
    public string SowFoodCompanyStaffId { get; set; }

    // Navigation properties
    public virtual SowFoodCompanyProductionItem? SowFoodCompanyProductItem { get; set; }
    public virtual SowFoodCompanyShelfItem? SowFoodCompanyShelfItem { get; set; }
    public virtual SowFoodCompanyCustomer SowFoodCompanyCustomer { get; set; }
    public virtual SowFoodCompanyStaff SowFoodCompanyStaff { get; set; }
}
