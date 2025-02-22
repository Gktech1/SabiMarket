using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.SowFoodLinkUp;

namespace SabiMarket.Application.IRepositories.SowFoodIRepositories
{
    public interface ISowFoodCompanyProductionItemRepository
    {
        void AddCompanyProductionItem(SowFoodCompanyProductionItem item);
        void UpdateCompanyProductionItem(SowFoodCompanyProductionItem item);
        void DeleteCompanyProductionItem(SowFoodCompanyProductionItem item);
        Task<SowFoodCompanyProductionItem> GetCompanyProductionItemById(string id, string companyId, bool trackChanges);
        Task<PaginatorDto<IEnumerable<SowFoodCompanyProductionItem>>> GetPagedCompanyProductionItem(PaginationFilter paginationFilter);
        Task<PaginatorDto<IEnumerable<SowFoodCompanyProductionItem>>> SearchCompanyProductionItem(string searchString, PaginationFilter paginationFilter);
        Task<IEnumerable<SowFoodCompanyProductionItem>> GetAllCompanyProductionItemForExport(bool trackChanges);
    }
}
