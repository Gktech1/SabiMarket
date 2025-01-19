using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.Interfaces;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Infrastructure.Data;

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

    }
}
