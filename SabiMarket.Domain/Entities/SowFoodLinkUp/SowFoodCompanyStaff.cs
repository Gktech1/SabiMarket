using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities;

public class SowFoodCompanyStaff : BaseEntity
{
    public string SowFoodCompanyId { get; set; }
    public string? UserId { get; set; }
    public string StaffId { get; set; }  // Internal staff identifier if needed
    public string ImageUrl { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; }  // Contains name, email, phone
    public virtual SowFoodCompany SowFoodCompany { get; set; }
    public virtual ICollection<SowFoodCompanyStaffAttendance> SowFoodCompanyStaffAttendances { get; set; } = new List<SowFoodCompanyStaffAttendance>();
    public virtual ICollection<SowFoodCompanySalesRecord> SowFoodCompanySalesRecords { get; set; } = new List<SowFoodCompanySalesRecord>();
}