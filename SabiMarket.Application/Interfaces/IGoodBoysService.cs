using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;

namespace SabiMarket.Application.Interfaces
{
    public interface IGoodBoysService
    {
        Task<BaseResponse<GoodBoyResponseDto>> GetGoodBoyById(string goodBoyId);

        Task<BaseResponse<GoodBoyResponseDto>> CreateGoodBoy(CreateGoodBoyRequestDto goodBoyDto);

        Task<BaseResponse<bool>> UpdateGoodBoyProfile(string goodBoyId, UpdateGoodBoyProfileDto profileDto);

        Task<BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>> GetGoodBoys(
            GoodBoyFilterRequestDto filterDto, PaginationFilter paginationFilter);

        Task<BaseResponse<bool>> ProcessLevyPayment(string goodBoyId, ProcessLevyPaymentDto paymentDto);
    }
}

