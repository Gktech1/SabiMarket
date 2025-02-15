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

        public void DeleteLevyPayment(LevyPayment levy) => Delete(levy);

        // Added method to get market-specific levy configuration
        public async Task<LevyPayment> GetMarketLevySetup(string marketId, PaymentPeriodEnum period)
        {
            return await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId && lp.Period == period)
                .OrderByDescending(lp => lp.CreatedAt)
                .FirstOrDefaultAsync();
        }

        // Added method to get all configurations for a market
   /*     public async Task<IEnumerable<LevyPayment>> GetMarketLevySetups(string marketId)
        {
            return await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId)
                .GroupBy(lp => lp.Period)
                .Select(g => g.OrderByDescending(lp => lp.CreatedAt).First())
                .OrderBy(lp => lp.Period)
                .ToListAsync();
        }*/

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