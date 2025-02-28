using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;

namespace SabiMarket.Application.IServices
{
    public interface ISettingsService
    {
        Task<BaseResponse<bool>> ChangePassword(string userId, ChangePasswordDto changePasswordDto);
        Task<BaseResponse<bool>> UpdateProfile(string userId, UpdateProfileDto updateProfileDto);
        Task<BaseResponse<UserDetailsResponseDto>> GetUserDetails(string userId, string userType);
        Task<BaseResponse<bool>> SendPasswordResetOTPBySMS(string phoneNumber);
        Task<BaseResponse<bool>> VerifyPasswordResetOTP(string phoneNumber, string otp);
        Task<BaseResponse<bool>> ResetPasswordAfterOTP(string phoneNumber, string newPassword);
        Task<BaseResponse<bool>> LogoutUser(string userId);
    }
}
