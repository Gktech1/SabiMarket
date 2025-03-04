using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories.SowFoodIRepositories;

using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories.SowFoodLinkedRepositories
{
    public class SowFoodCompanyStaffAppraiserRepository : GeneralRepository<SowFoodCompanyStaffAppraiser>, ISowFoodCompanyStaffAppraiserRepository
    {
        public SowFoodCompanyStaffAppraiserRepository(ApplicationDbContext context) : base(context) { }

        public void AddCompanyStaffAppraiser(SowFoodCompanyStaffAppraiser staffAppraiser) => Create(staffAppraiser);

        public async Task<SowFoodCompanyStaffAppraiser> GetCompanyStaffAppraiserById(string id, bool trackChanges) => await FindByCondition(x => x.Id == id, trackChanges).FirstOrDefaultAsync();

        public async Task<IEnumerable<SowFoodCompanyStaffAppraiser>> GetAllCompanyStaffAppraiserForExport(bool trackChanges) => await FindAll(trackChanges).ToListAsync();

        public async Task<PaginatorDto<IEnumerable<SowFoodCompanyStaffAppraiser>>> GetPagedCompanyStaffAppraiser(PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                       .Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<SowFoodCompanyStaffAppraiser>>> SearchCompanyStaffAppraiser(string searchString, PaginationFilter paginationFilter)
        {
            return await FindAll(false).Include(x=>x.SowFoodCompanyStaff)
                           .Where(a => a.SowFoodCompanyStaff.FullName.ToLower().Contains(searchString.ToLower()))
                           .Paginate(paginationFilter);
        }

        public void UpdateCompanyStaffAppraiser(SowFoodCompanyStaffAppraiser staffAppraiser) =>
           Update(staffAppraiser);

        public void DeleteCompanyStaffAppraiser(SowFoodCompanyStaffAppraiser staffAppraiser) =>
            Delete(staffAppraiser);
    }
}
