using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;

namespace SabiMarket.Application.IServices
{
    public interface ISettingsService
    {
        Task<BaseResponse<bool>> ChangePassword(string userId, ChangePasswordDto changePasswordDto);
        Task<BaseResponse<bool>> UpdateProfile(string userId, UpdateProfileDto updateProfileDto);
    }
}
