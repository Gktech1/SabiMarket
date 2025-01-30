using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using TraderDetailsDto = SabiMarket.Application.DTOs.Requests.TraderDetailsDto;

namespace SabiMarket.Application.Interfaces
{
    public interface IAssistCenterOfficerService
    {
        Task<BaseResponse<DashboardStatsDto>> GetDashboardStats(string officerId);
        Task<BaseResponse<bool>> ProcessLevyPayment(ProcessLevyPaymentDto paymentDto);
        Task<BaseResponse<IEnumerable<LevyPaymentResponseDto>>> GetLevyPayments(
            string marketId,
            LevyPaymentFilterDto filterDto);
    }
}