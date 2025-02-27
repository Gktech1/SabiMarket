﻿using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;

namespace SabiMarket.Application.IServices
{
    public interface ICaretakerService
    {

        // Caretaker Management
        Task<BaseResponse<CaretakerResponseDto>> GetCaretakerById(string userId);
        Task<BaseResponse<CaretakerResponseDto>> CreateCaretaker(CaretakerForCreationRequestDto caretakerDto);
        Task<BaseResponse<PaginatorDto<IEnumerable<CaretakerResponseDto>>>> GetCaretakers(PaginationFilter paginationFilter);

        // Trader Management
        Task<BaseResponse<bool>> AssignTraderToCaretaker(string caretakerId, string traderId);
        Task<BaseResponse<bool>> RemoveTraderFromCaretaker(string caretakerId, string traderId);
        Task<BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>> GetAssignedTraders(string caretakerId, PaginationFilter paginationFilter);

        // Levy Management
        Task<BaseResponse<PaginatorDto<IEnumerable<LevyPaymentResponseDto>>>> GetLevyPayments(string caretakerId, PaginationFilter paginationFilter);
        Task<BaseResponse<LevyPaymentResponseDto>> GetLevyPaymentDetails(string levyId);

        // GoodBoy Management
        Task<BaseResponse<GoodBoyResponseDto>> AddGoodBoy(string caretakerId, CreateGoodBoyDto goodBoyDto);
        Task<BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>> GetGoodBoys(string caretakerId, PaginationFilter paginationFilter);
        Task<BaseResponse<bool>> BlockGoodBoy(string caretakerId, string goodBoyId);
        Task<BaseResponse<bool>> UnblockGoodBoy(string caretakerId, string goodBoyId);
    }
}
