using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories.SowFoodIRepositories;

using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories.SowFoodLinkedRepositories
{
    public class SowFoodCompanyProductionItemRepository : GeneralRepository<SowFoodCompanyProductionItem>, ISowFoodCompanyProductionItemRepository
    {
        public SowFoodCompanyProductionItemRepository(ApplicationDbContext context) : base(context) { }

        public void AddCompanyProductionItem(SowFoodCompanyProductionItem productionItem) => Create(productionItem);

        public async Task<SowFoodCompanyProductionItem> GetCompanyProductionItemById(string id, string companyId, bool trackChanges) => await FindByCondition(x => x.Id == id && x.SowFoodCompanyId == companyId, trackChanges).FirstOrDefaultAsync();

        public async Task<IEnumerable<SowFoodCompanyProductionItem>> GetAllCompanyProductionItemForExport(bool trackChanges) => await FindAll(trackChanges).ToListAsync();

        public async Task<PaginatorDto<IEnumerable<SowFoodCompanyProductionItem>>> GetPagedCompanyProductionItem(string itemId, string companyId, PaginationFilter paginationFilter)
        {
            return await FindAll(false).Where(x => x.Id == itemId && x.SowFoodCompanyId == companyId)
                       .Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<SowFoodCompanyProductionItem>>> SearchCompanyProductionItem(string searchString, PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                           .Where(a => a.Name.Contains(searchString))
                           .Paginate(paginationFilter);
        }

        public void UpdateCompanyProductionItem(SowFoodCompanyProductionItem productionItem) =>
           Update(productionItem);

        public void DeleteCompanyProductionItem(SowFoodCompanyProductionItem productionItem) =>
            Delete(productionItem);
    }



}
