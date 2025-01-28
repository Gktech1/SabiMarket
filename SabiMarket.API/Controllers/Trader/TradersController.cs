using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.API.ServiceExtensions;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.Interfaces;

namespace SabiMarket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(Policy = PolicyNames.RequireTraderOnly)]
    public class TradersController : ControllerBase
    {
        private readonly ITraderService _traderService;
        private readonly ILogger<TradersController> _logger;

        public TradersController(ITraderService traderService, ILogger<TradersController> logger)
        {
            _traderService = traderService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Policy = PolicyNames.RequireMarketManagement)]
        [ProducesResponseType(typeof(BaseResponse<TraderResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse<TraderResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<TraderResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTrader([FromBody] CreateTraderRequestDto request)
        {
            var response = await _traderService.CreateTrader(request);
            return !response.IsSuccessful ? BadRequest(response) :
                CreatedAtAction(nameof(GetTraderById), new { traderId = response.Data.Id }, response);
        }

        [HttpGet("dashboard/{traderId}")]
        [ProducesResponseType(typeof(BaseResponse<TraderDashboardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<TraderDashboardDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<TraderDashboardDto>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDashboardStats(string traderId)
        {
            var response = await _traderService.GetDashboardStats(traderId);
            return !response.IsSuccessful ? NotFound(response) : Ok(response);
        }

        [HttpGet("{traderId}")]
        [ProducesResponseType(typeof(BaseResponse<TraderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<TraderResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<TraderResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTraderById(string traderId)
        {
            var response = await _traderService.GetTraderById(traderId);
            return !response.IsSuccessful ? NotFound(response) : Ok(response);
        }

        [HttpPut("{traderId}/profile")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProfile(string traderId, [FromBody] UpdateTraderProfileDto request)
        {
            var response = await _traderService.UpdateTraderProfile(traderId, request);
            return !response.IsSuccessful ? BadRequest(response) : Ok(response);
        }

        [HttpGet]
        [Authorize(Policy = PolicyNames.RequireMarketManagement)]
        [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTraders([FromQuery] TraderFilterRequestDto filter, [FromQuery] PaginationFilter pagination)
        {
            var response = await _traderService.GetTraders(filter, pagination);
            return Ok(response);
        }
    }
}