using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.SowFoodLinkUp;

namespace SabiMarket.Application.IRepositories.SowFoodIRepositories
{
    public interface ISowFoodCompanyShelfItemRepository
    {
        void AddCompanyShelfItem(SowFoodCompanyShelfItem product);
        void UpdateCompanyShelfItem(SowFoodCompanyShelfItem staff);
        void DeleteCompanyShelfItem(string id, string companyId);
        Task<SowFoodCompanyShelfItem> GetCompanyShelfItemById(string id, string companyId, bool trackChanges);
        Task<PaginatorDto<IEnumerable<SowFoodCompanyShelfItem>>> GetPagedCompanyShelfItem(string companyId, PaginationFilter paginationFilter);
        Task<PaginatorDto<IEnumerable<SowFoodCompanyShelfItem>>> SearchCompanyShelfItem(string searchString, string companyId, PaginationFilter paginationFilter);
        Task<IEnumerable<SowFoodCompanyShelfItem>> GetAllCompanyShelfItemForExport(bool trackChanges);
    }
}
