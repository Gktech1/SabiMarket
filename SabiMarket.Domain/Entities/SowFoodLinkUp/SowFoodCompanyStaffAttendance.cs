using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities;

public class SowFoodCompanyStaffAttendance : BaseEntity
{
    public string SowFoodCompanyStaffId { get; set; }
    public DateTime LogonTime { get; set; }
    public DateTime LogoutTime { get; set; }
    public DateTime? ConfirmedTimeIn { get; set; }
    public bool IsConfirmed { get; set; }
    public string? ConfirmedByUserId { get; set; }

    // Navigation properties
    public virtual ApplicationUser ConfirmedByUser { get; set; }
    public virtual SowFoodCompanyStaff SowFoodCompanyStaff { get; set; }
}