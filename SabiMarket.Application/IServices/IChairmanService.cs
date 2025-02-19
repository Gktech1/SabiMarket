﻿using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;

namespace SabiMarket.Application.IServices
{
    public interface IChairmanService
    {
        Task<BaseResponse<LGAResponseDto>> GetLocalGovernmentById(string id);
        Task<BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>> GetLocalGovernmentAreas(
         LGAFilterRequestDto filterDto,
         PaginationFilter paginationFilter);
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
        Task<BaseResponse<MarketResponseDto>> CreateMarket(CreateMarketRequestDto request);
        Task<BaseResponse<bool>> UpdateMarket(string marketId, UpdateMarketRequestDto request);
        Task<BaseResponse<bool>> DeleteMarket(string marketId);
        Task<BaseResponse<MarketDetailsDto>> GetMarketDetails(string marketId);
        Task<BaseResponse<IEnumerable<MarketResponseDto>>> SearchMarkets(string searchTerm);
        Task<BaseResponse<MarketRevenueDto>> GetMarketRevenue(string marketId);

        // Trader Management
        Task<BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>> GetTraders(string marketId, PaginationFilter filter);
        Task<BaseResponse<TraderDetailsDto>> GetTraderDetails(string traderId);
        Task<BaseResponse<QRCodeResponseDto>> GenerateTraderQRCode(string traderId);

        // Settings Management
        Task<BaseResponse<PaginatorDto<IEnumerable<AuditLogDto>>>> GetAuditLogs(PaginationFilter filter);
        Task<BaseResponse<bool>> ConfigureLevySetup(LevySetupRequestDto request);
        Task<BaseResponse<IEnumerable<LevySetupResponseDto>>> GetLevySetups();

        // Report Generation
        Task<BaseResponse<ReportMetricsDto>> GetReportMetrics(DateTime startDate, DateTime endDate);
        Task<BaseResponse<byte[]>> ExportReport(ReportExportRequestDto request);
        Task<BaseResponse<ReportMetricsDto>> GetReportMetrics();
        Task<BaseResponse<DashboardMetricsResponseDto>> GetDailyMetricsChange();

        // Market Analytics
        Task<BaseResponse<MarketComplianceDto>> GetMarketComplianceRates(string marketId);
        Task<BaseResponse<MarketRevenueDto>> GetMarketRevenue(string marketId, DateRangeDto dateRange);

        // New levy management methods
        Task<BaseResponse<LevyResponseDto>> CreateLevy(CreateLevyRequestDto request);
        Task<BaseResponse<bool>> UpdateLevy(string levyId, UpdateLevyRequestDto request);
        Task<BaseResponse<bool>> DeleteLevy(string levyId);
        Task<BaseResponse<LevyResponseDto>> GetLevyById(string levyId);
        Task<BaseResponse<PaginatorDto<IEnumerable<LevyResponseDto>>>> GetAllLevies(string chairmanId, PaginationFilter filter);
        Task<BaseResponse<PaginatorDto<IEnumerable<LevyInfoResponseDto>>>> GetMarketLevies(string marketId, PaginationFilter paginationFilter);
        Task<BaseResponse<bool>> DeleteChairmanById(string chairmanId);
    }
}
