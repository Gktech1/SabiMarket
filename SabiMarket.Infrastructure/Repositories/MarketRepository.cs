﻿using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{

    public class MarketRepository : GeneralRepository<Market>, IMarketRepository
    {
        private readonly ApplicationDbContext _context;
        public MarketRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void AddMarket(Market market) => Create(market);

        public void DeleteMarket(Market market) => Delete(market);

        public async Task<IEnumerable<Market>> GetAllMarketForExport(bool trackChanges) => await FindAll(trackChanges).Include(a => a.Caretaker)
            .Include(a => a.Traders)
            .Include(m => m.Chairman)
            .Include(m => m.Caretaker)
            .Include(a => a.LocalGovernment)
            .Include(a => a.MarketSections).ToListAsync();


        public async Task<Market> GetMarketById(string id, bool trackChanges) => await FindByCondition(x => x.Id == id, trackChanges)
            .Include(a => a.Caretaker)
            .Include(a => a.Traders)
            .Include(a => a.MarketSections)
            .Include(a => a.LocalGovernment)
            .FirstOrDefaultAsync();

        public async Task<Market> GetMarketByUserId(string userId, bool trackChanges) => await FindByCondition(x => x.Id == userId, trackChanges)
           .Include(a => a.Caretaker)
           .Include(a => a.Traders)
           .Include(a => a.MarketSections)
           .Include(a => a.LocalGovernment)
           .FirstOrDefaultAsync();

        public IQueryable<Market> GetMarketsQuery()
        {
            return FindAll(trackChanges: false)
                .Include(m => m.Chairman)
                .Include(m => m.Caretaker)
                .Include(m => m.Traders)
                .Include(m => m.LocalGovernment);
        }
        public async Task<PaginatorDto<IEnumerable<Market>>> GetPagedMarket(PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                        .Include(a => a.Caretaker)
                        .Include(a => a.Traders)
                        .Include(a => a.MarketSections)
                        .Include(a => a.LocalGovernment)
                        .Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<Market>>> SearchMarket(string searchString, PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                           .Where(a => a.LocalGovernment.Name.Contains(searchString) ||
                           a.MarketName.Contains(searchString))
                           .Paginate(paginationFilter);
        }

        public async Task<Market> GetMarketByIdAsync(string marketId, bool trackChanges)
        {
            // Fetch the market with its primary caretaker
            var market = await FindByCondition(m => m.Id == marketId, trackChanges)
                .Include(m => m.Caretaker)
                    .ThenInclude(c => c.User)
                .Include(m => m.Traders)
                    .ThenInclude(t => t.User)
                .Include(m => m.Chairman)
                .Include(m => m.LocalGovernment)
                .Include(m => m.MarketSections)
                .FirstOrDefaultAsync();

            if (market != null)
            {
                var additionalCaretakers = await _context.Caretakers
                         .Include(c => c.User)
                         .Where(c => c.MarketId == marketId && (market.CaretakerId == null || c.Id != market.CaretakerId))
                         .AsNoTracking()  // For performance
                         .ToListAsync();

                // Create a collection for all caretakers (if not exists)
                if (market.AdditionalCaretakers == null)
                {
                    market.AdditionalCaretakers = new List<Caretaker>();
                }

                // Add additional caretakers
                foreach (var caretaker in additionalCaretakers)
                {
                    market.AdditionalCaretakers.Add(caretaker);
                }
            }

            return market;
        }
        /*   public async Task<Market> GetMarketByIdAsync(string marketId, bool trackChanges)
           {
               var query = FindByCondition(m => m.Id == marketId, trackChanges);

               query = query
                   .Include(a => a.Caretaker)
                   .Include(a => a.Traders)
                   .Include(a => a.MarketSections)
                   .Include(a => a.LocalGovernment);

               return await query.FirstOrDefaultAsync();
           }*/

        public async Task<Market> GetMarketRevenueAsync(string marketId, DateTime startDate, DateTime endDate)
        {
            var market = await FindByCondition(m => m.Id == marketId, trackChanges: false)
                .Include(m => m.Traders)
                .Include(m => m.LocalGovernment)
                .Include(m => m.MarketSections)
                .FirstOrDefaultAsync();

            if (market == null)
                return null;

            // Get levy payments for this market's traders within the date range
            var levyPayments = await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId &&
                       lp.PaymentDate >= startDate &&
                       lp.PaymentDate <= endDate)
                .ToListAsync();

            // Create the revenue entity
            var marketRevenue = new Market
            {
                Id = market.Id,
                MarketName = market.MarketName,
                TotalRevenue = levyPayments.Sum(lp => lp.Amount),
                PaymentTransactions = levyPayments.Count,
                Location = market.Location,
                LocalGovernmentName = market.LocalGovernment?.Name,
                StartDate = startDate,
                EndDate = endDate,
                TotalTraders = market.Traders?.Count ?? 0,
                MarketCapacity = market.Capacity,
                OccupancyRate = market.Capacity > 0
                    ? (decimal)(market.Traders?.Count ?? 0) / market.Capacity * 100
                    : 0
            };

            return marketRevenue;

        }


        public async Task<Market> GetComplianceRatesAsync(string marketId)
        {
            var market = await FindByCondition(m => m.Id == marketId, trackChanges: false)
                .Include(m => m.Traders)
                .Include(m => m.LocalGovernment)
                .Include(m => m.MarketSections)
                .FirstOrDefaultAsync();

            if (market == null)
                return null;

            // Get all levy payments for this market
            var levyPayments = await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId)
                .ToListAsync();

            // Calculate compliance metrics
            var totalTraders = market.Traders?.Count ?? 0;
            var tradersWithPayments = levyPayments
                .Select(lp => lp.TraderId)
                .Distinct()
                .Count();

            // Create the compliance entity
            var marketCompliance = new Market
            {
                Id = market.Id,
                MarketName = market.MarketName,
                Location = market.Location,
                LocalGovernmentName = market.LocalGovernment?.Name,
                TotalTraders = totalTraders,
                ComplianceRate = totalTraders > 0
                    ? (decimal)tradersWithPayments / totalTraders * 100
                    : 0,
                CompliantTraders = tradersWithPayments,
                NonCompliantTraders = totalTraders - tradersWithPayments
            };

            return marketCompliance;
        }

    }
}
