using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities;

public class SowFoodCompanyCustomer : BaseEntity
{
    public string SowFoodCompanyId { get; set; }
    public string? UserId { get; set; }
    public string RegisteredBy { get; set; }  // UserId of staff who registered the customer

    // Navigation properties
    public virtual ApplicationUser User { get; set; }
    public virtual SowFoodCompany SowFoodCompany { get; set; }
    public virtual ICollection<SowFoodCompanySalesRecord> SowFoodCompanySalesRecords { get; set; } = new List<SowFoodCompanySalesRecord>();
}