using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.API.ServiceExtensions;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Advertisement;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Services.Interfaces;
using SabiMarket.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SabiMarket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AdvertisementController : ControllerBase
    {
        private readonly IAdvertisementService _advertisementService;
        private readonly ILogger<AdvertisementController> _logger;

        public AdvertisementController(
            IAdvertisementService advertisementService,
            ILogger<AdvertisementController> logger)
        {
            _advertisementService = advertisementService;
            _logger = logger;
        }

        /// <summary>
        /// Get all advertisements with pagination and filtering
        /// </summary>
        /// <param name="filterDto">Filter criteria for advertisements</param>
        /// <param name="paginationFilter">Pagination parameters</param>
        /// <returns>Paginated list of advertisements</returns>
        [HttpGet("get-advertisements")]
        [Authorize(Policy = PolicyNames.RequiredVendorCustomerAndAdmin)]
        [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAdvertisements(
            [FromQuery] AdvertisementFilterRequestDto filterDto,
            [FromQuery] PaginationFilter paginationFilter)
        {
            try
            {
                var result = await _advertisementService.GetAdvertisements(filterDto, paginationFilter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving advertisements");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ResponseFactory.Fail<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>(ex, "An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Get submitted advertisements with pagination and filtering
        /// </summary>
        /// <param name="filterDto">Filter criteria for advertisements</param>
        /// <param name="paginationFilter">Pagination parameters</param>
        /// <returns>Paginated list of submitted advertisements</returns>
        [HttpGet("submitted-advertisement")]
        [Authorize(Policy = PolicyNames.RequiredVendorCustomerAndAdmin)]
        [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSubmittedAdvertisements(
            [FromQuery] AdvertisementFilterRequestDto filterDto,
            [FromQuery] PaginationFilter paginationFilter)
        {
            try
            {
                var result = await _advertisementService.GetSubmittedAdvertisements(filterDto, paginationFilter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving submitted advertisements");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ResponseFactory.Fail<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>(ex, "An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Get archived advertisements with pagination and filtering
        /// </summary>
        /// <param name="filterDto">Filter criteria for advertisements</param>
        /// <param name="paginationFilter">Pagination parameters</param>
        /// <returns>Paginated list of archived advertisements</returns>
        [HttpGet("archived-advertisement")]
        [Authorize(Policy = PolicyNames.RequiredVendorCustomerAndAdmin)]
        [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetArchivedAdvertisements(
            [FromQuery] AdvertisementFilterRequestDto filterDto,
            [FromQuery] PaginationFilter paginationFilter)
        {
            try
            {
                var result = await _advertisementService.GetArchivedAdvertisements(filterDto, paginationFilter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving archived advertisements");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ResponseFactory.Fail<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>(ex, "An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Get advertisement by ID
        /// </summary>
        /// <param name="id">Advertisement ID</param>
        /// <returns>Detailed advertisement information</returns>
        [HttpGet("getadvertisementby/{id}")]
        [Authorize(Policy = PolicyNames.RequiredVendorCustomerAndAdmin)]
        [ProducesResponseType(typeof(BaseResponse<AdvertisementDetailResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAdvertisementById(string id)
        {
            try
            {
                var result = await _advertisementService.GetAdvertisementById(id);
                if (!result.IsSuccessful)
                    return NotFound(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving advertisement with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ResponseFactory.Fail<AdvertisementDetailResponseDto>(ex, "An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Create a new advertisement
        /// </summary>
        /// <param name="request">Advertisement creation data</param>
        /// <returns>Created advertisement information</returns>
        [HttpPost("create-advertisement")]
        [Authorize(Policy = PolicyNames.RequireVendorOnly)]
        [ProducesResponseType(typeof(BaseResponse<AdvertisementResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAdvertisement(
            [FromBody] CreateAdvertisementRequestDto request)
        {
            try
            {
                var result = await _advertisementService.CreateAdvertisement(request);
                if (!result.IsSuccessful)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating advertisement");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Update an existing advertisement
        /// </summary>
        /// <param name="request">Advertisement update data</param>
        /// <returns>Updated advertisement information</returns>
        [HttpPut("update-advertisement")]
        [Authorize(Policy = PolicyNames.RequireVendorOnly)]
        [ProducesResponseType(typeof(BaseResponse<AdvertisementResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAdvertisement(
            [FromBody] UpdateAdvertisementRequestDto request)
        {
            try
            {
                var result = await _advertisementService.UpdateAdvertisement(request);
                if (!result.IsSuccessful)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating advertisement");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Delete an advertisement
        /// </summary>
        /// <param name="id">Advertisement ID to delete</param>
        /// <returns>Deleted advertisement information</returns>
        [HttpDelete("delete/{id}")]
        [Authorize(Policy = PolicyNames.RequiredVendorCustomerAndAdmin)]
        [ProducesResponseType(typeof(BaseResponse<AdvertisementResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAdvertisement(string id)
        {
            try
            {
                var result = await _advertisementService.DeleteAdvertisement(id);
                if (!result.IsSuccessful)
                    return NotFound(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting advertisement");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Update advertisement status
        /// </summary>
        /// <param name="id">Advertisement ID</param>
        /// <param name="status">New status value</param>
        /// <returns>Updated advertisement information</returns>
        [HttpPatch("status/{id}")]
        [Authorize(Policy = PolicyNames.RequireAdminOnly)]
        [ProducesResponseType(typeof(BaseResponse<AdvertisementResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAdvertisementStatus(
            string id, [FromQuery] string status)
        {
            try
            {
                var result = await _advertisementService.UpdateAdvertisementStatus(id, status);
                if (!result.IsSuccessful)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating advertisement status");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Archive an advertisement
        /// </summary>
        /// <param name="id">Advertisement ID to archive</param>
        /// <returns>Archived advertisement information</returns>
        [HttpPost("archive/{id}")]
        [Authorize(Policy = PolicyNames.RequiredVendorCustomerAndAdmin)]
        [ProducesResponseType(typeof(BaseResponse<AdvertisementResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ArchiveAdvertisement(string id)
        {
            try
            {
                var result = await _advertisementService.ArchiveAdvertisement(id);
                if (!result.IsSuccessful)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving advertisement");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Restore an archived advertisement to active status
        /// </summary>
        /// <param name="id">Advertisement ID to restore</param>
        /// <returns>Restored advertisement information</returns>
        [HttpPost("restore/{id}")]
        [Authorize(Policy = PolicyNames.RequiredVendorCustomerAndAdmin)]
        [ProducesResponseType(typeof(BaseResponse<AdvertisementResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RestoreAdvertisement(string id)
        {
            try
            {
                var result = await _advertisementService.RestoreAdvertisement(id);
                if (!result.IsSuccessful)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring advertisement");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Upload payment proof for an advertisement
        /// </summary>
        /// <param name="request">Payment proof details</param>
        /// <param name="proofImage">Payment proof image file</param>
        /// <returns>Updated advertisement information</returns>
        [HttpPost("payment/proof")]
        [Authorize(Policy = PolicyNames.RequireVendorOnly)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(BaseResponse<AdvertisementResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadPaymentProof(
            [FromForm] UploadPaymentProofRequestDto request,
            IFormFile proofImage)
        {
            try
            {
                if (proofImage == null || proofImage.Length == 0)
                {
                    return BadRequest(ResponseFactory.Fail<AdvertisementResponseDto>(
                        new BadRequestException("Payment proof image is required"),
                        "Payment proof image is required"));
                }

                var result = await _advertisementService.UploadPaymentProof(request, proofImage);
                if (!result.IsSuccessful)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading payment proof");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Approve payment for an advertisement
        /// </summary>
        /// <param name="id">Advertisement ID</param>
        /// <returns>Updated advertisement information</returns>
        [HttpPost("payment/approve/{id}")]
        [Authorize(Policy = PolicyNames.RequireAdminOnly)]
        [ProducesResponseType(typeof(BaseResponse<AdvertisementResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ApprovePayment(string id)
        {
            try
            {
                var result = await _advertisementService.ApprovePayment(id);
                if (!result.IsSuccessful)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving payment");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred"));
            }
        }

        /// <summary>
        /// Reject payment for an advertisement
        /// </summary>
        /// <param name="id">Advertisement ID</param>
        /// <param name="reason">Reason for rejection</param>
        /// <returns>Updated advertisement information</returns>
        [HttpPost("payment/reject/{id}")]
        [Authorize(Policy = PolicyNames.RequireAdminOnly)]
        [ProducesResponseType(typeof(BaseResponse<AdvertisementResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RejectPayment(
            string id, [FromQuery] string reason)
        {
            try
            {
                var result = await _advertisementService.RejectPayment(id, reason);
                if (!result.IsSuccessful)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting payment");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred"));
            }
        }
    }
}