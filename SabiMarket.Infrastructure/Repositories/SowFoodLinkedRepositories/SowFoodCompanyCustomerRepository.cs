using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories.SowFoodIRepositories;

using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories.SowFoodLinkedRepositories
{
    public class SowFoodCompanyCustomerRepository : GeneralRepository<SowFoodCompanyCustomer>, ISowFoodCompanyCustomerRepository
    {
        public SowFoodCompanyCustomerRepository(ApplicationDbContext context) : base(context) { }

        public void AddCompanyCustomer(SowFoodCompanyCustomer customer) => Create(customer);

        public async Task<SowFoodCompanyCustomer> GetCompanyCustomerById(string id, string companyId, bool trackChanges) => await FindByCondition(x => x.Id == id && x.SowFoodCompanyId == companyId, trackChanges).Include(x => x.SowFoodCompanySalesRecords).FirstOrDefaultAsync();

        public async Task<IEnumerable<SowFoodCompanyCustomer>> GetAllCompanyCustomerForExport(bool trackChanges) => await FindAll(trackChanges).Include(x => x.SowFoodCompanySalesRecords).ToListAsync();

        public async Task<PaginatorDto<IEnumerable<SowFoodCompanyCustomer>>> GetPagedCompanyCustomer(PaginationFilter paginationFilter)
        {
            return await FindAll(false).Include(x => x.SowFoodCompanySalesRecords)
                       .Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<SowFoodCompanyCustomer>>> SearchCompanyCustomer(string searchString, PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                           .Where(a => a.FullName.ToLower().Contains(searchString.ToLower())).Include(x => x.SowFoodCompanySalesRecords)
                           .Paginate(paginationFilter);
        }

        public void UpdateCompanyCustomer(SowFoodCompanyCustomer customer) =>
           Update(customer);

        public void DeleteCompanyCustomer(SowFoodCompanyCustomer customer) =>
            Delete(customer);
    }



}
