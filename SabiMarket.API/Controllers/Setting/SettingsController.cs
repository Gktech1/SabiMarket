﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.IServices;
using SabiMarket.Infrastructure.Helpers;

namespace SabiMarket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize] // Base authorization for all endpoints
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(
            ISettingsService settingsService,
            ICurrentUserService currentUser,
            ILogger<SettingsController> logger)
        {
            _settingsService = settingsService;
            _currentUser = currentUser;
            _logger = logger;
        }

        /// <summary>
        /// Changes the password for the authenticated user
        /// </summary>
        /// <param name="request">Password change details</param>
        /// <returns>Success status of the password change operation</returns>
        /// <response code="200">Password changed successfully</response>
        /// <response code="400">Invalid password details</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var userId = _currentUser.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ResponseFactory.Fail<bool>("User is not authenticated"));
            }

            var response = await _settingsService.ChangePassword(userId, request);

            if (!response.IsSuccessful)
            {
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status404NotFound => NotFound(response),
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        /// <summary>
        /// Updates the profile information for the authenticated user
        /// </summary>
        /// <param name="request">Profile update details</param>
        /// <returns>Success status of the profile update operation</returns>
        /// <response code="200">Profile updated successfully</response>
        /// <response code="400">Invalid profile details</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("update-profile")]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            var userId = _currentUser.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ResponseFactory.Fail<bool>("User is not authenticated"));
            }

            var response = await _settingsService.UpdateProfile(userId, request);

            if (!response.IsSuccessful)
            {
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status404NotFound => NotFound(response),
                    StatusCodes.Status400BadRequest => BadRequest(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }

        /// <summary>
        /// Retrieves the profile information for the authenticated user
        /// </summary>
        /// <returns>User profile information</returns>
        /// <response code="200">Profile retrieved successfully</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("getuserprofile")]
        [ProducesResponseType(typeof(BaseResponse<UserProfileResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<UserProfileResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<UserProfileResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserProfile()
        {
            var userId = _currentUser.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ResponseFactory.Fail<UserProfileResponseDto>("User is not authenticated"));
            }

            var response = await _settingsService.GetUserProfile(userId);

            if (!response.IsSuccessful)
            {
                return response.Error?.StatusCode switch
                {
                    StatusCodes.Status404NotFound => NotFound(response),
                    _ => BadRequest(response)
                };
            }

            return Ok(response);
        }
    }
}