using SabiMarket.Domain.Entities.MarketParticipants;

namespace SabiMarket.Application.Interfaces
{
    public interface ITraderRepository
    {
        void AddTrader(Trader trader);
        void UpdateTrader(Trader trader);
        Task<IEnumerable<Trader>> GetAllAssistCenterOfficer(bool trackChanges);
    }
}
