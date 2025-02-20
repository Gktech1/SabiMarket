using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Infrastructure.Data;

namespace SabiMarket.Infrastructure.Repositories
{
    public class ChairmanRepository : GeneralRepository<Chairman>, IChairmanRepository
    {
        public ChairmanRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Chairman> GetChairmanByIdAsync(string userId, bool trackChanges)
        {
            return await FindByCondition(c => c.Id == userId, trackChanges)
                .Include(c => c.Market) 
                .FirstOrDefaultAsync();
        }

        public async Task<Chairman> GetChairmanByMarketIdAsync(string marketId, bool trackChanges)
        {
            return await FindByCondition(c => c.MarketId == marketId, trackChanges)
                .Include(c => c.Market)
                .FirstOrDefaultAsync();
        }

        public async Task<PaginatorDto<IEnumerable<Chairman>>> GetChairmenWithPaginationAsync(
            PaginationFilter paginationFilter, bool trackChanges)
        {
            return await FindPagedByCondition(
                paginationFilter: paginationFilter,
                trackChanges: trackChanges,
                orderBy: query => query.OrderBy(c => c.CreatedAt));
        }
        public async Task<IEnumerable<Chairman>> SearchChairmenAsync(
            string searchTerm, PaginationFilter paginationFilter, bool trackChanges)
        {
            return await FindByCondition(
                    c => c.FullName.Contains(searchTerm) || c.Email.Contains(searchTerm),
                    trackChanges)
                .OrderBy(c => c.FullName)
                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize)
                .ToListAsync();
        }

        public async Task<bool> ChairmanExistsAsync(string userId, string marketId)
        {
            return await FindByCondition(
                c => c.UserId == userId && c.MarketId == marketId,
                trackChanges: false).AnyAsync();
        }

        public async Task<bool> MarketHasChairmanAsync(string marketId, bool trackChanges)
        {
            return await FindByCondition(
                c => c.MarketId == marketId,
                trackChanges).AnyAsync();
        }

        public void CreateChairman(Chairman chairman) => Create(chairman);
       
        public void DeleteChairman(Chairman chairman) => Delete(chairman);  
       
        public void UpdateChairman(Chairman chairman) => Update(chairman);

        public async Task<IEnumerable<Chairman>> GetReportsByChairmanIdAsync(string chairmanId)
        {
            return await FindByCondition(r => r.Id == chairmanId, trackChanges: false)
                .ToListAsync();
        }
    }
}
