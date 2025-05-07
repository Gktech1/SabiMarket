using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Interfaces;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{
    public class TraderRepository : GeneralRepository<Trader>, ITraderRepository
    {
        private readonly ApplicationDbContext _context;

        public TraderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void AddTrader(Trader trader) => Create(trader);

        public void UpdateTrader(Trader trader) => Update(trader);

       /* public async Task<IEnumerable<Trader>> GetAllAssistCenterOfficer(bool trackChanges) =>
            await FindAll(trackChanges).ToListAsync();*/

        public async Task<Trader> GetTraderById(string traderId, bool trackChanges) =>
            await FindByCondition(t => t.Id == traderId, trackChanges)
                .Include(t => t.User)
                .Include(t => t.Market)
                .FirstOrDefaultAsync();

        public async Task<Trader> GetTraderDetails(string userId) =>
            await FindByCondition(t => t.UserId == userId, trackChanges: false)
                .Include(t => t.User)
                .Include(t => t.Market)
                .FirstOrDefaultAsync();

        public async Task<int> GetTraderCountAsync(DateTime startDate, DateTime endDate) =>
            await FindByCondition(t =>
                t.CreatedAt >= startDate &&
                t.CreatedAt <= endDate,
                trackChanges: false)
                .CountAsync();

        public async Task<PaginatorDto<IEnumerable<Trader>>> GetTradersByMarketAsync(
            string marketId,
            PaginationFilter paginationFilter,
            bool trackChanges = false)
        {
            var query = FindByCondition(t => t.MarketId == marketId, trackChanges)
                .Include(t => t.User)
                .Include(t => t.Market)
                .Include(t => t.LevyPayments)
                .OrderByDescending(t => t.CreatedAt);

            return await query.Paginate(paginationFilter);
        }

        public async Task<Trader> GetTraderByMarketAsync(
     string marketId, string userId,
     bool trackChanges = false)
        {
            // Using FirstOrDefaultAsync to return a single Trader or null
            var trader = await FindByCondition(t => t.MarketId == marketId && t.UserId == userId, trackChanges)
                .Include(t => t.User)
                .Include(t => t.Market)
                .Include(t => t.LevyPayments)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            return trader;
        }

        public async Task<IEnumerable<Trader>> GetAllTradersByMarketAsync(
            string marketId,
            bool trackChanges = false)
        {
            return await FindByCondition(t => t.MarketId == marketId, trackChanges)
                .Include(t => t.User)
                .Include(t => t.Market)
                .Include(t => t.LevyPayments)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        public IQueryable<Trader> GetTradersByCaretakerId(string caretakerId, bool trackChanges = false)
        {
            return FindByCondition(t => t.CaretakerId == caretakerId, trackChanges)
                .Include(t => t.User)
                .Include(t => t.Market)
                .Include(t => t.LevyPayments)
                .OrderByDescending(t => t.CreatedAt);
        }

        public void DeleteTrader(Trader trader)
        {
            Delete(trader);
        }
    }
}