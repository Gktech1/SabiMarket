using iText.Commons.Actions.Contexts;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{
    public class ChairmanRepository : GeneralRepository<Chairman>, IChairmanRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public ChairmanRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext; 
        }

        public async Task<Chairman> GetChairmanByIdAsync(string userId, bool trackChanges)
        {
            return await FindByCondition(c => c.LocalGovernmentId == userId, trackChanges)
                .Include(c => c.Market) 
                .FirstOrDefaultAsync();
        }

        /* public async Task<Chairman> GetChairmanById(string userId, bool trackChanges)
         {
             return await FindByCondition(c => c.Id == userId, trackChanges)
                 .Include(c => c.Market)
                 .FirstOrDefaultAsync();
         }*/

        public async Task<Chairman> GetChairmanById(string userId, bool trackChanges)
        {
            return await FindByCondition(c => c.UserId == userId, trackChanges)
                .Include(c => c.Market)
                .Include(c => c.User)
                .Include(c => c.LocalGovernment)  // Make sure to include LocalGovernment
                .Select(c => new Chairman
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    MarketId = c.MarketId,
                    LocalGovernmentId = c.LocalGovernmentId, // Explicitly select LocalGovernmentId
                    FullName = c.FullName,
                    Email = c.Email,
                    Title = c.Title,
                    Office = c.Office,
                    TotalRecords = c.TotalRecords,
                    TermStart = c.TermStart,
                    TermEnd = c.TermEnd,
                    LastLoginAt = c.LastLoginAt,
                    User = c.User,
                    Market = c.Market,
                    LocalGovernment = c.LocalGovernment,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Chairman> GetChairmanByMarketIdAsync(string marketId, bool trackChanges)
        {
            return await FindByCondition(c => c.MarketId == marketId, trackChanges)
                .Include(c => c.Market)
                .FirstOrDefaultAsync();
        }

        public async Task<PaginatorDto<IEnumerable<Chairman>>> GetChairmenWithPaginationAsync(
      PaginationFilter paginationFilter, bool trackChanges, string? searchTerm)
        {
            // Get the base query, assuming FindByCondition works as a base filter
            var query = FindByCondition(
                expression: _ => true,  // Initially return all Chairmen (no specific filter)
                trackChanges: trackChanges
            );

            // Apply eager loading for related entities
            query = query.Include(c => c.User)  // Include User details
                         .Include(c => c.Market) // Include Market details
                         .Include(c => c.LocalGovernment) // Include Local Government details
                         .OrderBy(c => c.CreatedAt);  // Apply sorting by creation date

            // Apply search filter if a search term is provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    (c.User.FirstName.Contains(searchTerm) ||
                     c.User.LastName.Contains(searchTerm) ||
                     c.User.Email.Contains(searchTerm) ||
                     c.Market.MarketName.Contains(searchTerm) ||
                     c.LocalGovernment.Name.Contains(searchTerm)));
            }

            // Apply pagination (this returns a Task, so await it)
            var result = await query.Paginate(paginationFilter);

            return result;
        }





        /*  public async Task<PaginatorDto<IEnumerable<Chairman>>> GetChairmenWithPaginationAsync(
       PaginationFilter paginationFilter, bool trackChanges)
          {
              return await FindPagedByCondition(
                  expression: _ => true,
                  paginationFilter: paginationFilter,
                  trackChanges: trackChanges,
                  orderBy: query => query
                      .Include(c => c.User)
                      .Include(c => c.Market)
                      .Include(c => c.LocalGovernment)
                      .OrderBy(c => c.CreatedAt));
          }*/
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
