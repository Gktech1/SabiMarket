using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.API.ServiceExtensions;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses.SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IServices;

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

    [HttpPost("assistant-officer")]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAssistantOfficer([FromBody] CreateAssistantOfficerRequestDto request)
    {
        var response = await _chairmanService.CreateAssistantOfficer(request);
        return !response.IsSuccessful ? BadRequest(response) : CreatedAtAction(nameof(GetAssistantOfficerById), new { id = response.Data.Id }, response);
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
    public async Task<IActionResult> GetAllMarkets()
    {
        var response = await _chairmanService.GetAllMarkets();
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
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<ChairmanResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<ChairmanResponseDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChairmen([FromQuery] PaginationFilter paginationFilter)
    {
        var response = await _chairmanService.GetChairmen(paginationFilter);
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
