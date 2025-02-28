using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QRCoder;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.IRepositories;
using SabiMarket.Application.IServices;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Exceptions;
using ValidationException = FluentValidation.ValidationException;

namespace SabiMarket.Infrastructure.Services
{
    public class SettingsService : ISettingsService
    {

        private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IValidator<UpdateProfileDto> _updateProfileValidator;
        private readonly IRepositoryManager _repository;
        private readonly ISmsService _smsService;
        public SettingsService(IValidator<ChangePasswordDto> changePasswordValidator, ILogger<AuthenticationService> logger,
            UserManager<ApplicationUser> userManager, IValidator<UpdateProfileDto> updateProfileValidator, IRepositoryManager repository, SignInManager<ApplicationUser> signInManager, ISmsService smsService)
        {
            _changePasswordValidator = changePasswordValidator;
            _logger = logger;
            _userManager = userManager;
            _updateProfileValidator = updateProfileValidator;
            _repository = repository;
            _signInManager = signInManager;
            _smsService = smsService;
        }

        public async Task<BaseResponse<bool>> ChangePassword(string userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var validationResult = await _changePasswordValidator.ValidateAsync(changePasswordDto);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Fail<bool>(
                        new FluentValidation.ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("User not found"),
                        "User not found");
                }

                var result = await _userManager.ChangePasswordAsync(user,
                    changePasswordDto.CurrentPassword,
                    changePasswordDto.NewPassword);

