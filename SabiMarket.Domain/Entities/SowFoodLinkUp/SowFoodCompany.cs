using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities;

public class SowFoodCompany : BaseEntity
{
    public string CompanyName { get; set; }
    public string? UserId { get; set; }  // Company owner/admin

    // Navigation properties
    public virtual ApplicationUser User { get; set; }
    public virtual ICollection<SowFoodCompanyStaff> Staff { get; set; } = new List<SowFoodCompanyStaff>();
    public virtual ICollection<SowFoodCompanyProductionItem> SowFoodProducts { get; set; } = new List<SowFoodCompanyProductionItem>();
    public virtual ICollection<SowFoodCompanySalesRecord> SowFoodSalesRecords { get; set; } = new List<SowFoodCompanySalesRecord>();
    public virtual ICollection<SowFoodCompanyShelfItem> SowFoodShelfItems { get; set; } = new List<SowFoodCompanyShelfItem>();
    public virtual ICollection<SowFoodCompanyCustomer> Customers { get; set; } = new List<SowFoodCompanyCustomer>();
}