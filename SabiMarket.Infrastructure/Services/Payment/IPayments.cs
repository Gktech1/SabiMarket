using SabiMarket.Application.DTOs.PaymentsDto;
using SabiMarket.Application.DTOs.Responses;


public interface IPayments
{
    public Task<BaseResponse<string>> Initialize(FundWalletVM model, string userId);
    public Task<BaseResponse<bool>> Verify(string reference);
    public Task<IEnumerable<Bank>> GetBanks();
}