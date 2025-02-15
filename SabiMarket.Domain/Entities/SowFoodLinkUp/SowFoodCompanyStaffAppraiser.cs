using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities;

public class SowFoodCompanyStaffAppraiser : BaseEntity
{
    public string SowFoodCompanyStaffId { get; set; }
    public string? UserId { get; set; }  // Appraiser's user ID
    public string Remark { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; }
    public virtual SowFoodCompanyStaff SowFoodCompanyStaff { get; set; }
}