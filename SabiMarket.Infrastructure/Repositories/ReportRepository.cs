using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Extensions;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Enum;
using SabiMarket.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<DashboardReportDto> GetDashboardReportDataAsync(
           string lgaFilter = null,
           string marketFilter = null,
           int? year = null,
           TimeFrame timeFrame = TimeFrame.ThisWeek)
        {
            // Get date range from timeframe or year
            DateRangeDto dateRange;

            if (year.HasValue)
            {
                // If year is specified, create a date range for the entire year
                dateRange = new DateRangeDto
                {
                    StartDate = new DateTime(year.Value, 1, 1),
                    EndDate = new DateTime(year.Value, 12, 31),
                    IsPreset = true,
                    PresetRange = "YearlyCustom",
                    DateRangeType = "Yearly"
                };
            }
            else
            {
                // Get DateRangeDto from TimeFrame
                dateRange = timeFrame.GetDateRange();
            }

            // Call the implementation using the date range
            return await GetDashboardDataByDateRangeAsync(
                lgaFilter,
                marketFilter,
                year,
                timeFrame,
                dateRange);
        }

        public async Task<DashboardReportDto> GetDashboardDataByDateRangeAsync(
            string lgaFilter = null,
            string marketFilter = null,
            int? year = null,
            TimeFrame timeFrame = TimeFrame.Custom,
            DateRangeDto dateRange = null)
        {
            // Use provided date range or default to current month
            if (dateRange == null)
            {
                dateRange = TimeFrame.ThisMonth.GetDateRange();
            }

            var startDate = dateRange.StartDate;
            var endDate = dateRange.EndDate;

            // Apply filters to market query
            var marketsQuery = _context.Markets.AsQueryable();
            if (!string.IsNullOrEmpty(lgaFilter))
            {
                marketsQuery = marketsQuery.Where(m => m.LocalGovernmentName == lgaFilter);
            }
            if (!string.IsNullOrEmpty(marketFilter))
            {
                marketsQuery = marketsQuery.Where(m => m.MarketName == marketFilter);
            }

            // Get markets
            var markets = await marketsQuery.ToListAsync();
            var marketIds = markets.Select(m => m.Id).ToList();

            // 1. Market Count Card Data
            var marketCount = new MarketCountDto
            {
                Count = markets.Count,
                Description = "Total Number of registered markets"
            };

            // 2. Total Revenue Card Data
            var totalRevenue = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startDate &&
                           lp.PaymentDate <= endDate &&
                           marketIds.Contains(lp.MarketId))
                .SumAsync(lp => lp.Amount);

            var totalRevenueDto = new TotalRevenueDto
            {
                Amount = totalRevenue,
                TimeFrame = timeFrame,
                TimeFrameDisplay = dateRange.IsPreset ? dateRange.PresetRange : timeFrame.ToDisplayString(),
                Description = "Total levy paid"
            };

            // 3. Levy Payments Breakdown Graph Data
            var monthlyData = await GetMonthlyLevyDataAsync(startDate, endDate, marketIds);

            // 4. Compliance Rates Donut Chart
            var complianceData = await GetComplianceRatesAsync(startDate, endDate, markets, year);

            // 5. Levy Collection Per Market
            var levyCollection = await GetLevyCollectionPerMarketAsync(startDate, endDate, markets, year);

            // Create and return the dashboard DTO
            return new DashboardReportDto
            {
                MarketCount = marketCount,
                TotalRevenue = totalRevenueDto,
                LevyPayments = monthlyData,
                ComplianceRates = complianceData,
                LevyCollection = levyCollection,
                CurrentDateTime = DateTime.Now
            };
        }

        private async Task<LevyPaymentsBreakdownDto> GetMonthlyLevyDataAsync(
            DateTime startDate,
            DateTime endDate,
            List<string> marketIds) // Changed from List<Guid> to List<string>
        {
            // Generate months dynamically based on the date range
            var months = new List<string>();
            var currentDate = new DateTime(startDate.Year, startDate.Month, 1);

            while (currentDate <= endDate)
            {
                months.Add(currentDate.ToString("MMM"));
                currentDate = currentDate.AddMonths(1);
            }

            // Get all markets for which we need data
            var markets = await _context.Markets
                .Where(m => marketIds.Contains(m.Id))
                .OrderByDescending(m => _context.LevyPayments
                    .Where(lp => lp.MarketId == m.Id &&
                              lp.PaymentDate >= startDate &&
                              lp.PaymentDate <= endDate)
                    .Sum(lp => lp.Amount))
                .Take(3) // Take top 3 by revenue
                .ToListAsync();

            // Define colors
            var colors = new[] { "#FF6B8E", "#20C997", "#FFD700" };

            // Get payments data for each market
            var marketData = new List<MarketMonthlyDataDto>();

            for (int i = 0; i < markets.Count; i++)
            {
                var market = markets[i];
                var values = new List<decimal>();

                // Get data for each month
                foreach (var month in months)
                {
                    var monthNum = DateTime.ParseExact(month, "MMM", CultureInfo.InvariantCulture).Month;
                    var year = currentDate.Month > monthNum ? currentDate.Year : startDate.Year;

                    var monthStart = new DateTime(year, monthNum, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    var amount = await _context.LevyPayments
                        .Where(lp => lp.MarketId == market.Id &&
                                  lp.PaymentDate >= monthStart &&
                                  lp.PaymentDate <= monthEnd)
                        .SumAsync(lp => lp.Amount);

                    values.Add(amount);
                }

                marketData.Add(new MarketMonthlyDataDto
                {
                    MarketName = market.MarketName,
                    Color = colors[i % colors.Length],
                    Values = values
                });
            }

            return new LevyPaymentsBreakdownDto
            {
                Months = months,
                MarketData = marketData
            };
        }

        private async Task<ComplianceRatesDto> GetComplianceRatesAsync(
            DateTime startDate,
            DateTime endDate,
            List<Market> markets,
            int? year = null)
        {
            var marketCompliance = new List<MarketReportComplianceDto>();
            var colors = new[] { "#FF6B8E", "#20C997", "#FFD700" };

            // Get the top markets by number of traders
            var marketsWithTraderCount = new List<(Market Market, int TraderCount)>();

            foreach (var market in markets)
            {
                var traderCount = await _context.Traders
                    .CountAsync(t => t.MarketId == market.Id);

                marketsWithTraderCount.Add((market, traderCount));
            }

            // Take top 3 markets by trader count
            var topMarkets = marketsWithTraderCount
                .OrderByDescending(m => m.TraderCount)
                .Take(3)
                .ToList();

            for (int i = 0; i < topMarkets.Count; i++)
            {
                var (market, totalTraders) = topMarkets[i];

                var compliantTraders = await _context.LevyPayments
                    .Where(lp => lp.MarketId == market.Id &&
                              lp.PaymentDate >= startDate &&
                              lp.PaymentDate <= endDate)
                    .Select(lp => lp.TraderId)
                    .Distinct()
                    .CountAsync();

                int compliancePercentage = totalTraders > 0
                    ? (int)Math.Round((double)compliantTraders / totalTraders * 100)
                    : 0;

                marketCompliance.Add(new MarketReportComplianceDto
                {
                    MarketName = market.MarketName,
                    CompliancePercentage = compliancePercentage,
                    Color = colors[i % colors.Length]
                });
            }

            return new ComplianceRatesDto
            {
                Year = year ?? DateTime.Now.Year,
                MarketCompliance = marketCompliance
            };
        }

        private async Task<LevyCollectionPerMarketDto> GetLevyCollectionPerMarketAsync(
            DateTime startDate,
            DateTime endDate,
            List<Market> markets,
            int? year = null)
        {
            var marketLevy = new List<MarketLevyDto>();
            decimal totalAmount = 0;

            // Get markets sorted by revenue
            var marketsWithRevenue = new List<(Market Market, decimal Revenue)>();

            foreach (var market in markets)
            {
                var revenue = await _context.LevyPayments
                    .Where(lp => lp.MarketId == market.Id &&
                              lp.PaymentDate >= startDate &&
                              lp.PaymentDate <= endDate)
                    .SumAsync(lp => lp.Amount);

                marketsWithRevenue.Add((market, revenue));
                totalAmount += revenue;
            }

            // Take top markets by revenue
            var topMarkets = marketsWithRevenue
                .OrderByDescending(m => m.Revenue)
                .Take(3)
                .ToList();

            foreach (var (market, revenue) in topMarkets)
            {
                marketLevy.Add(new MarketLevyDto
                {
                    MarketName = market.MarketName,
                    Amount = revenue
                });
            }

            return new LevyCollectionPerMarketDto
            {
                Year = year ?? DateTime.Now.Year,
                TotalAmount = totalAmount,
                MarketLevy = marketLevy
            };
        }

        public async Task<FilterOptionsDto> GetFilterOptionsAsync()
        {
            // Get all LGAs - fixed to use LocalGovernmentName
            var lgas = await _context.Markets
                .Select(m => m.LocalGovernmentName)
                .Where(lga => !string.IsNullOrEmpty(lga))
                .Distinct()
                .ToListAsync();

            // Get all Markets
            var markets = await _context.Markets
                .Select(m => m.MarketName)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .ToListAsync();

            // Get all years with data
            var years = await _context.LevyPayments
                .Select(lp => lp.PaymentDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            // If no historical data, add current year
            if (!years.Any())
            {
                years.Add(DateTime.Now.Year);
            }

            return new FilterOptionsDto
            {
                LGAs = lgas,
                Markets = markets,
                Years = years,
                TimeFrames = TimeFrameDateRangeExtensions.GetTimeFrameOptions()
            };
        }

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


/*using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Extensions;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Enum;
using SabiMarket.Infrastructure.Data;
using System.Globalization;
using System.Linq;

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

        public async Task<DashboardReportDto> GetDashboardDataAsync(
           string lgaFilter = null,
           string marketFilter = null,
           int? year = null,
           TimeFrame timeFrame = TimeFrame.ThisWeek)
        {
            // Get date range from timeframe or year
            DateRangeDto dateRange;

            if (year.HasValue)
            {
                // If year is specified, create a date range for the entire year
                dateRange = new DateRangeDto
                {
                    StartDate = new DateTime(year.Value, 1, 1),
                    EndDate = new DateTime(year.Value, 12, 31),
                    IsPreset = true,
                    PresetRange = "YearlyCustom",
                    DateRangeType = "Yearly"
                };
            }
            else
            {
                // Get DateRangeDto from TimeFrame
                dateRange = timeFrame.GetDateRange();
            }

            // Call the implementation using the date range
            return await GetDashboardDataByDateRangeAsync(
                lgaFilter,
                marketFilter,
                year,
                timeFrame,
                dateRange);
        }

        public async Task<DashboardReportDto> GetDashboardDataByDateRangeAsync(
            string lgaFilter = null,
            string marketFilter = null,
            int? year = null,
            TimeFrame timeFrame = TimeFrame.Custom,
            DateRangeDto dateRange = null)
        {
            // Use provided date range or default to current month
            if (dateRange == null)
            {
                dateRange = TimeFrame.ThisMonth.GetDateRange();
            }

            var startDate = dateRange.StartDate;
            var endDate = dateRange.EndDate;

            // Apply filters to market query
            var marketsQuery = _context.Markets.AsQueryable();
            if (!string.IsNullOrEmpty(lgaFilter))
            {
                marketsQuery = marketsQuery.Where(m => m.LocalGovernmentName == lgaFilter);

            }
            if (!string.IsNullOrEmpty(marketFilter))
            {
                marketsQuery = marketsQuery.Where(m => m.MarketName == marketFilter);
            }

            // Get markets
            var markets = await marketsQuery.ToListAsync();
            var marketIds = markets.Select(m => m.Id).ToList();

            // 1. Market Count Card Data
            var marketCount = new MarketCountDto
            {
                Count = markets.Count,
                Description = "Total Number of registered markets"
            };

            // 2. Total Revenue Card Data
            var totalRevenue = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startDate &&
                           lp.PaymentDate <= endDate &&
                           marketIds.Contains(lp.MarketId))
                .SumAsync(lp => lp.Amount);

            var totalRevenueDto = new TotalRevenueDto
            {
                Amount = totalRevenue,
                TimeFrame = timeFrame,
                TimeFrameDisplay = dateRange.IsPreset ? dateRange.PresetRange : timeFrame.ToDisplayString(),
                Description = "Total levy paid"
            };


            // 3. Levy Payments Breakdown Graph Data
            var monthlyData = await GetMonthlyLevyDataAsync(startDate, endDate, marketIds);

            // 4. Compliance Rates Donut Chart
            var complianceData = await GetComplianceRatesAsync(startDate, endDate, markets, year);

            // 5. Levy Collection Per Market
            var levyCollection = await GetLevyCollectionPerMarketAsync(startDate, endDate, markets, year);

            // Create and return the dashboard DTO
            return new DashboardReportDto
            {
                MarketCount = marketCount,
                TotalRevenue = totalRevenueDto,
                LevyPayments = monthlyData,
                ComplianceRates = complianceData,
                LevyCollection = levyCollection,
                CurrentDateTime = DateTime.Now
            };
        }

        private async Task<LevyPaymentsBreakdownDto> GetMonthlyLevyDataAsync(
            DateTime startDate,
            DateTime endDate,
            List<Guid> marketIds)
        {
            // Generate months dynamically based on the date range
            var months = new List<string>();
            var currentDate = new DateTime(startDate.Year, startDate.Month, 1);

            while (currentDate <= endDate)
            {
                months.Add(currentDate.ToString("MMM"));
                currentDate = currentDate.AddMonths(1);
            }

            // Get all markets for which we need data
            var markets = await _context.Markets
                .Where(m => marketIds.Contains(m.Id))
                .OrderByDescending(m => _context.LevyPayments
                    .Where(lp => lp.MarketId == m.Id &&
                              lp.PaymentDate >= startDate &&
                              lp.PaymentDate <= endDate)
                    .Sum(lp => lp.Amount))
                .Take(3) // Take top 3 by revenue
                .ToListAsync();

            // Define colors
            var colors = new[] { "#FF6B8E", "#20C997", "#FFD700" };

            // Get payments data for each market
            var marketData = new List<MarketMonthlyDataDto>();

            for (int i = 0; i < markets.Count; i++)
            {
                var market = markets[i];
                var values = new List<decimal>();

                // Get data for each month
                foreach (var month in months)
                {
                    var monthNum = DateTime.ParseExact(month, "MMM", CultureInfo.InvariantCulture).Month;
                    var year = currentDate.Month > monthNum ? currentDate.Year : startDate.Year;

                    var monthStart = new DateTime(year, monthNum, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    var amount = await _context.LevyPayments
                        .Where(lp => lp.MarketId == market.Id &&
                                  lp.PaymentDate >= monthStart &&
                                  lp.PaymentDate <= monthEnd)
                        .SumAsync(lp => lp.Amount);

                    values.Add(amount);
                }

                marketData.Add(new MarketMonthlyDataDto
                {
                    MarketName = market.MarketName,
                    Color = colors[i % colors.Length],
                    Values = values
                });
            }

            return new LevyPaymentsBreakdownDto
            {
                Months = months,
                MarketData = marketData
            };
        }

        private async Task<ComplianceRatesDto> GetComplianceRatesAsync(
            DateTime startDate,
            DateTime endDate,
            List<Market> markets,
            int? year = null)
        {
            var marketCompliance = new List<MarketReportComplianceDto>();
            var colors = new[] { "#FF6B8E", "#20C997", "#FFD700" };

            // Get the top markets by number of traders
            var marketsWithTraderCount = new List<(Market Market, int TraderCount)>();

            foreach (var market in markets)
            {
                var traderCount = await _context.Traders
                    .CountAsync(t => t.MarketId == market.Id);

                marketsWithTraderCount.Add((market, traderCount));
            }

            // Take top 3 markets by trader count
            var topMarkets = marketsWithTraderCount
                .OrderByDescending(m => m.TraderCount)
                .Take(3)
                .ToList();

            for (int i = 0; i < topMarkets.Count; i++)
            {
                var (market, totalTraders) = topMarkets[i];

                var compliantTraders = await _context.LevyPayments
                    .Where(lp => lp.MarketId == market.Id &&
                              lp.PaymentDate >= startDate &&
                              lp.PaymentDate <= endDate)
                    .Select(lp => lp.TraderId)
                    .Distinct()
                    .CountAsync();

                int compliancePercentage = totalTraders > 0
                    ? (int)Math.Round((double)compliantTraders / totalTraders * 100)
                    : 0;

                marketCompliance.Add(new MarketReportComplianceDto
                {
                    MarketName = market.MarketName,
                    CompliancePercentage = compliancePercentage,
                    Color = colors[i % colors.Length]
                });
            }

            return new ComplianceRatesDto
            {
                Year = year ?? DateTime.Now.Year,
                MarketCompliance = marketCompliance
            };
        }

        private async Task<LevyCollectionPerMarketDto> GetLevyCollectionPerMarketAsync(
            DateTime startDate,
            DateTime endDate,
            List<Market> markets,
            int? year = null)
        {
            var marketLevy = new List<MarketLevyDto>();
            decimal totalAmount = 0;

            // Get markets sorted by revenue
            var marketsWithRevenue = new List<(Market Market, decimal Revenue)>();

            foreach (var market in markets)
            {
                var revenue = await _context.LevyPayments
                    .Where(lp => lp.MarketId == market.Id &&
                              lp.PaymentDate >= startDate &&
                              lp.PaymentDate <= endDate)
                    .SumAsync(lp => lp.Amount);

                marketsWithRevenue.Add((market, revenue));
                totalAmount += revenue;
            }

            // Take top markets by revenue
            var topMarkets = marketsWithRevenue
                .OrderByDescending(m => m.Revenue)
                .Take(3)
                .ToList();

            foreach (var (market, revenue) in topMarkets)
            {
                marketLevy.Add(new MarketLevyDto
                {
                    MarketName = market.MarketName,
                    Amount = revenue
                });
            }

            return new LevyCollectionPerMarketDto
            {
                Year = year ?? DateTime.Now.Year,
                TotalAmount = totalAmount,
                MarketLevy = marketLevy
            };
        }

        public async Task<FilterOptionsDto> GetFilterOptionsAsync()
        {
            // Get all LGAs
            var lgas = await _context.Markets
                .Select(m => m.LocalGovernment)
                .Where(lga => !string.IsNullOrEmpty(lga))
                .Distinct()
                .ToListAsync();

            // Get all Markets
            var markets = await _context.Markets
                .Select(m => m.MarketName)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .ToListAsync();

            // Get all years with data
            var years = await _context.LevyPayments
                .Select(lp => lp.PaymentDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            // If no historical data, add current year
            if (!years.Any())
            {
                years.Add(DateTime.Now.Year);
            }

            return new FilterOptionsDto
            {
                LGAs = lgas,
                Markets = markets,
                Years = years,
                TimeFrames = TimeFrameDateRangeExtensions.GetTimeFrameOptions()
            };
        }
    }
    *//*    public async Task<Report> GetMetricsAsync(DateTime startDate, DateTime endDate)
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
        }*//*
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
}*/