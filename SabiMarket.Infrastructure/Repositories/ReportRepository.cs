using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities;
using SabiMarket.Infrastructure.Data;

namespace SabiMarket.Infrastructure.Repositories
{
    public class ReportRepository : GeneralRepository<Report>, IReportRepository
    {
        private readonly ApplicationDbContext _context;
        public ReportRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Report> GetDashboardSummary()
        {
            var markets = await _context.Markets.CountAsync();
            var totalRevenue = await _context.LevyPayments.SumAsync(x => x.Amount);
            return new Report
            {
                MarketCount = markets,
                TotalRevenueGenerated = totalRevenue,
                ReportDate = DateTime.UtcNow
            };
        }

        public async Task<Report> GetDailyMetricsAsync(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            var dailyRevenue = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startOfDay && lp.PaymentDate <= endOfDay)
                .SumAsync(lp => lp.Amount);

            var dailyTransactions = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startOfDay && lp.PaymentDate <= endOfDay)
                .CountAsync();

            var activeMarkets = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startOfDay && lp.PaymentDate <= endOfDay)
                .Select(lp => lp.MarketId)
                .Distinct()
                .CountAsync();

            var newTraders = await _context.Traders
                .Where(t => t.CreatedAt >= startOfDay && t.CreatedAt <= endOfDay)
                .CountAsync();

            return new Report
            {
                ReportDate = date,
                TotalRevenueGenerated = dailyRevenue,
                PaymentTransactions = dailyTransactions,
                ActiveMarkets = activeMarkets,
                NewTradersCount = newTraders,
                IsDaily = true
            };
        }

        public async Task<Report> GetMetricsAsync(DateTime startDate, DateTime endDate)
        {
            // Get total traders and caretakers
            var totalTraders = await _context.Traders.CountAsync();
            var totalCaretakers = await _context.Caretakers.CountAsync();

            // Get active markets - modified to check for non-null CaretakerId or having traders
            var activeMarkets = await _context.Markets
                .Where(m => m.Traders.Any() || m.CaretakerId != null)
                .CountAsync();

            // Get payment transactions and revenue
            var levyPayments = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startDate && lp.PaymentDate <= endDate)
                .ToListAsync();
            var paymentTransactions = levyPayments.Count;
            var totalRevenue = levyPayments.Sum(lp => lp.Amount);

            // Calculate compliance rate
            var tradersWithPayments = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startDate && lp.PaymentDate <= endDate)
                .Select(lp => lp.TraderId)
                .Distinct()
                .CountAsync();

            var complianceRate = totalTraders > 0
                ? (decimal)tradersWithPayments / totalTraders * 100
                : 0;

            return new Report
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalTraders = totalTraders,
                TotalCaretakers = totalCaretakers,
                TotalRevenueGenerated = totalRevenue,
                PaymentTransactions = paymentTransactions,
                ActiveMarkets = activeMarkets,
                ComplianceRate = complianceRate
            };
        }
        /*    public async Task<Report> GetMetricsAsync(DateTime startDate, DateTime endDate)
            {
                // Get total traders and caretakers
                var totalTraders = await _context.Traders.CountAsync();
                var totalCaretakers = await _context.Caretakers.CountAsync();

                // Get active markets
                var activeMarkets = await _context.Markets
                    .Where(m => m.Traders.Any() || m.Caretaker.Any())
                    .CountAsync();

                // Get payment transactions and revenue
                var levyPayments = await _context.LevyPayments
                    .Where(lp => lp.PaymentDate >= startDate && lp.PaymentDate <= endDate)
                    .ToListAsync();

                var paymentTransactions = levyPayments.Count;
                var totalRevenue = levyPayments.Sum(lp => lp.Amount);

                // Calculate compliance rate
                var tradersWithPayments = await _context.LevyPayments
                    .Where(lp => lp.PaymentDate >= startDate && lp.PaymentDate <= endDate)
                    .Select(lp => lp.TraderId)
                    .Distinct()
                    .CountAsync();

                var complianceRate = totalTraders > 0
                    ? (decimal)tradersWithPayments / totalTraders * 100
                    : 0;

                return new Report
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalTraders = totalTraders,
                    TotalCaretakers = totalCaretakers,
                    TotalRevenueGenerated = totalRevenue,
                    PaymentTransactions = paymentTransactions,
                    ActiveMarkets = activeMarkets,
                    ComplianceRate = complianceRate
                };
            }*/
        public async Task<IEnumerable<Report>> GetLevyPaymentsBreakdown(int year)
        {
            var payments = await _context.LevyPayments
                .Where(x => x.PaymentDate.Year == year)
                .GroupBy(x => new { x.MarketId, x.Market.MarketName, Month = x.PaymentDate.Month })
                .Select(g => new Report
                {
                    MarketId = g.Key.MarketId,
                    MarketName = g.Key.MarketName,
                    MonthlyRevenue = g.Sum(x => x.Amount),
                    Month = g.Key.Month,
                    Year = year
                })
                .ToListAsync();
            return payments;
        }

        public async Task<Report> GetMarketComplianceRates(string marketId)
        {
            var market = await _context.Markets
                .Include(m => m.Traders)
                .FirstOrDefaultAsync(m => m.Id == marketId);
            if (market == null)
                return null;
            var tradersWithPayments = await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId)
                .Select(lp => lp.TraderId)
                .Distinct()
                .CountAsync();
            return new Report
            {
                MarketId = marketId,
                MarketName = market.MarketName,
                TotalTraders = market.Traders?.Count ?? 0,
                CompliantTraders = tradersWithPayments,
                ComplianceRate = market.Traders?.Count > 0
                    ? (decimal)tradersWithPayments / market.Traders.Count * 100
                    : 0
            };
        }

        public async Task<IEnumerable<Report>> GetLevyCollectionPerMarket()
        {
            return await _context.Markets
                .Select(m => new Report
                {
                    MarketId = m.Id,
                    MarketName = m.MarketName,
                    TotalLevyCollected = m.Traders
                        .SelectMany(t => t.LevyPayments)
                        .Sum(lp => lp.Amount)
                })
                .ToListAsync();
        }

        public async Task<Report> ExportReport(DateTime startDate, DateTime endDate)
        {
            var report = new Report
            {
                MarketCount = await _context.Markets.CountAsync(),
                TotalRevenueGenerated = await _context.LevyPayments
                    .Where(lp => lp.PaymentDate >= startDate && lp.PaymentDate <= endDate)
                    .SumAsync(lp => lp.Amount),
                ReportDate = DateTime.UtcNow
            };
            return report;
        }
    }
}