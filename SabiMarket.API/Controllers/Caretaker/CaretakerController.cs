using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.IServices;

namespace SabiMarket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class CaretakerController : ControllerBase
    {
        private readonly ICaretakerService _caretakerService;
        private readonly ILogger<CaretakerController> _logger;

        public CaretakerController(
            ICaretakerService caretakerService,
            ILogger<CaretakerController> logger)
        {
            _caretakerService = caretakerService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new caretaker
        /// </summary>
        /// <param name="request">Caretaker creation details</param>
        /// <returns>Created caretaker information</returns>
        [HttpPost("createcaretaker")]
        [ProducesResponseType(typeof(BaseResponse<CaretakerResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<CaretakerResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<CaretakerResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCaretaker([FromBody] CaretakerForCreationRequestDto request)
        {
            var response = await _caretakerService.CreateCaretaker(request);
            return !response.IsSuccessful ? BadRequest(response) : Ok(response);
        }

        /// <summary>
        /// Retrieves a caretaker by ID
        /// </summary>
        /// <param name="id">Caretaker ID</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResponse<CaretakerResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<CaretakerResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<CaretakerResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCaretaker(string id)
        {
            var response = await _caretakerService.GetCaretakerById(id);
            return !response.IsSuccessful ? NotFound(response) : Ok(response);
        }

        /// <summary>
        /// Retrieves a paginated list of caretakers
        /// </summary>
        /// <param name="paginationFilter">Pagination parameters</param>
        [HttpGet("getcaretakers")]
        [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<CaretakerResponseDto>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<CaretakerResponseDto>>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCaretakers([FromQuery] PaginationFilter paginationFilter)
        {
            var response = await _caretakerService.GetCaretakers(paginationFilter);
            return Ok(response);
        }

        /// <summary>
        /// Assigns a trader to a caretaker
        /// </summary>
        /// <param name="caretakerId">Caretaker ID</param>
        /// <param name="traderId">Trader ID to assign</param>
        [HttpPost("{caretakerId}/traders/{traderId}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignTrader(string caretakerId, string traderId)
        {
            var response = await _caretakerService.AssignTraderToCaretaker(caretakerId, traderId);
            return !response.IsSuccessful ? BadRequest(response) : Ok(response);
        }

        /// <summary>
        /// Removes a trader from a caretaker
        /// </summary>
        /// <param name="caretakerId">Caretaker ID</param>
        /// <param name="traderId">Trader ID to remove</param>
        [HttpDelete("{caretakerId}/traders/{traderId}")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveTrader(string caretakerId, string traderId)
        {
            var response = await _caretakerService.RemoveTraderFromCaretaker(caretakerId, traderId);
            return !response.IsSuccessful ? NotFound(response) : Ok(response);
        }

        /// <summary>
        /// Gets paginated list of traders assigned to a caretaker
        /// </summary>
        /// <param name="caretakerId">Caretaker ID</param>
        /// <param name="paginationFilter">Pagination parameters</param>
        [HttpGet("{caretakerId}/traders")]
        [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAssignedTraders(string caretakerId, [FromQuery] PaginationFilter paginationFilter)
        {
            var response = await _caretakerService.GetAssignedTraders(caretakerId, paginationFilter);
            return !response.IsSuccessful ? NotFound(response) : Ok(response);
        }

        /// <summary>
        /// Gets paginated list of levy payments for a caretaker
        /// </summary>
        /// <param name="caretakerId">Caretaker ID</param>
        /// <param name="paginationFilter">Pagination parameters</param>
        [HttpGet("{caretakerId}/levy-payments")]
        [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyPaymentResponseDto>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyPaymentResponseDto>>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLevyPayments(string caretakerId, [FromQuery] PaginationFilter paginationFilter)
        {
            var response = await _caretakerService.GetLevyPayments(caretakerId, paginationFilter);
            return !response.IsSuccessful ? NotFound(response) : Ok(response);
        }

        /// <summary>
        /// Gets details of a specific levy payment
        /// </summary>
        /// <param name="levyId">Levy payment ID</param>
        [HttpGet("levy-payments/{levyId}")]
        [ProducesResponseType(typeof(BaseResponse<LevyPaymentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<LevyPaymentResponseDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLevyPaymentDetails(string levyId)
        {
            var response = await _caretakerService.GetLevyPaymentDetails(levyId);
            return !response.IsSuccessful ? NotFound(response) : Ok(response);
        }

        /// <summary>
        /// Creates a new GoodBoy associated with a caretaker
        /// </summary>
        /// <param name="caretakerId">Caretaker ID</param>
        /// <param name="request">GoodBoy creation details</param>
        [HttpPost("{caretakerId}/goodboys")]
        [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddGoodBoy(string caretakerId, [FromBody] CreateGoodBoyDto request)
        {
            var response = await _caretakerService.AddGoodBoy(caretakerId, request);
            return !response.IsSuccessful ? BadRequest(response) : Ok(response);
        }

        /// <summary>
        /// Gets paginated list of GoodBoys for a caretaker
        /// </summary>
        /// <param name="caretakerId">Caretaker ID</param>
        /// <param name="paginationFilter">Pagination parameters</param>
        [HttpGet("{caretakerId}/goodboys")]
        [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGoodBoys(string caretakerId, [FromQuery] PaginationFilter paginationFilter)
        {
            var response = await _caretakerService.GetGoodBoys(caretakerId, paginationFilter);
            return !response.IsSuccessful ? NotFound(response) : Ok(response);
        }

        /// <summary>
        /// Blocks a GoodBoy associated with a caretaker
        /// </summary>
        /// <param name="caretakerId">Caretaker ID</param>
        /// <param name="goodBoyId">GoodBoy ID to block</param>
        [HttpPatch("{caretakerId}/goodboys/{goodBoyId}/block")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BlockGoodBoy(string caretakerId, string goodBoyId)
        {
            var response = await _caretakerService.BlockGoodBoy(caretakerId, goodBoyId);
            return !response.IsSuccessful ? NotFound(response) : Ok(response);
        }
    }
}