using iText.Commons.Actions.Contexts;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.Interfaces;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{
    public class CaretakerRepository : GeneralRepository<Caretaker>, ICaretakerRepository
    {
        private readonly ApplicationDbContext _repositoryContext;
        public CaretakerRepository(ApplicationDbContext repositoryContext)
            : base(repositoryContext)
        {
            _repositoryContext = repositoryContext;
        }

        public IQueryable<Caretaker> GetCaretakersQuery()
        {
            return FindAll(trackChanges: false)
                .Include(c => c.User)
                .Include(c => c.Chairman)
                .Include(c => c.Markets);
        }
        public async Task<Caretaker> GetCaretakerById(string userId, bool trackChanges) =>
         await FindByCondition(x => x.UserId == userId, trackChanges)
             .Include(a => a.Markets)
             .Include(a => a.GoodBoys)
                 .ThenInclude(gb => gb.LevyPayments)
             .Include(a => a.AssignedTraders)
                 .ThenInclude(t => t.LevyPayments)
             .FirstOrDefaultAsync();

        public async Task<Caretaker> GetCaretakerByMarketId(string marketId, bool trackChanges) =>
            await FindByCondition(x => x.MarketId == marketId, trackChanges)
                .Include(a => a.Markets)
                .Include(a => a.GoodBoys)
                    .ThenInclude(gb => gb.LevyPayments)
                .Include(a => a.AssignedTraders)
                    .ThenInclude(t => t.Market)
                .FirstOrDefaultAsync();

        public async Task<Caretaker> GetCaretakerByLocalGovernmentId(string LGAId, bool trackChanges) =>
           await FindByCondition(x => x.LocalGovernmentId == LGAId, trackChanges)
               .Include(a => a.Markets)
               .Include(a => a.GoodBoys)
                   .ThenInclude(gb => gb.LevyPayments)
               .Include(a => a.AssignedTraders)
                   .ThenInclude(t => t.Market)
               .FirstOrDefaultAsync();

        /* public async Task<PaginatorDto<IEnumerable<Caretaker>>> GetCaretakersWithPagination(
             PaginationFilter paginationFilter, bool trackChanges)
         {
             var query = FindAll(trackChanges)
                 .Include(a => a.Markets)
                 .Include(a => a.GoodBoys)
                 .Include(a => a.AssignedTraders)
                 .OrderBy(c => c.CreatedAt);

             return await query.Paginate(paginationFilter);
         }*/

        public async Task<PaginatorDto<IEnumerable<Caretaker>>> GetCaretakersWithPagination(
    PaginationFilter paginationFilter, bool trackChanges)
        {
            var query = FindAll(trackChanges)
                .Include(a => a.Markets)
                .Include(a => a.GoodBoys)
                .Include(a => a.AssignedTraders)
                .Include(a => a.User) // Include User to populate firstName, lastName, email, etc.
                .OrderBy(c => c.CreatedAt);

            return await query.Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<Caretaker>>> GetCaretakersAsync(
           string chairmanId, PaginationFilter paginationFilter, bool trackChanges)
        {
            return await FindPagedByCondition(
                expression: c => c.ChairmanId == chairmanId,
                paginationFilter: paginationFilter,
                trackChanges: trackChanges,
                orderBy: query => query.OrderBy(c => c.CreatedAt));
        }

        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetLevyPayments(
            string caretakerId, PaginationFilter paginationFilter, bool trackChanges)
        {
            var query = _repositoryContext.LevyPayments
                .Where(lp => lp.GoodBoy.CaretakerId == caretakerId ||
                            lp.Trader.CaretakerId == caretakerId)
                .Include(lp => lp.GoodBoy)
                .Include(lp => lp.Trader)
                .OrderByDescending(lp => lp.CreatedAt);

            return await query.Paginate(paginationFilter);
        }

        public async Task<LevyPayment> GetLevyPaymentDetails(string levyId, bool trackChanges) =>
            await _repositoryContext.LevyPayments
                .Where(lp => lp.Id == levyId)
                .Include(lp => lp.GoodBoy)
                .Include(lp => lp.Trader)
                .FirstOrDefaultAsync();

        public async Task<PaginatorDto<IEnumerable<GoodBoy>>> GetGoodBoys(
            string caretakerId, PaginationFilter paginationFilter, bool trackChanges)
        {
            var query = _repositoryContext.GoodBoys
                .Where(gb => gb.CaretakerId == caretakerId)
                .Include(gb => gb.User)
                .Include(gb => gb.LevyPayments)
                .OrderBy(gb => gb.CreatedAt);

            return await query.Paginate(paginationFilter);
        }

        public async Task<bool> CaretakerExists(string chairmanId, string marketId) =>
            await FindByCondition(x => x.UserId == chairmanId && x.MarketId == marketId,
                trackChanges: false).AnyAsync();

        public void CreateCaretaker(Caretaker caretaker) => Create(caretaker);

        public void DeleteCaretaker(Caretaker caretaker) => Delete(caretaker);

        /*public async Task<int> GetCaretakerCountAsync()
        {
            return await FindAll(trackChanges: false).CountAsync();
        }*/

        public async Task<int> GetCaretakerCountAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
                var query = FindAll(trackChanges: false);

                if (startDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt <= endDate.Value);
                }

                // Optional: You might want to only count active caretakers
                query = query.Where(c => !c.IsBlocked);

                return await query.CountAsync();
        }

        public async Task<IEnumerable<Caretaker>> GetAllCaretakers(bool trackChanges) =>
            await FindAll(trackChanges)
                .Include(c => c.Markets)
                .Include(c => c.GoodBoys)
                .Include(c => c.AssignedTraders)
                .ToListAsync();

        public async Task<bool> ExistsAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            return await _repositoryContext.Caretakers
                .AnyAsync(c => c.Id == id);
        }
    }
}

