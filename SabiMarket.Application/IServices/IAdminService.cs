using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;

namespace SabiMarket.Application.IServices
{
    public interface IAdminService
    {
        Task<BaseResponse<AdminResponseDto>> GetAdminById(string adminId);
        Task<BaseResponse<AdminResponseDto>> CreateAdmin(CreateAdminRequestDto adminDto);
        Task<BaseResponse<bool>> UpdateAdminProfile(string adminId, UpdateAdminProfileDto profileDto);
        Task<BaseResponse<PaginatorDto<IEnumerable<AdminResponseDto>>>> GetAdmins(
            AdminFilterRequestDto filterDto, PaginationFilter paginationFilter);
        Task<BaseResponse<AdminDashboardStatsDto>> GetDashboardStats(string adminId);
        Task<BaseResponse<bool>> UpdateDashboardAccess(string adminId, UpdateAdminAccessDto accessDto);
        Task<BaseResponse<bool>> DeactivateAdmin(string adminId);
        Task<BaseResponse<bool>> ReactivateAdmin(string adminId);
        Task<BaseResponse<PaginatorDto<IEnumerable<AuditLogResponseDto>>>> GetAdminAuditLogs(
            string adminId, DateTime? startDate, DateTime? endDate, PaginationFilter paginationFilter);
    }
}


