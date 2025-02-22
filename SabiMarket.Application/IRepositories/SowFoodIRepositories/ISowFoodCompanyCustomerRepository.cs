using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.SowFoodLinkUp;

namespace SabiMarket.Application.IRepositories.SowFoodIRepositories
{
    public interface ISowFoodCompanyCustomerRepository
    {
        void AddCompanyCustomer(SowFoodCompanyCustomer customer);
        void UpdateCompanyCustomer(SowFoodCompanyCustomer customer);
        void DeleteCompanyCustomer(SowFoodCompanyCustomer customer);
        Task<SowFoodCompanyCustomer> GetCompanyCustomerById(string id, string companyId, bool trackChanges);
        Task<PaginatorDto<IEnumerable<SowFoodCompanyCustomer>>> GetPagedCompanyCustomer(PaginationFilter paginationFilter);
        Task<PaginatorDto<IEnumerable<SowFoodCompanyCustomer>>> SearchCompanyCustomer(string searchString, PaginationFilter paginationFilter);
        Task<IEnumerable<SowFoodCompanyCustomer>> GetAllCompanyCustomerForExport(bool trackChanges);
    }
}
