using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.MarketParticipants;

public interface ITraderRepository : IGeneralRepository<Trader>
{
    void AddTrader(Trader trader);
    void UpdateTrader(Trader trader);
    Task<Trader> GetTraderById(string traderId, bool trackChanges);
    Task<Trader> GetTraderDetails(string userId);
    Task<int> GetTraderCountAsync(DateTime startDate, DateTime endDate);
    Task<PaginatorDto<IEnumerable<Trader>>> GetTradersByMarketAsync(string marketId, PaginationFilter paginationFilter, bool trackChanges = false);
    Task<IEnumerable<Trader>> GetAllTradersByMarketAsync(string marketId, bool trackChanges = false);
}