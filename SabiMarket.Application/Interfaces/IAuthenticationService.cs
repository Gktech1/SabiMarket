using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;

namespace SabiMarket.Application.Interfaces
{
    public interface IAuthenticationService
    {
        Task<BaseResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest);
    }
}
