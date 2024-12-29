using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Interfaces;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Exceptions;
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
        private readonly IValidator<RegistrationRequestDto> _registrationValidator;
        private readonly IValidator<LoginRequestDto> _loginValidator;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthenticationService> logger,
            IValidator<RegistrationRequestDto> registrationValidator,
            RoleManager<ApplicationRole> roleManager,
            IValidator<LoginRequestDto> loginValidator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _registrationValidator = registrationValidator;
            _roleManager = roleManager;
            _loginValidator = loginValidator;
        }

        public async Task<BaseResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest)
        {
            try
            {
                // Validate request using FluentValidation
                var validationResult = await _loginValidator.ValidateAsync(loginRequest);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Fail<LoginResponseDto>(
                        new FluentValidation.ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                // Find user by email
                var user = await _userManager.FindByEmailAsync(loginRequest.Email);
                if (user == null)
                {
                    return ResponseFactory.Fail<LoginResponseDto>(
                        new NotFoundException("Invalid email or password"),
                        "User not found");
                }

                if (!user.IsActive)
                {
                    return ResponseFactory.Fail<LoginResponseDto>(
                        new ForbidException("Account is deactivated"),
                        "Account inactive");
                }

                // Verify password and sign in
                var result = await _signInManager.PasswordSignInAsync(user, loginRequest.Password, false, true);
                if (!result.Succeeded)
                {
                    return ResponseFactory.Fail<LoginResponseDto>(
                        new BadRequestException("Invalid email or password"),
                        "Login failed");
                }

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Any())
                {
                    return ResponseFactory.Fail<LoginResponseDto>(
                        new UnauthorizedException("No roles assigned to this user"),
                        "No roles assigned");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during login for email: {Email}", loginRequest.Email);
                return ResponseFactory.Fail<LoginResponseDto>(
                    new BadRequestException("An unexpected error occurred during login"),
                    "Login failed");
            }
        }

        public async Task<BaseResponse<RegistrationResponseDto>> RegisterAsync(RegistrationRequestDto request)
        {
            try
            {
                // Validate request using FluentValidation
                var validationResult = await _registrationValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Fail<RegistrationResponseDto>(
                        new FluentValidation.ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return ResponseFactory.Fail<RegistrationResponseDto>(
                        new BadRequestException("Email already registered"),
                        "Email exists");
                }

                // Validate role
                if (!await _roleManager.RoleExistsAsync(request.Role.ToUpper()))
                {
                    return ResponseFactory.Fail<RegistrationResponseDto>(
                        new BadRequestException("Invalid role specified"),
                        "Invalid role");
                }

                // Create base user
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Create user
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return ResponseFactory.Fail<RegistrationResponseDto>(
                        new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description))),
                        "User creation failed");
                }

                // Assign role
                var roleResult = await _userManager.AddToRoleAsync(user, request.Role.ToUpper());
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    return ResponseFactory.Fail<RegistrationResponseDto>(
                        new BadRequestException("Failed to assign role"),
                        "Role assignment failed");
                }

                var response = new RegistrationResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Role = request.Role,
                    Message = "Registration successful. You can now log in",
                    RequiresApproval = RequiresApproval(request.Role)
                };

                return ResponseFactory.Success(response, "Registration successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for email: {Email}", request.Email);
                return ResponseFactory.Fail<RegistrationResponseDto>(
                    new BadRequestException("An unexpected error occurred during registration"),
                    "Registration failed");
            }
        }


        private bool RequiresApproval(string role)
        {
            return role.ToUpper() switch
            {
                DefaultRoles.Vendor => true,
                DefaultRoles.Trader => true,
                DefaultRoles.Chairman => true,
                DefaultRoles.Caretaker => true,
                DefaultRoles.Goodboy => true,
                DefaultRoles.Customer => true,
                _ => false
            };
        }

        /* private string GetRegistrationMessage(string role)
         {
            // var requiresVerification = RequiresEmailVerification(role);
             var requiresApproval = RequiresApproval(role);

             return (requiresApproval) switch
             {
                 (true) => "Registration successful. Please verify your email and wait for admin approval.",
                 (true, false) => "Registration successful. Please verify your email to activate your account.",
                 (false, true) => "Registration successful. Your account is pending admin approval.",
                 _ => "Registration successful. You can now log in."
             };
         }*/



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
            try
            {
                // Validate JWT configuration
                var jwtSecret = _configuration["JWT:Secret"];
                var validIssuer = _configuration["JWT:ValidIssuer"];
                var validAudience = _configuration["JWT:ValidAudience"];

                if (string.IsNullOrEmpty(jwtSecret))
                {
                    _logger.LogError("JWT Secret is not configured");
                    throw new InvalidOperationException("JWT configuration is missing");
                }

                if (string.IsNullOrEmpty(validIssuer) || string.IsNullOrEmpty(validAudience))
                {
                    _logger.LogError("JWT Issuer or Audience is not configured");
                    throw new InvalidOperationException("JWT configuration is incomplete");
                }

                // Validate user data
                if (user == null || string.IsNullOrEmpty(user.Id) || string.IsNullOrEmpty(user.Email))
                {
                    throw new ArgumentException("Invalid user data");
                }

                // Create claims
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
            new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new Claim(ClaimTypes.Role, role ?? string.Empty),
            new Claim("profile_image", user.ProfileImageUrl ?? string.Empty),
            new Claim("last_login", user.LastLoginAt?.ToString("O") ?? DateTime.UtcNow.ToString("O"))
        };

                // Add additional role-specific claims with null checking
                if (additionalDetails != null)
                {
                    foreach (var detail in additionalDetails)
                    {
                        if (!string.IsNullOrEmpty(detail.Key))
                        {
                            claims.Add(new Claim(detail.Key, detail.Value?.ToString() ?? string.Empty));
                        }
                    }
                }

                // Create token
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiresAt = DateTime.UtcNow.AddDays(1);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = expiresAt,
                    SigningCredentials = creds,
                    Issuer = validIssuer,
                    Audience = validAudience
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                return (tokenHandler.WriteToken(token), expiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user: {UserId}", user?.Id);
                throw;
            }
        }

        /* private async Task<(string token, DateTime expiresAt)> GenerateJwtTokenAsync(
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
 */
    }
}

