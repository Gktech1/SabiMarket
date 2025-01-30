using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.API.ServiceExtensions;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Interfaces;
using SabiMarket.Domain.Entities.LevyManagement;

namespace SabiMarket.API.Controllers
{
    [Route("api/assist-centre-office")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(Policy = PolicyNames.RequireMarketManagement)]
    public class AssistCentreOfficeController : ControllerBase
    {
        private readonly IAssistCenterOfficerService _officerService;
        private readonly ILogger<AssistCentreOfficeController> _logger;

        public AssistCentreOfficeController(
            IAssistCenterOfficerService officerService,
            ILogger<AssistCentreOfficeController> logger)
        {
            _officerService = officerService;
            _logger = logger;
        }

        /// <summary>
        /// Gets dashboard statistics for an officer
        /// </summary>
        [HttpGet("dashboard/{officerId}")]
        [ProducesResponseType(typeof(BaseResponse<DashboardStatsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<DashboardStatsDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<DashboardStatsDto>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDashboardStats(string officerId)
        {
            var response = await _officerService.GetDashboardStats(officerId);

            if (!response.IsSuccessful)
            {
                if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(response);
                }
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Processes a new levy payment
        /// </summary>
        [HttpPost("levy-payments/process")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessLevyPayment([FromBody] ProcessLevyPaymentDto paymentDto)
        {

            var response = await _officerService.ProcessLevyPayment(paymentDto);

            if (!response.IsSuccessful)
            {
                if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(response);
                }
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Gets levy payments for a market with optional filtering
        /// </summary>
        [HttpGet("levy-payments/market/{marketId}")]
        [ProducesResponseType(typeof(BaseResponse<IEnumerable<LevyPaymentResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<IEnumerable<LevyPaymentResponseDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<IEnumerable<LevyPaymentResponseDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLevyPayments(
            string marketId,
            [FromQuery] LevyPaymentFilterDto filterDto)
        {
            var response = await _officerService.GetLevyPayments(marketId, filterDto);

            if (!response.IsSuccessful)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}