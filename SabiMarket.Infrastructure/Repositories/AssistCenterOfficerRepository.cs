using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Infrastructure.Data;
using System.Security.Policy;

namespace SabiMarket.Infrastructure.Repositories
{
    public class AssistCenterOfficerRepository : GeneralRepository<AssistCenterOfficer>, IAssistCenterOfficerRepository
    {
        public AssistCenterOfficerRepository(ApplicationDbContext context) : base(context) { }

        public void AddAssistCenterOfficer(AssistCenterOfficer assistCenter) => Create(assistCenter);

        public async Task<PaginatorDto<IEnumerable<AssistCenterOfficer>>> GetAssistantOfficersAsync(
    string chairmanId, PaginationFilter paginationFilter, bool trackChanges)
        {
            return await FindPagedByCondition(
                expression: a => a.ChairmanId == chairmanId,
                paginationFilter: paginationFilter,
                trackChanges: trackChanges,
                orderBy: query => query.OrderBy(a => a.CreatedAt));
        }

        public void UpdateAssistCenterOfficer(AssistCenterOfficer assistCenter) => Update(assistCenter);

        public async Task<IEnumerable<AssistCenterOfficer>> GetAllAssistCenterOfficer(bool trackChanges) => await FindAll(trackChanges).ToListAsync();

        public async Task<AssistCenterOfficer> GetByIdAsync(string officerId, bool trackChanges)
        {
            return await FindByCondition(a => a.Id == officerId, trackChanges)
                .FirstOrDefaultAsync();
        }

        public async Task<AssistCenterOfficer> GetAssistantOfficerByIdAsync(string officerId, bool trackChanges)
        {
            var query = FindByCondition(a => a.Id == officerId, trackChanges);

            // Include related entities one by one
            query = query.Include(a => a.User);
            query = query.Include(a => a.Market);
            query = query.Include(a => a.LocalGovernment);

            // Disable tracking if needed
            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync();
        }

    }
}


