using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;

namespace SabiMarket.Application.Interfaces
{
    public interface IGoodBoysService
    {
        Task<BaseResponse<GoodBoyResponseDto>> GetGoodBoyById(string goodBoyId);
        Task<BaseResponse<GoodBoyResponseDto>> CreateGoodBoy(CreateGoodBoyRequestDto goodBoyDto);
        Task<BaseResponse<bool>> UpdateGoodBoyProfile(string goodBoyId, UpdateGoodBoyProfileDto profileDto);
        Task<BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>> GetGoodBoys(
            GoodBoyFilterRequestDto filterDto, PaginationFilter paginationFilter);
        Task<BaseResponse<TraderDetailsDto>> GetTraderDetails(string traderId);
        Task<BaseResponse<bool>> ProcessLevyPayment(string goodBoyId, ProcessLevyPaymentDto paymentDto);
        Task<BaseResponse<TraderQRValidationResponseDto>> ValidateTraderQRCode(ScanTraderQRCodeDto scanDto);
        Task<BaseResponse<bool>> VerifyTraderPaymentStatus(string traderId);
        Task<BaseResponse<bool>> UpdateTraderPayment(string traderId, ProcessLevyPaymentDto paymentDto);
        Task<BaseResponse<DashboardStatsDto>> GetDashboardStats(string goodBoyId);
    }

}


