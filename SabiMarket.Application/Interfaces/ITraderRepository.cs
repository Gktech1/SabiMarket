using SabiMarket.Domain.Entities.MarketParticipants;

namespace SabiMarket.Application.Interfaces
{
    public interface ITraderRepository
    {
        void AddTrader(Trader trader);
        void UpdateTrader(Trader trader);
        Task<IEnumerable<Trader>> GetAllAssistCenterOfficer(bool trackChanges);
        Task<Trader> GetTraderById(string traderId, bool trackChanges);
        Task<Trader> GetTraderDetails(string userId);
        Task<int> GetTraderCountAsync();
    }
}
