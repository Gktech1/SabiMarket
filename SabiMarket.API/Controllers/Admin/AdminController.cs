using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.API.ServiceExtensions;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IServices;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize(Policy = PolicyNames.RequireAdminOnly)]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IAdminService adminService,
        ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    [HttpGet("{adminId}")]
    [ProducesResponseType(typeof(BaseResponse<AdminResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<AdminResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<AdminResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAdmin(string adminId)
    {
        var response = await _adminService.GetAdminById(adminId);
        if (!response.IsSuccessful)
        {
            return StatusCode(response.Error.StatusCode, response);
        }
        return Ok(response);
    }

    [HttpPost("createadmin")]
    [ProducesResponseType(typeof(BaseResponse<AdminResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<AdminResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<AdminResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequestDto request)
    {
        var response = await _adminService.CreateAdmin(request);
        if (!response.IsSuccessful)
        {
            return StatusCode(response.Error.StatusCode, response);
        }
        return CreatedAtAction(nameof(GetAdmin), new { adminId = response.Data.Id }, response);
    }

    [HttpPut("{adminId}/profile")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProfile(string adminId, [FromBody] UpdateAdminProfileDto request)
    {
        var response = await _adminService.UpdateAdminProfile(adminId, request);
        if (!response.IsSuccessful)
        {
            return StatusCode(response.Error.StatusCode, response);
        }
        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<AdminResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<AdminResponseDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAdmins([FromQuery] AdminFilterRequestDto filter, [FromQuery] PaginationFilter pagination)
    {
        var response = await _adminService.GetAdmins(filter, pagination);
        if (!response.IsSuccessful)
        {
            return StatusCode(response.Error.StatusCode, response);
        }
        return Ok(response);
    }

    [HttpGet("{adminId}/dashboard")]
    [ProducesResponseType(typeof(BaseResponse<AdminDashboardStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<AdminDashboardStatsDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse<AdminDashboardStatsDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<AdminDashboardStatsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDashboardStats(string adminId)
    {
        var response = await _adminService.GetDashboardStats(adminId);
        if (!response.IsSuccessful)
        {
            return StatusCode(response.Error.StatusCode, response);
        }
        return Ok(response);
    }

    [HttpPut("{adminId}/access")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAccess(string adminId, [FromBody] UpdateAdminAccessDto request)
    {
        var response = await _adminService.UpdateDashboardAccess(adminId, request);
        if (!response.IsSuccessful)
        {
            return StatusCode(response.Error.StatusCode, response);
        }
        return Ok(response);
    }

    [HttpPost("{adminId}/deactivate")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeactivateAdmin(string adminId)
    {
        var response = await _adminService.DeactivateAdmin(adminId);
        if (!response.IsSuccessful)
        {
            return StatusCode(response.Error.StatusCode, response);
        }
        return Ok(response);
    }

    [HttpPost("{adminId}/reactivate")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReactivateAdmin(string adminId)
    {
        var response = await _adminService.ReactivateAdmin(adminId);
        if (!response.IsSuccessful)
        {
            return StatusCode(response.Error.StatusCode, response);
        }
        return Ok(response);
    }

    [HttpGet("{adminId}/audit-logs")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<AuditLogResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<AuditLogResponseDto>>>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<AuditLogResponseDto>>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<AuditLogResponseDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAuditLogs(
        string adminId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] PaginationFilter pagination)
    {
        var response = await _adminService.GetAdminAuditLogs(adminId, startDate, endDate, pagination);
        if (!response.IsSuccessful)
        {
            return StatusCode(response.Error.StatusCode, response);
        }
        return Ok(response);
    }
}