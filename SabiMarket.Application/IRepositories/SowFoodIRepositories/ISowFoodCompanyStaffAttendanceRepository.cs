using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.SowFoodLinkUp;

namespace SabiMarket.Application.IRepositories.SowFoodIRepositories
{
    public interface ISowFoodCompanyStaffAttendanceRepository
    {
        void AddCompanyStaffAttendance(SowFoodCompanyStaffAttendance product);
        void UpdateCompanyStaffAttendance(SowFoodCompanyStaffAttendance staff);
        void DeleteCompanyStaffAttendance(SowFoodCompanyStaffAttendance staff);
        Task<SowFoodCompanyStaffAttendance> GetCompanyStaffAttendanceById(string id, bool trackChanges);
        Task<PaginatorDto<IEnumerable<SowFoodCompanyStaffAttendance>>> GetPagedCompanyStaffAttendance(PaginationFilter paginationFilter);
        Task<PaginatorDto<IEnumerable<SowFoodCompanyStaffAttendance>>> SearchCompanyStaffAttendance(string searchString, PaginationFilter paginationFilter);
        Task<IEnumerable<SowFoodCompanyStaffAttendance>> GetAllCompanyStaffAttendanceForExport(bool trackChanges);
    }
}
