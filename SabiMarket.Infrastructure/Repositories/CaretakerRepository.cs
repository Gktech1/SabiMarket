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

        public async Task<Caretaker> GetCaretakerById(string userId, bool trackChanges) =>
         await FindByCondition(x => x.UserId == userId, trackChanges)
             .Include(a => a.Market)
             .Include(a => a.GoodBoys)
                 .ThenInclude(gb => gb.LevyPayments)
             .Include(a => a.AssignedTraders)
                 .ThenInclude(t => t.LevyPayments)
             .FirstOrDefaultAsync();

        public async Task<Caretaker> GetCaretakerByMarketId(string marketId, bool trackChanges) =>
            await FindByCondition(x => x.MarketId == marketId, trackChanges)
                .Include(a => a.Market)
                .Include(a => a.GoodBoys)
                    .ThenInclude(gb => gb.LevyPayments)
                .Include(a => a.AssignedTraders)
                    .ThenInclude(t => t.Market)
                .FirstOrDefaultAsync();

        public async Task<PaginatorDto<IEnumerable<Caretaker>>> GetCaretakersWithPagination(
            PaginationFilter paginationFilter, bool trackChanges)
        {
            var query = FindAll(trackChanges)
                .Include(a => a.Market)
                .Include(a => a.GoodBoys)
                .Include(a => a.AssignedTraders)
                .OrderBy(c => c.CreatedAt);

            return await query.Paginate(paginationFilter);
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

        public async Task<bool> CaretakerExists(string userId, string marketId) =>
            await FindByCondition(x => x.UserId == userId && x.MarketId == marketId,
                trackChanges: false).AnyAsync();

        public void CreateCaretaker(Caretaker caretaker) => Create(caretaker);

        public void DeleteCaretaker(Caretaker caretaker) => Delete(caretaker);
    }
}

