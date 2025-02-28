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

        /*public async Task<AssistCenterOfficer> GetByEmailAsync(string email, bool trackChanges = false)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await FindByCondition(a => a.Email.ToLower() == email.ToLower(), trackChanges)
                .FirstOrDefaultAsync();
        }

        public async Task<AssistCenterOfficer> GetByPhoneNumberAsync(string phoneNumber, bool trackChanges = false)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return null;

            return await FindByCondition(a => a.PhoneNumber == phoneNumber, trackChanges)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> OfficerExistsAsync(string email = null, string phoneNumber = null)
        {
            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            var query = FindAll(false).AsQueryable();

            if (!string.IsNullOrWhiteSpace(email))
            {
                var existsByEmail = await query.AnyAsync(o => o.Email.ToLower() == email.ToLower());
                if (existsByEmail)
                    return true;
            }

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                var existsByPhone = await query.AnyAsync(o => o.PhoneNumber == phoneNumber);
                if (existsByPhone)
                    return true;
            }

            return false;
        }*/
    }
}


