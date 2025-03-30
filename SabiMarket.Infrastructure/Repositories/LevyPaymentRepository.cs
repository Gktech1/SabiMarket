using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;
using SabiMarket.Domain.Enum;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;

namespace SabiMarket.Infrastructure.Repositories
{
    public class LevyPaymentRepository : GeneralRepository<LevyPayment>, ILevyPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public LevyPaymentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void AddPayment(LevyPayment levyPayment) => Create(levyPayment);

        public IQueryable<LevyPayment> GetPaymentsQuery()
        {
            return FindAll(trackChanges: false)
                .Include(l => l.Market)
                .Include(l => l.Trader)
                .Include(l => l.GoodBoy);
        }
        public async Task<IEnumerable<LevyPayment>> GetAllLevyPaymentForExport(bool trackChanges) =>
            await FindAll(trackChanges).ToListAsync();

        public async Task<LevyPayment> GetPaymentById(string id, bool trackChanges) =>
            await FindByCondition(x => x.Id == id, trackChanges)
                .Include(x => x.Market)
                .Include(x => x.Trader)
                .FirstOrDefaultAsync();

        // Modified method to get levy configurations per market
        public async Task<IEnumerable<LevyPayment>> GetAllLevySetupsAsync(bool trackChanges)
        {
            return await _context.LevyPayments
                .AsTracking(trackChanges ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking)
                .Include(lp => lp.Market)
                .GroupBy(lp => new { lp.MarketId, lp.Period })
                .Select(g => g.OrderByDescending(lp => lp.CreatedAt).First())
                .OrderBy(lp => lp.MarketId)
                .ThenBy(lp => lp.Period)
                .ToListAsync();
        }

        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetPagedPayment(int? period, PaginationFilter paginationFilter)
        {
            if (period is not null)
            {
                return await FindAll(false)
                            .Where(l => (int)l.Period == period)
                            .Include(l => l.Market)
                            .Include(l => l.Trader)
                            .Include(l => l.GoodBoy)
                                .ThenInclude(gb => gb.User)
                            .Paginate(paginationFilter);
            }

            return await FindAll(false)
                       .Include(l => l.Market)
                       .Include(l => l.Trader)
                       .Include(l => l.GoodBoy)
                           .ThenInclude(gb => gb.User)
                       .Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetLevyPaymentsAsync(
            string chairmanId,
            PaginationFilter paginationFilter,
            bool trackChanges)
        {
            return await FindPagedByCondition(
                expression: lp => lp.ChairmanId == chairmanId,
                paginationFilter: paginationFilter,
                trackChanges: trackChanges,
                orderBy: query => query.OrderByDescending(lp => lp.CreatedAt)
            );
        }

        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> SearchPayment(
            string searchString,
            PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                           .Where(a => a.TransactionReference.Contains(searchString) ||
                           a.Trader.BusinessName.Contains(searchString) ||
                           a.GoodBoy.User.LastName.Contains(searchString) ||
                           a.GoodBoy.User.FirstName.Contains(searchString))
                           .Include(l => l.Market)
                           .Include(l => l.Trader)
                           .Include(l => l.GoodBoy)
                               .ThenInclude(gb => gb.User)
                           .Paginate(paginationFilter);
        }

        public async Task<decimal> GetTotalLeviesAsync(DateTime startDate, DateTime endDate)
        {
            return await FindByCondition(l =>
                l.PaymentDate >= startDate &&
                l.PaymentDate <= endDate,
                trackChanges: false)
                .SumAsync(l => l.Amount);
        }

        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> SearchLevyPaymentsInMarket(
    string marketId,
    string searchQuery,
    PaginationFilter paginationFilter,
    bool trackChanges)
        {
            // Normalize marketId for consistent comparison
            var normalizedMarketId = marketId.ToUpper();

            // Start with base query for the market
            var query = FindByCondition(l => l.MarketId.ToUpper() == normalizedMarketId, trackChanges);

            // Apply search filter if query provided
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                // Normalize search query
                var normalizedSearch = searchQuery.Trim().ToLower();

                // Check if search query is a number
                decimal amount = 0;
                bool isNumeric = decimal.TryParse(normalizedSearch, out amount);

                // Build the query based on search criteria
                query = query.Where(l =>
                    // Search by trader name (first or last)
                    (l.Trader != null && (
                        l.Trader.User.FirstName.ToLower().Contains(normalizedSearch) ||
                        l.Trader.User.LastName.ToLower().Contains(normalizedSearch) ||
                        (l.Trader.User.FirstName + " " + l.Trader.User.LastName).ToLower().Contains(normalizedSearch) ||
                        (l.Trader.BusinessName != null && l.Trader.BusinessName.ToLower().Contains(normalizedSearch))
                    )) ||
                    // Search by GoodBoy name
                    (l.GoodBoy != null && (
                        l.GoodBoy.User.FirstName.ToLower().Contains(normalizedSearch) ||
                        l.GoodBoy.User.LastName.ToLower().Contains(normalizedSearch) ||
                        (l.GoodBoy.User.FirstName + " " + l.GoodBoy.User.LastName).ToLower().Contains(normalizedSearch)
                    )) ||
                    // Search by transaction reference
                    (l.TransactionReference != null && l.TransactionReference.ToLower().Contains(normalizedSearch))
                );

                // If the search is numeric, add amount search as a separate filter
                // to avoid the out parameter in expression trees
                if (isNumeric)
                {
                    query = query.Where(l => l.Amount == amount || query.Any());
                }
            }

            // Include related entities
            query = query
                .Include(l => l.Trader)
                    .ThenInclude(t => t.User)
                .Include(l => l.GoodBoy)
                    .ThenInclude(g => g.User)
                .OrderByDescending(l => l.PaymentDate);

            // Apply pagination using our extension method
            return await query.Paginate(paginationFilter);
        }
        public void DeleteLevyPayment(LevyPayment levy) => Delete(levy);

        // Added method to get market-specific levy configuration
        public async Task<LevyPayment> GetMarketLevySetup(string marketId, PaymentPeriodEnum period)
        {
            return await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId && lp.Period == period)
                .OrderByDescending(lp => lp.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Set<LevyPayment>()
                .Where(lp => lp.PaymentStatus == PaymentStatusEnum.Paid)
                .SumAsync(lp => lp.Amount);
        }

        public async Task<IEnumerable<LevyPayment>> GetByMarketAndOccupancyAsync(string marketId, MarketTypeEnum traderOccupancy)
        {
            var result = await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId || lp.Trader.TraderOccupancy == traderOccupancy)
                .Include(lp => lp.Market)
                .Include(lp => lp.Trader)
                .ToListAsync();

            return result.Any() ? result : null; // Return null if no records are found
        }



        public async Task<IQueryable<LevyPayment>> GetMarketLevySetups(string marketId)
        {
            return _context.LevyPayments
                .Where(lp => lp.MarketId == marketId)
                .GroupBy(lp => lp.Period)
                .Select(g => g.OrderByDescending(lp => lp.CreatedAt).First())
                .OrderBy(lp => lp.Period)
                .AsQueryable();
        }
    }
}