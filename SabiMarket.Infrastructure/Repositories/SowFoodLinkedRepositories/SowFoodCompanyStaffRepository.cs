using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Application.IRepositories.SowFoodIRepositories;

using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories.SowFoodLinkedRepositories
{
    public class SowFoodCompanyStaffRepository : GeneralRepository<SowFoodCompanyStaff>, ISowFoodCompanyStaffRepository
    {
        public SowFoodCompanyStaffRepository(ApplicationDbContext context) : base(context) { }

        public void AddCompanyStaff(SowFoodCompanyStaff staff) => Create(staff);

        public async Task<SowFoodCompanyStaff> GetCompanyStaffById(string id, string companyId, bool trackChanges) => await FindByCondition(x => x.Id == id && x.SowFoodCompanyId == companyId, trackChanges).FirstOrDefaultAsync();

        public async Task<SowFoodCompanyStaff> GetLastCompanyStaff(string companyId, bool trackChanges) => await FindByCondition(x => x.SowFoodCompanyId == companyId, trackChanges).LastOrDefaultAsync();

        public async Task<IEnumerable<SowFoodCompanyStaff>> GetAllCompanyStaffForExport(bool trackChanges) => await FindAll(trackChanges).ToListAsync();

        public async Task<PaginatorDto<IEnumerable<SowFoodCompanyStaff>>> GetPagedCompanyStaff(PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                       .Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<SowFoodCompanyStaff>>> SearchCompanyStaff(string searchString, PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                           .Where(a => a.FullName.Contains(searchString) ||
                           a.EmailAddress.Contains(searchString))
                           .Paginate(paginationFilter);
        }

        public void UpdateCompanyStaff(SowFoodCompanyStaff staff) =>
           Update(staff);

        public void DeleteCompanyStaff(SowFoodCompanyStaff staff) =>
            Delete(staff);
    }
}
