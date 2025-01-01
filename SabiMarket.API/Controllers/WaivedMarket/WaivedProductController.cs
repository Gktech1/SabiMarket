using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Interfaces;

namespace SabiMarket.API.Controllers.WaivedMarket
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WaivedProductController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        public WaivedProductController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpGet("GetWaivedProductById")]
        public async Task<IActionResult> GetWaivedProductById([FromQuery] string id)
        {
            var result = await _serviceManager.IWaivedProductService.GetWaivedProductById(id);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result.Data));
        }

        [HttpGet("GetWaivedProducts")]
        public async Task<IActionResult> GetWaivedProducts([FromQuery] string? category, [FromQuery] PaginationFilter filter)
        {
            var result = await _serviceManager.IWaivedProductService.GetAllWaivedProducts(category, filter);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result.Data));
        }

        [HttpPost("CreateWaivedProducts")]
        public async Task<IActionResult> CreateWaivedProducts(CreateWaivedProductDto dto)
        {
            var result = await _serviceManager.IWaivedProductService.CreateWaivedProduct(dto);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result));
        }

        [HttpPut("UpdateWaivedProducts")]
        public async Task<IActionResult> UpdateWaivedProducts(UpdateWaivedProductDto dto)
        {
            var result = await _serviceManager.IWaivedProductService.UpdateProduct(dto);
            if (result.IsFailure)
                return BadRequest(ResponseDto<object>.Failure(result.Errors));

            return Ok(ResponseDto<object>.Success(result));
        }
    }
}
