using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories.SowFoodIRepositories;

using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories.SowFoodLinkedRepositories
{
    public class SowFoodCompanyRepository : GeneralRepository<SowFoodCompany>, ISowFoodCompanyRepository
    {
        public SowFoodCompanyRepository(ApplicationDbContext context) : base(context) { }

        public void AddCompany(SowFoodCompany company) => Create(company);

        public async Task<SowFoodCompany> GetCompanyById(string id, bool trackChanges) => await FindByCondition(x => x.Id == id, trackChanges).FirstOrDefaultAsync();

        public async Task<IEnumerable<SowFoodCompany>> GetAllCompanyForExport(bool trackChanges) => await FindAll(trackChanges).ToListAsync();

        public async Task<PaginatorDto<IEnumerable<SowFoodCompany>>> GetPagedCompany(PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                       .Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<SowFoodCompany>>> SearchCompany(string searchString, PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                           .Where(a => a.CompanyName.ToLower().Contains(searchString.ToLower()))
                           .Paginate(paginationFilter);
        }

        public void UpdateCompany(SowFoodCompany company) =>
           Update(company);

        public void DeleteCompany(SowFoodCompany company) =>
            Delete(company);
    }
}
