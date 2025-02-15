using SabiMarket.Domain.Entities;

public class SowFoodCompanyProductionItem : BaseEntity
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string ImageUrl { get; set; }
    public string SowFoodCompanyId { get; set; }

    // Navigation property
    public virtual SowFoodCompany SowFoodCompany { get; set; }
}
