using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.SowFoodLinkUp;

namespace SabiMarket.Application.IRepositories.SowFoodIRepositories
{
    public interface ISowFoodCompanyStaffRepository
    {
        void AddCompanyStaff(SowFoodCompanyStaff product);
        void UpdateCompanyStaff(SowFoodCompanyStaff staff);
        void DeleteCompanyStaff(SowFoodCompanyStaff staff);
        Task<SowFoodCompanyStaff> GetCompanyStaffById(string id, string companyId, bool trackChanges);
        Task<SowFoodCompanyStaff> GetLastCompanyStaff(string companyId, bool trackChanges);
        Task<PaginatorDto<IEnumerable<SowFoodCompanyStaff>>> GetPagedCompanyStaff(PaginationFilter paginationFilter);
        Task<PaginatorDto<IEnumerable<SowFoodCompanyStaff>>> SearchCompanyStaff(string searchString, PaginationFilter paginationFilter);
        Task<IEnumerable<SowFoodCompanyStaff>> GetAllCompanyStaffForExport(bool trackChanges);
    }
}
