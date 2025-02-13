using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Domain.Entities.SowFoodLinkUp
{
    public class SowFoodCompanyStaffAppraiser
    {
        public string SowFoodCompanyStaffId { get; set; }
        public string Remark { get; set; }
        public SowFoodCompanyStaff SowFoodCompanyStaff { get; set; }
    }
}
