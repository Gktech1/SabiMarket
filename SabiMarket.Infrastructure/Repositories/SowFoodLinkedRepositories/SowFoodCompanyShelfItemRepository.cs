using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories.SowFoodIRepositories;

using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace SabiMarket.Infrastructure.Repositories.SowFoodLinkedRepositories
{
    public class SowFoodCompanyShelfItemRepository : GeneralRepository<SowFoodCompanyShelfItem>, ISowFoodCompanyShelfItemRepository
    {
        public SowFoodCompanyShelfItemRepository(ApplicationDbContext context) : base(context) { }

        public void AddCompanyShelfItem(SowFoodCompanyShelfItem shelfItem) => Create(shelfItem);

        public async Task<SowFoodCompanyShelfItem> GetCompanyShelfItemById(string id, string companyId, bool trackChanges) => await FindByCondition(x => x.Id == id && x.SowFoodCompanyId == companyId, trackChanges).FirstOrDefaultAsync();

        public async Task<IEnumerable<SowFoodCompanyShelfItem>> GetAllCompanyShelfItemForExport(bool trackChanges) => await FindAll(trackChanges).ToListAsync();

        public async Task<PaginatorDto<IEnumerable<SowFoodCompanyShelfItem>>> GetPagedCompanyShelfItem(string companyId, PaginationFilter paginationFilter)
        {
            return await FindAll(false).Where(x => x.SowFoodCompanyId == companyId)
                       .OrderBy(x => x.Name)
                       .Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<SowFoodCompanyShelfItem>>> SearchCompanyShelfItem(string searchString, PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                           .Where(a => a.Name.ToLower().Contains(searchString.ToLower()))
                           .Paginate(paginationFilter);
        }

        public void UpdateCompanyShelfItem(SowFoodCompanyShelfItem shelfItem) => Update(shelfItem);

        public void DeleteCompanyShelfItem(SowFoodCompanyShelfItem shelfItem) => Delete(shelfItem);

        public async void DeleteCompanyShelfItem(string id, string companyId)
        {
            var item = await FindByCondition(x => x.Id == id && x.SowFoodCompanyId == companyId, true).FirstOrDefaultAsync();

            Delete(item);
        }

        public async Task<PaginatorDto<IEnumerable<SowFoodCompanyShelfItem>>> SearchCompanyShelfItem(string searchString, string companyId, PaginationFilter paginationFilter)
        {
            var query = FindAll(trackChanges: false)
                .Where(x =>
                    x.SowFoodCompanyId == companyId &&
                    (
                        x.Name.Contains(searchString)
                    ))
                .OrderBy(x => x.Name);

            return await query.Paginate(paginationFilter);
        }

    }
}
