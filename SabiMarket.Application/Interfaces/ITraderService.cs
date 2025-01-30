using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using TraderDetailsDto = SabiMarket.Application.DTOs.Requests.TraderDetailsDto;

namespace SabiMarket.Application.Interfaces
{
    public interface ITraderService
    {
        Task<BaseResponse<TraderResponseDto>> GetTraderById(string traderId);
        Task<BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>> GetTraders(
            TraderFilterRequestDto filterDto, PaginationFilter paginationFilter);
        Task<BaseResponse<TraderDashboardDto>> GetDashboardStats(string traderId);
        Task<BaseResponse<TraderDto>> CreateTrader(CreateTraderDto createDto);

        Task<BaseResponse<TraderDto>> UpdateTrader(string traderId, UpdateTraderDto updateDto);
        Task<BaseResponse<bool>> UpdateTraderProfile(string traderId, UpdateTraderProfileDto profileDto);
        Task<BaseResponse<TraderDetailsDto>> GetTraderDetails(string traderId);
        Task<BaseResponse<bool>> DeleteTrader(string traderId);
        Task<BaseResponse<PaginatorDto<IEnumerable<TraderDto>>>> GetTraders(
        string searchTerm, PaginationFilter paginationFilter);
    }
}
