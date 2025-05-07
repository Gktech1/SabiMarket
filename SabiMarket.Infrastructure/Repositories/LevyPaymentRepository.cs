using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;
using SabiMarket.Domain.Enum;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Domain.Entities.MarketParticipants;

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

        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetPagedPaymentWithDetails(
     PaymentPeriodEnum? period,
     string searchQuery,
     PaginationFilter paginationFilter,
     bool trackChanges = false)
        {
            // Start building our query
            var baseQuery = _context.LevyPayments
                .AsTracking(trackChanges ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking);

            // Apply period filter if specified
            if (period.HasValue)
            {
                baseQuery = baseQuery.Where(l => l.Period == period.Value);
            }

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim().ToLower();
                bool isNumeric = decimal.TryParse(searchQuery, out decimal searchAmount);

                baseQuery = baseQuery.Where(l =>
                    (l.TransactionReference != null && EF.Functions.Like(l.TransactionReference.ToLower(), $"%{searchQuery}%")) ||
                    (isNumeric && l.Amount == searchAmount) ||
                    (l.Notes != null && EF.Functions.Like(l.Notes.ToLower(), $"%{searchQuery}%"))
                );
            }

            // Apply ordering to the base query
            var orderedBaseQuery = baseQuery
                .OrderByDescending(p => p.PaymentDate)
                .ThenBy(p => p.PaymentStatus);

            // Use your existing pagination on the base query
            var paginatedPayments = await orderedBaseQuery.Paginate(paginationFilter);

            // Now load the related entities for the paginated results
            foreach (var payment in paginatedPayments.PageItems)
            {
                // Get trader with user if TraderId is not null
                if (!string.IsNullOrEmpty(payment.TraderId))
                {
                    var trader = await _context.Traders
                        .AsNoTracking()
                        .Include(t => t.User)
                        .FirstOrDefaultAsync(t => t.Id == payment.TraderId);

                    payment.Trader = trader;


                    if (trader != null && trader.User == null)
                    {
                        // Try to fetch the user directly if it's missing
                        var user = await _context.Users.FindAsync(trader.UserId);
                        if (trader != null)
                        {
                            trader.User = user;
                        }
                    }
                }

                // Similar logic for GoodBoy
                if (!string.IsNullOrEmpty(payment.GoodBoyId))
                {
                    var goodBoy = await _context.GoodBoys
                        .AsNoTracking()
                        .Include(gb => gb.User)
                        .FirstOrDefaultAsync(gb => gb.Id == payment.GoodBoyId);

                    payment.GoodBoy = goodBoy;
                }

                // Also try to load data via ChairmanId since your data shows this relationship
                if (!string.IsNullOrEmpty(payment.ChairmanId) && (payment.Trader == null || payment.Trader.User == null))
                {
                    var chairman = await _context.Chairmen
                        .AsNoTracking()
                        .Include(c => c.User)
                        .FirstOrDefaultAsync(c => c.Id == payment.ChairmanId);

                    if (chairman?.User != null)
                    {
                        // If we couldn't get trader info but chairman is available, create a temporary trader
                        // with chairman user data for display purposes
                        if (payment.Trader == null)
                        {
                            payment.Trader = new Trader { User = chairman.User };
                        }
                    }
                }
            }

            return paginatedPayments;
        }

        /*  public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetPagedPaymentWithDetails(
       PaymentPeriodEnum? period,
       string searchQuery,
       PaginationFilter paginationFilter,
       bool trackChanges = false)
          {
              // Start with base query - using FindByCondition from your repository pattern
              IQueryable<LevyPayment> query = FindByCondition(p => true, trackChanges);

              // Apply period filter if specified
              if (period.HasValue)
              {
                  query = query.Where(l => l.Period == period.Value);
              }

              // Apply search filter if provided
              if (!string.IsNullOrWhiteSpace(searchQuery))
              {
                  searchQuery = searchQuery.Trim().ToLower();
                  bool isNumeric = decimal.TryParse(searchQuery, out decimal searchAmount);

                  query = query.Where(l =>
                      (l.TransactionReference != null && EF.Functions.Like(l.TransactionReference.ToLower(), $"%{searchQuery}%")) ||
                      (isNumeric && l.Amount == searchAmount) ||
                      (l.Notes != null && EF.Functions.Like(l.Notes.ToLower(), $"%{searchQuery}%"))
                  );
              }

              // Apply ordering
              query = query.OrderByDescending(l => l.PaymentDate)
                           .ThenBy(l => l.PaymentStatus);

              // Include related entities - eager loading
              query = query.Include(l => l.Market)
                           .Include(l => l.Trader)
                              .ThenInclude(t => t.User)
                           .Include(l => l.GoodBoy)
                              .ThenInclude(gb => gb.User);

              // Use your existing pagination extension
              return await query.Paginate(paginationFilter);
          }*/

        /*        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetPagedPaymentWithDetails(
               PaymentPeriodEnum? period,
               string searchQuery,
               PaginationFilter paginationFilter,
               bool trackChanges = false)
                {
                    // Start with base query
                    IQueryable<LevyPayment> query = FindByCondition(p => true, trackChanges);

                    // Apply period filter if specified
                    if (period.HasValue)
                    {
                        query = query.Where(l => l.Period == period.Value);
                    }

                    // Include related entities without conditional logic
                    query = query
                        .Include(l => l.Market)
                        .Include(l => l.Trader)
                            .ThenInclude(t => t.User)
                        .Include(l => l.GoodBoy)
                            .ThenInclude(gb => gb.User);

                    // Apply search filter if provided
                    if (!string.IsNullOrWhiteSpace(searchQuery))
                    {
                        searchQuery = searchQuery.Trim().ToLower();
                        bool isNumeric = decimal.TryParse(searchQuery, out decimal searchAmount);

                        query = query.Where(l =>
                            // Search by Trader names
                            (l.Trader != null && l.Trader.User != null && (
                                EF.Functions.Like(l.Trader.User.FirstName.ToLower(), $"%{searchQuery}%") ||
                                EF.Functions.Like(l.Trader.User.LastName.ToLower(), $"%{searchQuery}%"))) ||

                            // Search by GoodBoy names  
                            (l.GoodBoy != null && l.GoodBoy.User != null && (
                                EF.Functions.Like(l.GoodBoy.User.FirstName.ToLower(), $"%{searchQuery}%") ||
                                EF.Functions.Like(l.GoodBoy.User.LastName.ToLower(), $"%{searchQuery}%"))) ||

                            // Search by IDs
                            EF.Functions.Like(l.TraderId ?? "", $"%{searchQuery}%") ||
                            EF.Functions.Like(l.GoodBoyId ?? "", $"%{searchQuery}%") ||

                            // Search by transaction references
                            EF.Functions.Like(l.TransactionReference ?? "", $"%{searchQuery}%") ||

                            // Search by amount if numeric
                            (isNumeric && l.Amount == searchAmount) ||

                            // Search by notes
                            EF.Functions.Like(l.Notes ?? "", $"%{searchQuery}%")
                        );
                    }

                    // Apply ordering
                    query = query
                        .OrderByDescending(l => l.PaymentDate)
                        .ThenBy(l => l.PaymentStatus);

                    return await query.Paginate(paginationFilter);
                }*/
        /* public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetPagedPaymentWithDetails(
       PaymentPeriodEnum? period,
       string? searchQuery,
       PaginationFilter paginationFilter,
       bool trackChanges = false)
         {
             IQueryable<LevyPayment> query = FindAll(trackChanges);

             // Apply period filter if specified
             if (period.HasValue)
             {
                 query = query.Where(l => l.Period == period.Value);
             }

             // Apply search filter if provided
             if (!string.IsNullOrWhiteSpace(searchQuery))
             {
                 searchQuery = searchQuery.Trim().ToLower();
                 query = query.Where(l =>
                     // Search by trader name
                     (l.Trader != null && (
                         l.Trader.User.FirstName.ToLower().Contains(searchQuery) ||
                         l.Trader.User.LastName.ToLower().Contains(searchQuery))) ||
                     // Search by GoodBoy name
                     (l.GoodBoy != null && (
                         l.GoodBoy.User.FirstName.ToLower().Contains(searchQuery) ||
                         l.GoodBoy.User.LastName.ToLower().Contains(searchQuery))) ||
                     // Search by amount
                     l.Amount.ToString().Contains(searchQuery) ||
                    // Search by payment status
                    l.PaymentStatus.ToString().ToLower().Contains(searchQuery)
                 );
             }

             // Include related entities needed for display
             query = query
                 .Include(l => l.Market)
                 .Include(l => l.Trader)
                     .ThenInclude(t => t.User)
                 .Include(l => l.GoodBoy)
                     .ThenInclude(gb => gb.User)
                 .OrderByDescending(l => l.PaymentDate)  // Most recent payments first
                 .ThenBy(l => l.PaymentStatus);  // Group by status

             // Apply pagination
             return await query.Paginate(paginationFilter);
         }*/


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