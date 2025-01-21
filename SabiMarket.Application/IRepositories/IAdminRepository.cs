using Microsoft.EntityFrameworkCore.Query;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Application.IRepositories
{
    public interface IAdminRepository : IGeneralRepository<Admin>
    {
        Task<Admin> GetAdminByIdAsync(string adminId, bool trackChanges);
        Task<Admin> GetAdminByUserIdAsync(string userId, bool trackChanges);
        Task<PaginatorDto<IEnumerable<Admin>>> GetAdminsWithPaginationAsync(
            PaginationFilter paginationFilter, bool trackChanges);
        Task<bool> AdminExistsAsync(string userId);
        void CreateAdmin(Admin admin);
        void UpdateAdmin(Admin admin);
        Task UpdateAdminStatsAsync(string adminId, int registeredLGAs, int activeChairmen, decimal totalRevenue);
        Task<Admin> GetAdminDashboardStatsAsync(string adminId);
        IQueryable<AuditLog> GetAdminAuditLogsQuery(string adminId, DateTime? startDate, DateTime? endDate);
        IIncludableQueryable<Admin, ApplicationUser> GetFilteredAdminsQuery(AdminFilterRequestDto filterDto);
    }
}
