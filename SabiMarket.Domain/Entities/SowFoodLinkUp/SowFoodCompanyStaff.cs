using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Domain.Entities.SowFoodLinkUp
{
    public class SowFoodCompanyStaff : BaseEntity
    {
        public string SowFoodCompanyId { get; set; }
        public string StaffId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Role { get; set; }
        public string ImageUrl { get; set; }
        public SowFoodCompany SowFoodCompany { get; set; }
        public ICollection<SowFoodCompanyStaffAttendance> SowFoodCompanyStaffAttendances { get; set; }
    }
}
