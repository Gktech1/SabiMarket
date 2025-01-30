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

        public async Task<int> GetCompliantTraderCount(IEnumerable<string> traderIds)
        {
            var twoDaysAgo = DateTime.UtcNow.AddDays(-2);

            return await _context.LevyPayments
                .Where(p => traderIds.Contains(p.TraderId) &&
                           p.PaymentStatus == PaymentStatusEnum.Paid &&
                           p.PaymentDate >= twoDaysAgo)
                .Select(p => p.TraderId)
                .Distinct()
                .CountAsync();
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

        public void DeleteLevyPayment(LevyPayment levy) => Delete(levy);

        // Added method to get market-specific levy configuration
        public async Task<LevyPayment> GetMarketLevySetup(string marketId, PaymentPeriodEnum period)
        {
            return await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId && lp.Period == period)
                .OrderByDescending(lp => lp.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetTotalAmount()
        {
            return await _context.LevyPayments
                .Where(l => l.PaymentStatus == PaymentStatusEnum.Paid)
                .SumAsync(l => l.Amount);
        }

        public async Task<decimal> GetMonthlyCollection()
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-30);
            return await _context.LevyPayments
                .Where(l =>
                l.PaymentDate >= startDate &&
                l.PaymentStatus == PaymentStatusEnum.Paid)
                .SumAsync(l => l.Amount);
        }

        public async Task<decimal> GetDailyCollection()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.LevyPayments
                .Where(l => l.PaymentDate.Date == today && l.PaymentStatus == PaymentStatusEnum.Paid)
                .SumAsync(l => l.Amount);
        }

        public async Task<int> CountPendingPayments()
        {
            return await _context.LevyPayments
                .Where(p => p.PaymentStatus == PaymentStatusEnum.Pending)
                .CountAsync();
        }

        public async Task<List<TodayLevyDto>> GetTodayLevies(string goodBoyId)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.LevyPayments
                .Where(p => p.PaymentDate.Date == today && p.GoodBoyId == goodBoyId)
                .Include(p => p.Trader)
                    .ThenInclude(t => t.User)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new TodayLevyDto
                {
                    TraderName = $"{p.Trader.User.FirstName} {p.Trader.User.LastName}",
                    Amount = p.Amount,
                    Time = p.PaymentDate.ToString("h:mm tt")
                })
                .ToListAsync();
        }

        public async Task<decimal> GetTotalLevies(string goodBoyId) =>
            await _context.LevyPayments
                .Where(p => p.GoodBoyId == goodBoyId)
                .SumAsync(p => p.Amount);

        public async Task<IQueryable<LevyPayment>> GetMarketLevySetups(string marketId)
        {
            return _context.LevyPayments
                .Where(lp => lp.MarketId == marketId)
                .GroupBy(lp => lp.Period)
                .Select(g => g.OrderByDescending(lp => lp.CreatedAt).First())
                .OrderBy(lp => lp.Period)
                .AsQueryable();
        }

        public async Task<decimal> CalculateOutstandingDebt(string traderId)
        {
            // Get the trader with their payment frequency and amount
            var trader = await _context.Traders
                .Include(t => t.LevyPayments)
                .FirstOrDefaultAsync(t => t.Id == traderId);

            if (trader == null)
                return 0;

            var today = DateTime.UtcNow.Date;
            var lastPayment = trader.LevyPayments
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefault();

            if (lastPayment == null)
            {
                // If no payment has ever been made, calculate debt from trader creation date
                var daysSinceCreation = (today - trader.CreatedAt.Date).Days;
                var expectedPayments = daysSinceCreation / (int)trader.PaymentFrequency;
                return expectedPayments * trader.PaymentAmount;
            }

            // Calculate days since last payment
            var daysSinceLastPayment = (today - lastPayment.PaymentDate.Date).Days;

            // Calculate number of missed payments
            var missedPayments = daysSinceLastPayment / (int)trader.PaymentFrequency;

            // Calculate total outstanding debt
            var outstandingDebt = missedPayments * trader.PaymentAmount;

            return outstandingDebt;

        }
    }
}