                if (!result.Succeeded)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException(result.Errors.First().Description),
                        "Password change failed");
                }

                return ResponseFactory.Success(true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> SendPasswordResetOTPBySMS(string phoneNumber)
        {
            try
            {
                // Find user by phone number
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
                if (user == null)
                {
                    // For security reasons, don't reveal that the user doesn't exist
                    return ResponseFactory.Success(true, "If your phone number exists in our system, an OTP has been sent");
                }

                // Check environment (use ENV variable to determine Dev or Production)
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                bool isDevelopment = environment == "Development";

                // Generate OTP: Use static OTP in Dev, Random OTP in Live
                var otp = isDevelopment ? "777777" : new Random().Next(100000, 999999).ToString();

                // Store OTP in user tokens with expiration
                await _userManager.SetAuthenticationTokenAsync(user, "PasswordReset", "OTP", otp);
                await _userManager.SetAuthenticationTokenAsync(user, "PasswordReset", "OTPExpiry",
                    DateTime.UtcNow.AddMinutes(5).ToString("o")); // 5-minute expiry

                // Send SMS with OTP
                bool smsSent = await _smsService.SendSMS(
                    phoneNumber,
                    $"Your password reset OTP is: {otp}. Valid for 5 minutes."
                );

                if (!smsSent)
                {
                    _logger.LogWarning($"Failed to send SMS to {phoneNumber}");
                   // return ResponseFactory.Success(true, $"OTP is {otp} and was sent successfully to your phone");
                }

                return ResponseFactory.Success(true, "OTP was sent successfully to your phone");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset OTP via SMS");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }


        public async Task<BaseResponse<bool>> VerifyPasswordResetOTP(string phoneNumber, string otp)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
                if (user == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Invalid phone number or OTP"),
                        "Verification failed");
                }

                // Retrieve stored OTP and expiry
                var storedOTP = await _userManager.GetAuthenticationTokenAsync(user, "PasswordReset", "OTP");
                var expiryString = await _userManager.GetAuthenticationTokenAsync(user, "PasswordReset", "OTPExpiry");

                if (string.IsNullOrEmpty(storedOTP) || string.IsNullOrEmpty(expiryString))
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("No active OTP found"),
                        "Verification failed");
                }

                // Check expiration
                if (DateTime.TryParse(expiryString, out var expiry) && expiry < DateTime.UtcNow)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("OTP has expired"),
                        "Verification failed");
                }

                // Verify OTP
                if (storedOTP != otp)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("Invalid OTP"),
                        "Verification failed");
                }

                // Generate a reset token
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Store the reset token temporarily
                await _userManager.SetAuthenticationTokenAsync(user, "PasswordReset", "ResetToken", resetToken);

                return ResponseFactory.Success(true, "OTP verified successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password reset OTP");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> ResetPasswordAfterOTP(string phoneNumber, string newPassword)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
                if (user == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("User not found"),
                        "Password reset failed");
                }

                // Retrieve stored reset token
                var resetToken = await _userManager.GetAuthenticationTokenAsync(user, "PasswordReset", "ResetToken");
                if (string.IsNullOrEmpty(resetToken))
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("Invalid or expired verification session"),
                        "Password reset failed");
                }

                // Reset password
                var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
                if (!result.Succeeded)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException(result.Errors.First().Description),
                        "Password reset failed");
                }

                // Clean up tokens
                await _userManager.RemoveAuthenticationTokenAsync(user, "PasswordReset", "OTP");
                await _userManager.RemoveAuthenticationTokenAsync(user, "PasswordReset", "OTPExpiry");
                await _userManager.RemoveAuthenticationTokenAsync(user, "PasswordReset", "ResetToken");

                return ResponseFactory.Success(true, "Password reset successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

      /*  public async Task<BaseResponse<bool>> ChangePassword(string userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var validationResult = await _changePasswordValidator.ValidateAsync(changePasswordDto);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Fail<bool>(
                        new FluentValidation.ValidationException(validationResult.Errors),
                        "Validation failed");
                }
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("User not found"),
                        "User not found");
                }
                var result = await _userManager.ChangePasswordAsync(user,
                    changePasswordDto.CurrentPassword,
                    changePasswordDto.NewPassword);
                if (!result.Succeeded)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException(result.Errors.First().Description),
                        "Password change failed");
                }
                return ResponseFactory.Success(true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }*/

        public async Task<BaseResponse<bool>> LogoutUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ResponseFactory.Fail<bool>(new NotFoundException("User not found"), "User not found");
                }

                // Force logout by invalidating security stamp
                await _userManager.UpdateSecurityStampAsync(user);
                await _signInManager.SignOutAsync();

                return ResponseFactory.Success(true, "User logged out successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return ResponseFactory.Fail<bool>(ex, "An error occurred during logout");
            }
        }


        public async Task<BaseResponse<bool>> UpdateProfile(string userId, UpdateProfileDto updateProfileDto)
        {
            try
            {
                // Validate input
                var validationResult = await _updateProfileValidator.ValidateAsync(updateProfileDto);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Fail<bool>(
                        new ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                // Get user and verify existence
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("User not found"),
                        "User not found");
                }

                // Verify LocalGovernment exists
                var localGovernmentExists = await _repository.LocalGovernmentRepository.LocalGovernmentExist
                    (updateProfileDto.LocalGovernmentId);
                if (!localGovernmentExists)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("Invalid Local Government selected"),
                        "Invalid Local Government");
                }

                // Check if email is being changed and verify it's not taken by another user
                if (!string.Equals(user.Email, updateProfileDto.EmailAddress, StringComparison.OrdinalIgnoreCase))
                {
                    var emailExists = await _userManager.FindByEmailAsync(updateProfileDto.EmailAddress);
                    if (emailExists != null && emailExists.Id != userId)
                    {
                        return ResponseFactory.Fail<bool>(
                            new BadRequestException("Email address is already in use"),
                            "Email address is already taken");
                    }
                }

                // Update user properties
                var nameParts = updateProfileDto.FullName?.Trim().Split(' ', 2);
                user.FirstName = nameParts?[0] ?? user.FirstName;
                user.LastName = nameParts?.Length > 1 ? nameParts[1] : string.Empty;
                user.Email = updateProfileDto.EmailAddress;
                user.UserName = updateProfileDto.EmailAddress;
                user.PhoneNumber = updateProfileDto.PhoneNumber;
                user.Address = updateProfileDto.Address;
                user.LocalGovernmentId = updateProfileDto.LocalGovernmentId;

                // Update profile image if provided
                if (!string.IsNullOrEmpty(updateProfileDto.ProfileImageUrl))
                {
                    user.ProfileImageUrl = updateProfileDto.ProfileImageUrl;
                }

                // Update user
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException(errors),
                        "Profile update failed");
                }

                // Log the successful update
                _logger.LogInformation("Profile updated successfully for user {UserId}", userId);

                return ResponseFactory.Success(true, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {Use[prId}", userId);
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<UserDetailsResponseDto>> GetUserDetails(string userId, string userType)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ResponseFactory.Fail<UserDetailsResponseDto>(
                        new NotFoundException("User not found"),
                        "User profile not found");
                }

                // Get user's market and other related information
                var marketInfo = await _repository.MarketRepository.GetMarketByUserId(userId, false);

                var traderDetailsResponse = new TraderDetailsResponseDto(); 
                // Base user details
                var baseDetails = new UserDetailsResponseDto
                {
                    Id = user.Id,
                    UserId = userId,
                    FullName = string.Join(" ",
                        new[] { user.FirstName, user.LastName }
                        .Where(x => !string.IsNullOrEmpty(x))),
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    EmailAddress = user.Email ?? string.Empty,
                    Gender = user.Gender,
                    Market = marketInfo?.MarketName ?? string.Empty,
                    LGA = marketInfo?.LocalGovernment?.LGA ?? string.Empty,
                    Address = user.Address ?? string.Empty,
                    DateAdded = user.CreatedAt,
                    IsBlocked = user.IsBlocked,
                    QrCodeData = GenerateQrCodeData(userId)
                };

                // Return specific details based on user type
                switch (userType.ToLower())
                {
                    case "vendor":
                        var vendorDetails = await _repository.VendorRepository.GetVendorDetails(userId);
                        return ResponseFactory.Success(new VendorDetailsResponseDto
                        {
                            // Base details
                            Id = baseDetails.Id,
                            UserId = baseDetails.UserId,
                            FullName = baseDetails.FullName,
                            PhoneNumber = baseDetails.PhoneNumber,
                            EmailAddress = baseDetails.EmailAddress,
                            Gender = baseDetails.Gender,
                            Market = baseDetails.Market,
                            LGA = baseDetails.LGA,
                            Address = baseDetails.Address,
                            DateAdded = baseDetails.DateAdded,
                            IsBlocked = baseDetails.IsBlocked,
                            QrCodeData = baseDetails.QrCodeData,
                            // Vendor-specific details
                            BusinessName = vendorDetails?.BusinessName ?? string.Empty
                        } as UserDetailsResponseDto, "Vendor details retrieved successfully");


                    case "trader":
                        var traderDetails = await _repository.TraderRepository.GetTraderDetails(userId);
                        return ResponseFactory.Success(new UserDetailsResponseDto
                        {
                            Id = baseDetails.Id,
                            UserId = baseDetails.UserId,
                            FullName = baseDetails.FullName,
                            PhoneNumber = baseDetails.PhoneNumber,
                            EmailAddress = baseDetails.EmailAddress,
                            Gender = baseDetails.Gender,
                            Market = baseDetails.Market,
                            LGA = baseDetails.LGA,
                            Address = baseDetails.Address,
                            DateAdded = baseDetails.DateAdded,
                            IsBlocked = baseDetails.IsBlocked,
                            QrCodeData = baseDetails.QrCodeData,
                            TraderDetails = traderDetails == null ? null : new TraderDetailsResponseDto
                            {
                                TraderIdentityNumber = traderDetails.TIN,
                                TraderOccupancy = string.Empty,
                                PaymentFrequency = string.Empty
                            }
                        }, "Trader details retrieved successfully");


                    default:
                        return ResponseFactory.Success(baseDetails, "User details retrieved successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user details for user {UserId}", userId);
                return ResponseFactory.Fail<UserDetailsResponseDto>(ex, "An unexpected error occurred");
            }
        }

        private string GenerateQrCodeData(string userId)
        {
            try
            {
                // Generate unique QR code data with user specific information
                string qrData = $"SABI-MARKET-USER-{userId}-{DateTime.UtcNow:yyyyMMddHHmmss}";

                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                using var pngByteQRCode = new PngByteQRCode(qrCodeData);
                var qrCodeImage = pngByteQRCode.GetGraphic(20); // 20 pixels per module

                // Convert to base64 for web display
                var qrCodeBase64 = Convert.ToBase64String(qrCodeImage);
                return $"data:image/png;base64,{qrCodeBase64}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for user {UserId}", userId);
                throw;
            }
        }

        // Helper method for generating byte array
        private byte[] GenerateQrCodeImage(string data)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using var pngByteQRCode = new PngByteQRCode(qrCodeData);
            return pngByteQRCode.GetGraphic(20);
        }
    }
}

