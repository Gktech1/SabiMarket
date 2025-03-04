using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories.SowFoodIRepositories;

using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories.SowFoodLinkedRepositories
{
    public class SowFoodCompanyStaffAttendanceRepository : GeneralRepository<SowFoodCompanyStaffAttendance>, ISowFoodCompanyStaffAttendanceRepository
    {
        public SowFoodCompanyStaffAttendanceRepository(ApplicationDbContext context) : base(context) { }

        public void AddCompanyStaffAttendance(SowFoodCompanyStaffAttendance company) => Create(company);

        public async Task<SowFoodCompanyStaffAttendance> GetCompanyStaffAttendanceById(string id, bool trackChanges) => await FindByCondition(x => x.Id == id, trackChanges).FirstOrDefaultAsync();

        public async Task<IEnumerable<SowFoodCompanyStaffAttendance>> GetAllCompanyStaffAttendanceForExport(bool trackChanges) => await FindAll(trackChanges).ToListAsync();

        public async Task<PaginatorDto<IEnumerable<SowFoodCompanyStaffAttendance>>> GetPagedCompanyStaffAttendance(PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                       .Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<SowFoodCompanyStaffAttendance>>> SearchCompanyStaffAttendance(string searchString, PaginationFilter paginationFilter)
        {
            return await FindAll(false).Include(x=>x.SowFoodCompanyStaff)
                           .Where(a => a.SowFoodCompanyStaff.FullName.ToLower().Contains(searchString.ToLower()))
                           .Paginate(paginationFilter);
        }

        public void UpdateCompanyStaffAttendance(SowFoodCompanyStaffAttendance company) =>
           Update(company);

        public void DeleteCompanyStaffAttendance(SowFoodCompanyStaffAttendance company) =>
            Delete(company);
    }
}
