using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories.SowFoodIRepositories;
using SabiMarket.Domain.Entities.SowFoodLinkUp;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories.SowFoodLinkedRepositories
{
    public class SowFoodCompanySalesRecordRepository : GeneralRepository<SowFoodCompanySalesRecord>, ISowFoodCompanySalesRecordRepository
    {
        public SowFoodCompanySalesRecordRepository(ApplicationDbContext context) : base(context) { }

        public void AddCompanySalesRecord(SowFoodCompanySalesRecord salesRecord) => Create(salesRecord);

        public async Task<SowFoodCompanySalesRecord> GetCompanySalesRecordById(string id, string companyId, bool trackChanges) => await FindByCondition(x => x.Id == id && x.SowFoodCompanyId == companyId, trackChanges).FirstOrDefaultAsync();

        public async Task<IEnumerable<SowFoodCompanySalesRecord>> GetAllCompanySalesRecordForExport(string companyId, bool trackChanges) => await FindAll(trackChanges).Where(x => x.SowFoodCompanyId == companyId).ToListAsync();

        public async Task<PaginatorDto<IEnumerable<SowFoodCompanySalesRecord>>> GetPagedCompanySalesRecord(string companyId, PaginationFilter paginationFilter)
        {
            return await FindAll(false).Where(x => x.SowFoodCompanyId == companyId)
                       .Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<SowFoodCompanySalesRecord>>> SearchCompanySalesRecord(string companyId, string searchString, PaginationFilter paginationFilter)
        {
            return await FindAll(false).Where(x => x.SowFoodCompanyId == companyId)
                           .Where(a => a.SowFoodCompanyShelfItem.Name.ToLower().Contains(searchString.ToLower()))
                           .Paginate(paginationFilter);
        }

        public void UpdateCompanySalesRecord(SowFoodCompanySalesRecord salesRecord) =>
           Update(salesRecord);

        public async void DeleteCompanySalesRecord(string companyId, string salesRecordId)
        {
            Delete(await GetCompanySalesRecordById(salesRecordId, companyId, false));
        }
    }
}
