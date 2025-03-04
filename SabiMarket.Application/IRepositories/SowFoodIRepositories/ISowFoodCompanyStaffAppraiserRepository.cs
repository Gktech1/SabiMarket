using SabiMarket.Application.DTOs;


namespace SabiMarket.Application.IRepositories.SowFoodIRepositories
{
    public interface ISowFoodCompanyStaffAppraiserRepository
    {
        void AddCompanyStaffAppraiser(SowFoodCompanyStaffAppraiser product);
        void UpdateCompanyStaffAppraiser(SowFoodCompanyStaffAppraiser staff);
        void DeleteCompanyStaffAppraiser(SowFoodCompanyStaffAppraiser staff);
        Task<SowFoodCompanyStaffAppraiser> GetCompanyStaffAppraiserById(string id, bool trackChanges);
        Task<PaginatorDto<IEnumerable<SowFoodCompanyStaffAppraiser>>> GetPagedCompanyStaffAppraiser(PaginationFilter paginationFilter);
        Task<PaginatorDto<IEnumerable<SowFoodCompanyStaffAppraiser>>> SearchCompanyStaffAppraiser(string searchString, PaginationFilter paginationFilter);
        Task<IEnumerable<SowFoodCompanyStaffAppraiser>> GetAllCompanyStaffAppraiserForExport(bool trackChanges);
    }
}
