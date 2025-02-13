using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Application.IRepositories.SowFoodIRepositories;
using SabiMarket.Domain.Entities.SowFoodLinkUp;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories.SowFoodLinkedRepositories
{
    public class SowFoodStaffRepository : GeneralRepository<SowFoodCompanyStaff>, ISowFoodStaffRepository
    {
        public SowFoodStaffRepository(ApplicationDbContext context) : base(context) { }

        public void AddStaff(SowFoodCompanyStaff product) => Create(product);

        public async Task<SowFoodCompanyStaff> GetStaffById(string id, bool trackChanges) => await FindByCondition(x => x.Id == id, trackChanges).FirstOrDefaultAsync();

        public async Task<IEnumerable<SowFoodCompanyStaff>> GetAllStaffForExport(bool trackChanges) => await FindAll(trackChanges).ToListAsync();

        public async Task<PaginatorDto<IEnumerable<SowFoodCompanyStaff>>> GetPagedStaff(PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                       .Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<SowFoodCompanyStaff>>> SearchStaff(string searchString, PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                           .Where(a => a.FullName.Contains(searchString) ||
                           a.EmailAddress.Contains(searchString))
                           .Paginate(paginationFilter);
        }

        public void UpdateStaff(SowFoodCompanyStaff staff) =>
           Update(staff);

        public void DeleteStaff(SowFoodCompanyStaff staff) =>
            Delete(staff);
    }



}
