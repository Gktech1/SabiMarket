﻿using Microsoft.AspNetCore.Mvc;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IServices;

namespace SabiMarket.API.Utility
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UtilityController : ControllerBase
    {
        private readonly IChairmanService _chairmanService;
        private readonly ILogger<ChairmanController> _logger;

        public UtilityController(
            IChairmanService chairmanService,
            ILogger<ChairmanController> logger)
        {
            _chairmanService = chairmanService;
            _logger = logger;
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
    }
}
