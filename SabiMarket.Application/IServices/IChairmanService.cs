using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs.Responses.SabiMarket.Application.DTOs.Responses;

namespace SabiMarket.Application.IServices
{
    public interface IChairmanService
    {
        Task<BaseResponse<ChairmanResponseDto>> GetChairmanById(string chairmanId);
        Task<BaseResponse<ChairmanResponseDto>> CreateChairman(CreateChairmanRequestDto chairmanDto);
        Task<BaseResponse<bool>> UpdateChairmanProfile(string chairmanId, UpdateProfileDto profileDto);
        Task<BaseResponse<PaginatorDto<IEnumerable<ChairmanResponseDto>>>> GetChairmen(
            PaginationFilter paginationFilter);
        Task<BaseResponse<IEnumerable<MarketResponseDto>>> GetAllMarkets();
        Task<BaseResponse<DashboardMetricsResponseDto>> GetDashboardMetrics();
        Task<BaseResponse<bool>> AssignCaretakerToMarket(string marketId, string caretakerId);
        Task<BaseResponse<bool>> AssignCaretakerToChairman(string chairmanId, string caretakerId);
        Task<BaseResponse<IEnumerable<CaretakerResponseDto>>> GetAllCaretakers();
        Task<BaseResponse<IEnumerable<ReportResponseDto>>> GetChairmanReports(string chairmanId);
        Task<BaseResponse<bool>> UnblockAssistantOfficer(string officerId);
        Task<BaseResponse<bool>> BlockAssistantOfficer(string officerId);
        Task<BaseResponse<AssistantOfficerResponseDto>> CreateAssistantOfficer(CreateAssistantOfficerRequestDto officerDto);
        Task<BaseResponse<AssistantOfficerResponseDto>> GetAssistantOfficerById(string officerId);
        Task<BaseResponse<LevyResponseDto>> CreateLevy(CreateLevyRequestDto request);
        Task<BaseResponse<bool>> UpdateLevy(string levyId, UpdateLevyRequestDto request);
        Task<BaseResponse<bool>> DeleteLevy(string levyId);
        Task<BaseResponse<LevyResponseDto>> GetLevyById(string levyId);
        Task<BaseResponse<PaginatorDto<IEnumerable<LevyResponseDto>>>> GetAllLevies(string chairmanId, PaginationFilter filter);
    }
}
