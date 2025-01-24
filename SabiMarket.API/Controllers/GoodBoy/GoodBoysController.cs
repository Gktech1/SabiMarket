using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.API.ServiceExtensions;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.Interfaces;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize(Policy = PolicyNames.RequireMarketStaff)]
public class GoodBoysController : ControllerBase
{
    private readonly IGoodBoysService _goodBoysService;
    private readonly ILogger<GoodBoysController> _logger;

    public GoodBoysController(IGoodBoysService goodBoysService, ILogger<GoodBoysController> logger)
    {
        _goodBoysService = goodBoysService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Policy = PolicyNames.RequireMarketManagement)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateGoodBoy([FromBody] CreateGoodBoyRequestDto request)
    {
        var response = await _goodBoysService.CreateGoodBoy(request);
        return !response.IsSuccessful ? BadRequest(response) : CreatedAtAction(nameof(GetGoodBoyById), new { goodBoyId = response.Data.Id }, response);
    }

    [HttpGet("{goodBoyId}")]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetGoodBoyById(string goodBoyId)
    {
        var response = await _goodBoysService.GetGoodBoyById(goodBoyId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpPut("{goodBoyId}/profile")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProfile(string goodBoyId, [FromBody] UpdateGoodBoyProfileDto request)
    {
        var response = await _goodBoysService.UpdateGoodBoyProfile(goodBoyId, request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpGet]
    [Authorize(Policy = PolicyNames.RequireMarketManagement)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetGoodBoys([FromQuery] GoodBoyFilterRequestDto filter, [FromQuery] PaginationFilter pagination)
    {
        var response = await _goodBoysService.GetGoodBoys(filter, pagination);
        return Ok(response);
    }

    [HttpPost("scan-qr")]
    [ProducesResponseType(typeof(BaseResponse<TraderQRValidationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<TraderQRValidationResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<TraderQRValidationResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ScanTraderQRCode([FromBody] ScanTraderQRCodeDto request)
    {
        var response = await _goodBoysService.ValidateTraderQRCode(request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpGet("traders/{traderId}")]
    [ProducesResponseType(typeof(BaseResponse<TraderDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<TraderDetailsDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<TraderDetailsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTraderDetails(string traderId)
    {
        var response = await _goodBoysService.GetTraderDetails(traderId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("traders/{traderId}/payment-status")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyTraderPaymentStatus(string traderId)
    {
        var response = await _goodBoysService.VerifyTraderPaymentStatus(traderId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpPost("traders/{traderId}/payments")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessPayment(string traderId, [FromBody] ProcessLevyPaymentDto request)
    {
        var response = await _goodBoysService.UpdateTraderPayment(traderId, request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }
}