using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
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
        public SettingsService(IValidator<ChangePasswordDto> changePasswordValidator = null, ILogger<AuthenticationService> logger = null, UserManager<ApplicationUser> userManager = null, IValidator<UpdateProfileDto> updateProfileValidator = null)
        {
            _changePasswordValidator = changePasswordValidator;
            _logger = logger;
            _userManager = userManager;
            _updateProfileValidator = updateProfileValidator;
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
                var validationResult = await _updateProfileValidator.ValidateAsync(updateProfileDto);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Fail<bool>(
                        new ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("User not found"),
                        "User not found");
                }

                var nameParts = updateProfileDto.FullName.Trim().Split(' ', 2);
                var firstName = nameParts[0];
                var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

                user.Email = updateProfileDto.Email;
                user.UserName = updateProfileDto.Email;
                user.FirstName = firstName;
                user.LastName = lastName;  

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException(result.Errors.First().Description),
                        "Profile update failed");
                }

                return ResponseFactory.Success(true, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during profile update");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }
    }
}
