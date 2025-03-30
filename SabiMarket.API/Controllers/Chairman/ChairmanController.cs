﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.API.ServiceExtensions;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IServices;
using SabiMarket.Domain.Exceptions;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize(Policy = PolicyNames.RequireMarketManagement)]
public class ChairmanController : ControllerBase
{
    private readonly IChairmanService _chairmanService;
    private readonly ILogger<ChairmanController> _logger;

    public ChairmanController(
        IChairmanService chairmanService,
        ILogger<ChairmanController> logger)
    {
        _chairmanService = chairmanService;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard statistics for the logged-in chairman
    /// </summary>
    /// <returns>Dashboard statistics including trader counts, caretaker counts, and revenue metrics</returns>
    [HttpGet("chairmandashboardstats")]
    [ProducesResponseType(typeof(BaseResponse<ChairmanDashboardStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<ChairmanDashboardStatsDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<ChairmanDashboardStatsDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<ChairmanDashboardStatsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChairmanDashboardStats(string chairmanId)
    {
        var response = await _chairmanService.GetChairmanDashboardStats(chairmanId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);

    }

    [HttpGet("getall-localgovernments")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLocalGovernments([FromQuery] LGAFilterRequestDto filterDto, [FromQuery] PaginationFilter paginationFilter)
    {
        var response = await _chairmanService.GetLocalGovernments(filterDto, paginationFilter);
        return !response.IsSuccessful
            ? StatusCode(StatusCodes.Status500InternalServerError, response)
            : Ok(response);
    }
    /// <summary>
    /// Search levy payments for a chairman's market
    /// </summary>
    /// <param name="chairmanId">ID of the chairman</param>
    /// <param name="query">Search term to filter levy payments</param>
    /// <param name="pageNumber">Page number for pagination (defaults to 1)</param>
    /// <param name="pageSize">Number of items per page (defaults to 10)</param>
    /// <returns>Paginated list of levy payments matching the search criteria</returns>
    [HttpGet("searchlevies")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyPaymentDetailDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyPaymentDetailDto>>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyPaymentDetailDto>>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyPaymentDetailDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchLevyPayments(
        string chairmanId,
        [FromQuery] string query,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var paginationFilter = new PaginationFilter
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber,
            PageSize = pageSize > 50 ? 50 : (pageSize < 1 ? 10 : pageSize)
        };

        var response = await _chairmanService.SearchLevyPayments(chairmanId, query, paginationFilter);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("search-localgovernmentarea")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLocalGovernmentArea([FromQuery] string search, [FromQuery] PaginationFilter paginationFilter)
    {
        var response = await _chairmanService.GetLocalGovernmentAreas(search, paginationFilter);
        return !response.IsSuccessful
            ? StatusCode(StatusCodes.Status500InternalServerError, response)
            : Ok(response);
    }

    [HttpGet("localgovernment/{id}")]
    [ProducesResponseType(typeof(BaseResponse<LGAResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<LGAResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<LGAResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLocalGovernmentById(string id)
    {
        var response = await _chairmanService.GetLocalGovernmentById(id);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpDelete("{chairmanId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteChairman(string chairmanId)
    {
        var response = await _chairmanService.DeleteChairmanByAdmin(chairmanId);

        if (!response.IsSuccessful)
        {
            return StatusCode(response.Error.StatusCode, response);
        }

        return Ok(response);
    }

    [HttpPost("markets")]
    [ProducesResponseType(typeof(BaseResponse<MarketResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<MarketResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<MarketResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateMarket([FromBody] CreateMarketRequestDto request)
    {
        var response = await _chairmanService.CreateMarket(request);
        return !response.IsSuccessful ? BadRequest(response) : CreatedAtAction(nameof(GetMarketDetails), new { marketId = response.Data.Id }, response);
    }

    [HttpPut("markets/{marketId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMarket(string marketId, [FromBody] UpdateMarketRequestDto request)
    {
        var response = await _chairmanService.UpdateMarket(marketId, request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpGet("markets/{marketId}/traders")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTraders(string marketId, [FromQuery] PaginationFilter filter)
    {
        var response = await _chairmanService.GetTraders(marketId, filter);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("markets/{marketId}/metrics")]
    [ProducesResponseType(typeof(BaseResponse<ReportMetricsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<ReportMetricsDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<ReportMetricsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetReportMetrics(string marketId, [FromQuery] DateRangeDto dateRange)
    {
        var response = await _chairmanService.GetReportMetrics(dateRange.StartDate, dateRange.EndDate);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpDelete("markets/{marketId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteMarket(string marketId)
    {
        var response = await _chairmanService.DeleteMarket(marketId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("markets/{marketId}/levy-setups")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyInfoResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyInfoResponseDto>>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyInfoResponseDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMarketLevies(string marketId, [FromQuery] PaginationFilter filter)
    {
        var response = await _chairmanService.GetMarketLevies(marketId, filter);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpGet("daily-metrics")]
    [ProducesResponseType(typeof(BaseResponse<DashboardMetricsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<DashboardMetricsResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDailyMetricsChange()
    {
        var response = await _chairmanService.GetDailyMetricsChange();
        return !response.IsSuccessful ?
            StatusCode(StatusCodes.Status500InternalServerError, response) :
            Ok(response);
    }

    [HttpGet("export-report")]
    [ProducesResponseType(typeof(BaseResponse<byte[]>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<byte[]>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<byte[]>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportReport([FromQuery] ReportExportRequestDto request)
    {
        var response = await _chairmanService.ExportReport(request);
        if (!response.IsSuccessful)
            return BadRequest(response);

        return File(
            response.Data,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"market_report_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
        );
    }

    [HttpGet("markets/{marketId}/revenue")]
    [ProducesResponseType(typeof(BaseResponse<MarketRevenueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<MarketRevenueDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<MarketRevenueDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMarketRevenue(string marketId, [FromQuery] DateRangeDto dateRange)
    {
        var response = await _chairmanService.GetMarketRevenue(marketId, dateRange);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpPost("levy-setup")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), statusCode: StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfigureLevySetup([FromBody] LevySetupRequestDto request)
    {
        var response = await _chairmanService.ConfigureLevySetup(request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpGet("levy-setups")]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<LevySetupResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<LevySetupResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLevySetups()
    {
        var response = await _chairmanService.GetLevySetups();
        return !response.IsSuccessful ?
            StatusCode(StatusCodes.Status500InternalServerError, response) :
            Ok(response);
    }

    [HttpGet("traders/{traderId}/details")]
    [ProducesResponseType(typeof(BaseResponse<TraderDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<TraderDetailsDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<TraderDetailsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTraderDetails(string traderId)
    {
        var response = await _chairmanService.GetTraderDetails(traderId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("markets/{marketId}/details")]
    [ProducesResponseType(typeof(BaseResponse<MarketDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<MarketDetailsDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<MarketDetailsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMarketDetails(string marketId)
    {
        var response = await _chairmanService.GetMarketDetails(marketId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("markets/search")]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<MarketResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<MarketResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchMarkets([FromQuery] string searchTerm)
    {
        var response = await _chairmanService.SearchMarkets(searchTerm);
        return !response.IsSuccessful ?
            StatusCode(StatusCodes.Status500InternalServerError, response) :
            Ok(response);
    }

    [HttpGet("traders/{traderId}/qrcode")]
    [ProducesResponseType(typeof(BaseResponse<QRCodeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<QRCodeResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<QRCodeResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateTraderQRCode(string traderId)
    {
        var response = await _chairmanService.GenerateTraderQRCode(traderId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpPost("levy")]
    [ProducesResponseType(typeof(BaseResponse<LevyResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<LevyResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<LevyResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateLevy([FromBody] CreateLevyRequestDto request)
    {
        var response = await _chairmanService.CreateLevy(request);
        return !response.IsSuccessful ? BadRequest(response) : CreatedAtAction(nameof(GetLevyById), new { id = response.Data.Id }, response);
    }

    [HttpPut("levy/{levyId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLevy(string levyId, [FromBody] UpdateLevyRequestDto request)
    {
        var response = await _chairmanService.UpdateLevy(levyId, request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpDelete("levy/{levyId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteLevy(string levyId)
    {
        var response = await _chairmanService.DeleteLevy(levyId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("levy/{levyId}")]
    [ProducesResponseType(typeof(BaseResponse<LevyResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<LevyResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<LevyResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLevyById(string levyId)
    {
        var response = await _chairmanService.GetLevyById(levyId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("chairman/{chairmanId}/levies")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyResponseDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllLevies(string chairmanId, [FromQuery] PaginationFilter filter)
    {
        var response = await _chairmanService.GetAllLevies(chairmanId, filter);
        return !response.IsSuccessful ? StatusCode(StatusCodes.Status500InternalServerError, response) : Ok(response);
    }

    [HttpGet("assistant-officer/{id}")]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssistantOfficerById(string id)
    {
        var response = await _chairmanService.GetAssistantOfficerById(id);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpPost("createassistant-officer")]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAssistantOfficer([FromBody] CreateAssistantOfficerRequestDto request)
    {
        var response = await _chairmanService.CreateAssistantOfficer(request);
        return !response.IsSuccessful ? BadRequest(response) : CreatedAtAction(nameof(GetAssistantOfficerById), new { id = response.Data.Id }, response);
    }

    [HttpPut("updateassistant-officer/{officerId}")]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAssistantOfficer(string officerId, [FromBody] UpdateAssistantOfficerRequestDto request)
    {
        var response = await _chairmanService.UpdateAssistantOfficer(officerId, request);

        if (!response.IsSuccessful)
        {
            _logger.LogWarning($"Update assistant officer failed: {response.Message}");

            // Return specific status codes based on error types
            if (response.Error is NotFoundException)
                return NotFound(response);
            else if (response.Error is BadRequestException)
                return BadRequest(response);
            else if (response.Error is UnauthorizedAccessException)
                return Unauthorized(response);
            else
                return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        return Ok(response);
    }

    [HttpGet("assistant-officer/{officerId}")]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssistantOfficer(string officerId)
    {
        var response = await _chairmanService.GetAssistantOfficerById(officerId);

        if (!response.IsSuccessful)
        {
            _logger.LogWarning($"Get assistant officer failed: {response.Message}");

            if (response.Error is NotFoundException)
                return NotFound(response);
            else
                return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        return Ok(response);
    }

    [HttpPatch("assistant-officer/{officerId}/unblock")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UnblockAssistantOfficer(string officerid)
    {
        var response = await _chairmanService.UnblockAssistantOfficer(officerid);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }


    [HttpPatch("assistant-officer/{officerId}/block")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> BlockAssistantOfficer(string officerid)
    {
        var response = await _chairmanService.BlockAssistantOfficer(officerid);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("chairman/{id}/reports")]
    [ProducesResponseType(typeof(BaseResponse<ReportResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<ReportResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChairmanReports(string id)
    {
        var response = await _chairmanService.GetChairmanReports(id);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("dashboard-metrics")]
    [ProducesResponseType(typeof(BaseResponse<DashboardMetricsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<DashboardMetricsResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDashboardMetrics()
    {
        var response = await _chairmanService.GetDashboardMetrics();
        return !response.IsSuccessful ? StatusCode(StatusCodes.Status500InternalServerError, response) : Ok(response);
    }

    [HttpGet("markets")]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<MarketResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<MarketResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllMarkets(string? localgovernmentId, string? searchTerm)
    {
        var response = await _chairmanService.GetAllMarkets(localgovernmentId!, searchTerm!);
        return !response.IsSuccessful ? StatusCode(StatusCodes.Status500InternalServerError, response) : Ok(response);
    }

    [HttpPost("markets/{marketId}/assign-caretaker/{caretakerId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignCaretakerToMarket(string marketId, string caretakerId)
    {
        var response = await _chairmanService.AssignCaretakerToMarket(marketId, caretakerId);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpGet("caretakers")]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<CaretakerResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<CaretakerResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCaretakers()
    {
        var response = await _chairmanService.GetAllCaretakers();
        return !response.IsSuccessful ? StatusCode(StatusCodes.Status500InternalServerError, response) : Ok(response);
    }

    [HttpGet("chairman/{id}")]
    [ProducesResponseType(typeof(BaseResponse<ChairmanResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<ChairmanResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<ChairmanResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChairmanById(string id)
    {
        var response = await _chairmanService.GetChairmanById(id);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpPost("create-chairman")]
    [Authorize(Policy = PolicyNames.RequireAdminOnly)]
    [ProducesResponseType(typeof(BaseResponse<ChairmanResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<ChairmanResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<ChairmanResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateChairman([FromBody] CreateChairmanRequestDto request)
    {
        var response = await _chairmanService.CreateChairman(request);
        return !response.IsSuccessful ? BadRequest(response) : CreatedAtAction(nameof(GetChairmanById), new { id = response.Data.Id }, response);
    }

    [HttpPut("{id}/updatechairman-profile")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateChairmanProfile(string id, [FromBody] UpdateProfileDto profileDto)
    {
        var response = await _chairmanService.UpdateChairmanProfile(id, profileDto);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpGet("chairmen")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<AdminDashboardResponse>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<AdminDashboardResponse>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChairmen([FromQuery] string? searchTerm, [FromQuery] PaginationFilter paginationFilter)
    {
        var response = await _chairmanService.GetChairmen(searchTerm, paginationFilter);
        return Ok(response);
    }

    [HttpPost("{id}/assign-caretaker/{caretakerId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignCaretakerToChairman(string id, string caretakerId)
    {
        var response = await _chairmanService.AssignCaretakerToChairman(id, caretakerId);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }
}
