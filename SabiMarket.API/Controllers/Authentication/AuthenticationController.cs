using global::SabiMarket.Application.DTOs.Requests;
using global::SabiMarket.Application.DTOs.Responses;
using global::SabiMarket.Infrastructure.Utilities;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.Application.Interfaces;

namespace SabiMarket.API.Controllers.Authentication
{
    namespace SabiMarket.API.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        [Produces("application/json")]
        public class AuthenticationController : ControllerBase
        {
            private readonly IAuthenticationService _authService;
            private readonly ILogger<AuthenticationController> _logger;

            public AuthenticationController(
                IAuthenticationService authService,
                ILogger<AuthenticationController> logger)
            {
                _authService = authService;
                _logger = logger;
            }

            /// <summary>
            /// Authenticates an admin user and returns a JWT token
            /// </summary>
            /// <param name="request">Login credentials</param>
            /// <returns>Authentication token and user information</returns>
            /// <response code="200">Returns the JWT token and user data</response>
            /// <response code="400">If the credentials are invalid or account is deactivated</response>
            /// <response code="401">If the user is not authorized as admin</response>
            /// <response code="500">If there was an internal server error</response>
            [HttpPost("login")]
            [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), StatusCodes.Status200OK)]
            [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), StatusCodes.Status400BadRequest)]
            [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), StatusCodes.Status401Unauthorized)]
            [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), StatusCodes.Status500InternalServerError)]
            public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseFactory.Fail<LoginResponseDto>(
                        "Invalid input. Please check your request."));
                }

                var response = await _authService.LoginAsync(request);

                if (!response.Success)
                {
                    // Determine the appropriate status code based on the error message
                    if (response.Message.Contains("Unauthorized"))
                    {
                        return Unauthorized(response);
                    }
                    return BadRequest(response);
                }

                return Ok(response);
            }

            /// <summary>
            /// Test endpoint to verify if authentication is working
            /// </summary>
            /// <returns>A message indicating the authentication status</returns>
            [HttpGet("test")]
            [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
            public IActionResult TestAuth()
            {
                return Ok(ResponseFactory.Success("Authentication is working!"));
            }
        }
    }
}
