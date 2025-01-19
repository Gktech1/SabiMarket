using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{
    public class LocalGovernmentRepository : GeneralRepository<LocalGovernment>, ILocalGovernmentRepository
    {
        private readonly ApplicationDbContext _repositoryContext;

        public LocalGovernmentRepository(ApplicationDbContext repositoryContext)
            : base(repositoryContext)
        {
            _repositoryContext = repositoryContext;
        }

        public async Task<LocalGovernment> GetLocalGovernmentById(string id, bool trackChanges) =>
            await FindByCondition(lg => lg.Id == id, trackChanges)
                .FirstOrDefaultAsync();

        public async Task<LocalGovernment> GetLocalGovernmentWithUsers(string id, bool trackChanges) =>
            await FindByCondition(lg => lg.Id == id, trackChanges)
                .Include(lg => lg.Users)
                .FirstOrDefaultAsync();

        public async Task<LocalGovernment> GetLocalGovernmentWithMarkets(string id, bool trackChanges) =>
            await FindByCondition(lg => lg.Id == id, trackChanges)
                .Include(lg => lg.Markets)
                .FirstOrDefaultAsync();

        public async Task<PaginatorDto<IEnumerable<LocalGovernment>>> GetLocalGovernmentsWithPagination(
            PaginationFilter paginationFilter, bool trackChanges)
        {
            var query = FindAll(trackChanges)
                .Include(lg => lg.Markets)
                .OrderBy(lg => lg.Name);

            return await query.Paginate(paginationFilter);
        }

        public async Task<LocalGovernment> GetLocalGovernmentByName(
            string name, string state, bool trackChanges) =>
            await FindByCondition(
                lg => lg.Name.ToLower() == name.ToLower() &&
                      lg.State.ToLower() == state.ToLower(),
                trackChanges)
            .FirstOrDefaultAsync();

        public async Task<decimal> GetTotalRevenue(string localGovernmentId)
        {
            var localGovernment = await FindByCondition(
                lg => lg.Id == localGovernmentId, false)
                .FirstOrDefaultAsync();

            return localGovernment?.CurrentRevenue ?? 0;
        }

        public async Task<bool> LocalGovernmentExists(string name, string state) =>
            await FindByCondition(
                lg => lg.Name.ToLower() == name.ToLower() &&
                      lg.State.ToLower() == state.ToLower(),
                trackChanges: false)
            .AnyAsync();

        public async Task<bool> LocalGovernmentExist(string localgovernmentId) =>
            await FindByCondition(
                lg => lg.Id == localgovernmentId,
                trackChanges: false)
            .AnyAsync();
        public void CreateLocalGovernment(LocalGovernment localGovernment) =>
            Create(localGovernment);

        public void UpdateLocalGovernment(LocalGovernment localGovernment) =>
            Update(localGovernment);

        public void DeleteLocalGovernment(LocalGovernment localGovernment) =>
            Delete(localGovernment);
    }

    // DTOs
   /* public class LocalGovernmentDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string Address { get; set; }
        public decimal CurrentRevenue { get; set; }
        public int TotalMarkets { get; set; }
        public int TotalUsers { get; set; }
    }*/

}
