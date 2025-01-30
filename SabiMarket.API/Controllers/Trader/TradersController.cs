using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.API.ServiceExtensions;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.Interfaces;
using TraderDetailsDto = SabiMarket.Application.DTOs.Requests.TraderDetailsDto;

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
        [ProducesResponseType(typeof(BaseResponse<TraderDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse<TraderDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<TraderDto>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTrader([FromBody] CreateTraderDto request)
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
        [ProducesResponseType(typeof(BaseResponse<TraderDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<TraderDetailsDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<TraderDetailsDto>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTraderById(string traderId)
        {
            var response = await _traderService.GetTraderDetails(traderId);
            return !response.IsSuccessful ? NotFound(response) : Ok(response);
        }

        [HttpPut("{traderId}")]
        [Authorize(Policy = PolicyNames.RequireMarketManagement)]
        [ProducesResponseType(typeof(BaseResponse<TraderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<TraderDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<TraderDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<TraderDto>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTrader(string traderId, [FromBody] UpdateTraderDto request)
        {
            var response = await _traderService.UpdateTrader(traderId, request);
            return !response.IsSuccessful ? BadRequest(response) : Ok(response);
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
        [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<TraderDto>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<TraderDto>>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTraders([FromQuery] string searchTerm, [FromQuery] PaginationFilter pagination)
        {
            var response = await _traderService.GetTraders(searchTerm, pagination);
            return Ok(response);
        }

        [HttpDelete("{traderId}")]
        [Authorize(Policy = PolicyNames.RequireMarketManagement)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTrader(string traderId)
        {
            var response = await _traderService.DeleteTrader(traderId);
            return !response.IsSuccessful ? NotFound(response) : Ok(response);
        }
    }
}