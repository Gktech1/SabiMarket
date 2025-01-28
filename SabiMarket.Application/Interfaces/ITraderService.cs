using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;

namespace SabiMarket.Application.Interfaces
{
    public interface ITraderService
    {
        Task<BaseResponse<TraderResponseDto>> GetTraderById(string traderId);
        Task<BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>> GetTraders(
            TraderFilterRequestDto filterDto, PaginationFilter paginationFilter);
        Task<BaseResponse<TraderDashboardDto>> GetDashboardStats(string traderId);
        Task<BaseResponse<TraderResponseDto>> CreateTrader(CreateTraderRequestDto traderDto);
        Task<BaseResponse<bool>> UpdateTraderProfile(string traderId, UpdateTraderProfileDto profileDto);
    }
}
