using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{

    public class MarketRepository : GeneralRepository<Market>, IMarketRepository
    {
        public MarketRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void AddMarket(Market market) => Create(market);

        public async Task<IEnumerable<Market>> GetAllMarketForExport(bool trackChanges) => await FindAll(trackChanges).Include(a => a.Caretakers)
            .Include(a => a.Traders)
            .Include(a => a.Sections).ToListAsync();

        public async Task<Market> GetMarketById(string id, bool trackChanges) => await FindByCondition(x => x.Id == id, trackChanges)
            .Include(a => a.Caretakers)
            .Include(a => a.Traders)
            .Include(a => a.Sections)
            .Include(a => a.LocalGovernment)
            .FirstOrDefaultAsync();

        public async Task<Market> GetMarketByUserId(string userId, bool trackChanges) => await FindByCondition(x => x.Id == userId, trackChanges)
           .Include(a => a.Caretakers)
           .Include(a => a.Traders)
           .Include(a => a.Sections)
           .Include(a => a.LocalGovernment)
           .FirstOrDefaultAsync();

        public async Task<PaginatorDto<IEnumerable<Market>>> GetPagedMarket(PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                        .Include(a => a.Caretakers)
                        .Include(a => a.Traders)
                        .Include(a => a.Sections)
                        .Include(a => a.LocalGovernment)
                        .Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<Market>>> SearchMarket(string searchString, PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                           .Where(a => a.LocalGovernment.Name.Contains(searchString) ||
                           a.Name.Contains(searchString))
                           .Paginate(paginationFilter);
        }

    }
}
