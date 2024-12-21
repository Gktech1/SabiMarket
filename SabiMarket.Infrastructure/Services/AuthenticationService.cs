using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Interfaces;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Infrastructure.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SabiMarket.Infrastructure.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationService> _logger;  // Added logger

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthenticationService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<BaseResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest)
        {
            try
            {
                // Find user by email
                var user = await _userManager.FindByEmailAsync(loginRequest.Email)
                    ?? throw new NotFoundException("Invalid email or password");

                if (!user.IsActive)
                {
                    throw new ForbidException("Account is deactivated");
                }

                // Verify password and sign in
                var result = await _signInManager.PasswordSignInAsync(user, loginRequest.Password, false, true);
                if (!result.Succeeded)
                {
                    throw new BadRequestException("Invalid email or password");
                }

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Any())
                {
                    throw new UnauthorizedException("No roles assigned to this user");
                }

                // Get associated entity based on role
                var userDetails = await GetUserDetailsByRole(user, roles.First());

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Generate JWT token with claims
                var (token, expiresAt) = await GenerateJwtTokenAsync(user, roles.First(), userDetails);

                var response = new LoginResponseDto
                {
                    AccessToken = token,
                    ExpiresAt = expiresAt,
                    UserInfo = new UserClaimsDto
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        Username = user.UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = roles.First(),
                        LastLoginAt = user.LastLoginAt ?? DateTime.UtcNow,
                        ProfileImageUrl = user.ProfileImageUrl,
                        AdditionalDetails = userDetails
                    }
                };

                return ResponseFactory.Success(response, "Login successful");
            }
            catch (ApiException ex)
            {
                _logger.LogWarning(ex, "Login failed for email: {Email}", loginRequest.Email);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during login for email: {Email}", loginRequest.Email);
                throw new BadRequestException("An unexpected error occurred during login");
            }
        }

        private async Task<IDictionary<string, object>> GetUserDetailsByRole(ApplicationUser user, string role)
        {
            var details = new Dictionary<string, object>();

            switch (role.ToUpper())
            {
                case "ADMIN":
                    // Add any admin-specific details
                    break;

                case "CHAIRMAN":
                    if (user.Chairman != null)
                    {
                        details.Add("title", user.Chairman.Title);
                        details.Add("office", user.Chairman.Office);
                        details.Add("termStart", user.Chairman.TermStart);
                        details.Add("termEnd", user.Chairman.TermEnd);
                        details.Add("localGovernmentId", user.Chairman.LocalGovernmentId);
                    }
                    break;

                case "TRADER":
                    if (user.Trader != null)
                    {
                        details.Add("traderId", user.Trader.Id);
                        details.Add("businessName", user.Trader.BusinessName);
                        // Add other trader details
                    }
                    break;

                case "VENDOR":
                    if (user.Vendor != null)
                    {
                        details.Add("vendorId", user.Vendor.Id);
                        details.Add("businessName", user.Vendor.BusinessName);
                        details.Add("businessType", user.Vendor.BusinessDescription);
                        details.Add("businessType", user.Vendor.BusinessAddress);
                        // Add other vendor-specific details
                    }
                    break;

                case "CUSTOMER":
                    if (user.Customer != null)
                    {
                        details.Add("customerId", user.Customer.Id);
                        details.Add("customerId", user.Customer.LocalGovernmentId);
                        details.Add("customerType", user.Customer.LocalGovernment);
                        details.Add("preferredMarket", user.Customer.FullName);
                        // Add other customer-specific details
                    }
                    break;

                case "GOODBOY":
                    if (user.GoodBoy != null)
                    {
                        details.Add("goodBoyId", user.GoodBoy.Id);
                        // Add other GoodBoy details
                    }
                    break;

                case "CARETAKER":
                    if (user.Caretaker != null)
                    {
                        details.Add("caretakerId", user.Caretaker.Id);
                        // Add other caretaker details
                    }
                    break;

                case "ASSISTCENTEROFFICER":
                    if (user.AssistCenterOfficer != null)
                    {
                        details.Add("officerId", user.AssistCenterOfficer.Id);
                        // Add other officer details
                    }
                    break;

                default:
                    _logger.LogWarning("Unknown role encountered: {Role}", role);
                    break;
            }

            if (user.LocalGovernmentId.HasValue)
            {
                details.Add("localGovernmentId", user.LocalGovernmentId.Value);
            }

            return details;
        }
        private async Task<(string token, DateTime expiresAt)> GenerateJwtTokenAsync(
            ApplicationUser user,
            string role,
            IDictionary<string, object> additionalDetails)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, role),
                new Claim("profile_image", user.ProfileImageUrl ?? string.Empty),
                new Claim("last_login", user.LastLoginAt?.ToString("O") ?? DateTime.UtcNow.ToString("O"))
            };

            // Add additional role-specific claims
            foreach (var detail in additionalDetails)
            {
                claims.Add(new Claim(detail.Key, detail.Value?.ToString() ?? string.Empty));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddDays(1);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        /*    public async Task<BaseResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest)
            {
                try
                {
                    // Find user by email
                    var user = await _userManager.FindByEmailAsync(loginRequest.Email);
                    if (user == null)
                    {
                        return ResponseFactory.Fail<LoginResponseDto>("Invalid email or password");
                    }

                    if (!user.IsActive)
                    {
                        return ResponseFactory.Fail<LoginResponseDto>("Account is deactivated");
                    }

                    // Check if user is in Admin role
                    var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                    if (!isAdmin)
                    {
                        return ResponseFactory.Fail<LoginResponseDto>("Unauthorized access");
                    }

                    // Verify password and sign in
                    var result = await _signInManager.PasswordSignInAsync(user, loginRequest.Password, false, true);
                    if (!result.Succeeded)
                    {
                        return ResponseFactory.Fail<LoginResponseDto>("Invalid email or password");
                    }

                    // Update last login
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    // Generate JWT token with claims
                    var (token, expiresAt) = await GenerateJwtTokenAsync(user);

                    var response = new LoginResponseDto
                    {
                        AccessToken = token,
                        ExpiresAt = expiresAt,
                        UserInfo = new UserClaimsDto
                        {
                            UserId = user.Id,
                            Email = user.Email,
                            Username = user.UserName,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Role = "Admin",
                            LastLoginAt = user.LastLoginAt ?? DateTime.UtcNow,
                            ProfileImageUrl = user.ProfileImageUrl
                        }
                    };

                    return ResponseFactory.Success(response, "Login successful");
                }
                catch (ApiException ex)
                {
                    _logger.LogWarning(ex, "Login failed for email: {Email}", loginRequest.Email);
                    throw; // Let the middleware handle the exception
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred during login for email: {Email}", loginRequest.Email);
                    throw new BadRequestException("An unexpected error occurred during login");
                }
            }
    */
        /*  private async Task<(string token, DateTime expiresAt)> GenerateJwtTokenAsync(ApplicationUser user)
          {
              var claims = new List<Claim>
              {
                  new Claim(ClaimTypes.NameIdentifier, user.Id),
                  new Claim(ClaimTypes.Email, user.Email),
                  new Claim(ClaimTypes.Name, user.UserName),
                  new Claim(ClaimTypes.GivenName, user.FirstName),
                  new Claim(ClaimTypes.Surname, user.LastName),
                  new Claim(ClaimTypes.Role, "Admin"),
                  new Claim("profile_image", user.ProfileImageUrl ?? string.Empty),
                  new Claim("last_login", user.LastLoginAt?.ToString("O") ?? DateTime.UtcNow.ToString("O"))
              };

              var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
              var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
              var expiresAt = DateTime.UtcNow.AddDays(1);

              var token = new JwtSecurityToken(
                  issuer: _configuration["JWT:ValidIssuer"],
                  audience: _configuration["JWT:ValidAudience"],
                  claims: claims,
                  expires: expiresAt,
                  signingCredentials: creds
              );

              return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
          }
     */
    }
}

