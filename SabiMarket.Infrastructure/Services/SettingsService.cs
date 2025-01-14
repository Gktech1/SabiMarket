using FluentValidation;
using Microsoft.AspNetCore.Identity;
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
        private readonly IValidator<UpdateProfileDto> _updateProfileValidator;
        private readonly IRepositoryManager _repository;
        public SettingsService(IValidator<ChangePasswordDto> changePasswordValidator, ILogger<AuthenticationService> logger,
            UserManager<ApplicationUser> userManager, IValidator<UpdateProfileDto> updateProfileValidator, IRepositoryManager repository)
        {
            _changePasswordValidator = changePasswordValidator;
            _logger = logger;
            _userManager = userManager;
            _updateProfileValidator = updateProfileValidator;
            _repository = repository;
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
                    Market = marketInfo?.Name ?? string.Empty,
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
                            // Copy base details
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
                            // Add vendor-specific details
                            BusinessName = vendorDetails?.BusinessName ?? string.Empty
                        }, "Vendor details retrieved successfully");

                    case "trader":
                        var traderDetails = await _repository.TraderRepository.GetTraderDetails(userId);
                        return ResponseFactory.Success(new TraderDetailsResponseDto
                        {
                            // Copy base details
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
                            // Add trader-specific details
                            TraderOccupancy = traderDetails?.Occupancy ?? string.Empty,
                            PaymentFrequency = traderDetails?.PaymentFrequency ?? string.Empty,
                            TraderIdentityNumber = traderDetails?.IdentityNumber ?? string.Empty
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

