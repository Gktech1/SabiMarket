using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.SowFoodLinkUp;

namespace SabiMarket.Application.IRepositories.SowFoodIRepositories
{
    public interface ISowFoodCompanyRepository
    {
        void AddCompany(SowFoodCompany product);
        void UpdateCompany(SowFoodCompany staff);
        void DeleteCompany(SowFoodCompany staff);
        Task<SowFoodCompany> GetCompanyById(string id, bool trackChanges);
        Task<PaginatorDto<IEnumerable<SowFoodCompany>>> GetPagedCompany(PaginationFilter paginationFilter);
        Task<PaginatorDto<IEnumerable<SowFoodCompany>>> SearchCompany(string searchString, PaginationFilter paginationFilter);
        Task<IEnumerable<SowFoodCompany>> GetAllCompanyForExport(bool trackChanges);
    }
}
