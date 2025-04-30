using SabiMarket.Application.DTOs.PaymentsDto;

namespace SabiMarket.Infrastructure.Services;

public interface IPaymentService
{
    public Task<Tuple<bool, string, string>> InitializePayment(FundWalletVM model, string userId);
    public Task<bool> Verify(string reference);
    public Task<IEnumerable<Bank>> GetListOfBanks();
}