using global::SabiMarket.Application.DTOs.Requests;
using global::SabiMarket.Application.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.Application.Interfaces;

namespace SabiMarket.API.Controllers.Authentication
{
    namespace SabiMarket.API.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        [Produces("application/json")]
        //[Authorize]
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
            /// Authenticates a user and returns a JWT token
            /// </summary>
            /// <param name="request">Login credentials</param>
            /// <returns>Authentication token and user information</returns>
            /// <response code="200">Returns the JWT token and user data</response>
            /// <response code="400">If the credentials are invalid</response>
            /// <response code="401">If the user is unauthorized</response>
            /// <response code="403">If the account is deactivated</response>
            /// <response code="500">If there was an internal server error</response>
            [HttpPost("login")]
            [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), StatusCodes.Status200OK)]
            [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), StatusCodes.Status400BadRequest)]
            [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), StatusCodes.Status401Unauthorized)]
            [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), StatusCodes.Status403Forbidden)]
            [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), StatusCodes.Status500InternalServerError)]
            public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
            {
                var response = await _authService.LoginAsync(request);

                if (!response.Status)
                {
                    return response.Error?.StatusCode switch
                    {
                        StatusCodes.Status400BadRequest => BadRequest(response),
                        StatusCodes.Status401Unauthorized => Unauthorized(response),
                        StatusCodes.Status403Forbidden => StatusCode(StatusCodes.Status403Forbidden, response),
                        StatusCodes.Status404NotFound => NotFound(response),
                        _ => BadRequest(response)
                    };
                }

                return Ok(response);
            }

            /// <summary>
            /// Registers a new user in the system
            /// </summary>
            /// <param name="request">User registration details</param>
            /// <returns>Registration confirmation with user information</returns>
            /// <response code="200">Returns the registration confirmation</response>
            /// <response code="400">If the registration data is invalid</response>
            /// <response code="409">If the email is already registered</response>
            /// <response code="500">If there was an internal server error</response>
            [HttpPost("register")]
            [AllowAnonymous]
            [ProducesResponseType(typeof(BaseResponse<RegistrationResponseDto>), StatusCodes.Status200OK)]
            [ProducesResponseType(typeof(BaseResponse<RegistrationResponseDto>), StatusCodes.Status400BadRequest)]
            [ProducesResponseType(typeof(BaseResponse<RegistrationResponseDto>), StatusCodes.Status409Conflict)]
            [ProducesResponseType(typeof(BaseResponse<RegistrationResponseDto>), StatusCodes.Status500InternalServerError)]
            public async Task<IActionResult> Register([FromBody] RegistrationRequestDto request)
            {
                var response = await _authService.RegisterAsync(request);

                if (!response.Status)
                {
                    // Handle different types of registration failures
                    return response.Error?.StatusCode switch
                    {
                        StatusCodes.Status400BadRequest => BadRequest(response),
                        StatusCodes.Status409Conflict => Conflict(response),
                        _ => BadRequest(response)
                    };
                }

                return Ok(response);

            }

            /*  /// <summary>
              /// Verifies a user's email address
              /// </summary>
              /// <param name="request">Email verification details</param>
              /// <returns>Email verification result</returns>
              /// <response code="200">Email successfully verified</response>
              /// <response code="400">If the verification token is invalid</response>
              /// <response code="404">If the user is not found</response>
              [HttpPost("verify-email")]
              [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
              [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
              [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status404NotFound)]
              public async Task<IActionResult> VerifyEmail([FromBody] EmailVerificationRequestDto request)
              {
                  try
                  {
                      var response = await _authService.VerifyEmailAsync(request);

                      if (!response.Status)
                      {
                          return response.Error?.StatusCode switch
                          {
                              StatusCodes.Status404NotFound => NotFound(response),
                              _ => BadRequest(response)
                          };
                      }

                      return Ok(response);
                  }
                  catch (Exception ex)
                  {
                      _logger.LogError(ex, "Email verification failed for: {Email}", request.Email);
                      return StatusCode(
                          StatusCodes.Status500InternalServerError,
                          ResponseFactory.Fail<string>(
                              new Exception("An unexpected error occurred while verifying your email"),
                              "Email verification failed"));
                  }
              }

              /// <summary>
              /// Resends the verification email to a user
              /// </summary>
              /// <param name="request">Email details</param>
              /// <returns>Confirmation of email resend</returns>
              [HttpPost("resend-verification")]
              [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
              [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
              [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status404NotFound)]
              public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailRequestDto request)
              {
                  try
                  {
                      var response = await _authService.ResendVerificationEmailAsync(request);

                      if (!response.Status)
                      {
                          return response.Error?.StatusCode switch
                          {
                              StatusCodes.Status404NotFound => NotFound(response),
                              _ => BadRequest(response)
                          };
                      }

                      return Ok(response);
                  }
                  catch (Exception ex)
                  {
                      _logger.LogError(ex, "Failed to resend verification email to: {Email}", request.Email);
                      return StatusCode(
                          StatusCodes.Status500InternalServerError,
                          ResponseFactory.Fail<string>(
                              new Exception("An unexpected error occurred while resending verification email"),
                              "Failed to resend verification email"));
                  }
              }
          }*/


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
