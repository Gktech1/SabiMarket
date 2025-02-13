using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Domain.Entities.SowFoodLinkUp
{
    public class SowFoodCompanyStaffAttendance
    {
        public string SowFoodCompanyStaffId { get; set; }
        public DateTime LogonTime { get; set; } = DateTime.Now;
        public DateTime LogoutTime { get; set; } = DateTime.Now;
        public DateTime? ConfirmedTimeIn { get; set; } = DateTime.Now;
        public bool IsConfirmed { get; set; }
        public SowFoodCompanyStaff SowFoodCompanyStaff { get; set; }
    }
}
