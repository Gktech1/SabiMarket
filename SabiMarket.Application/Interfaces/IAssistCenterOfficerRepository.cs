using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.MarketParticipants;

namespace SabiMarket.Application.Interfaces
{
    public interface IAssistCenterOfficerRepository
    {
        void AddAssistCenterOfficer(AssistCenterOfficer assistCenter);
        void UpdateAssistCenterOfficer(AssistCenterOfficer assistCenter);
        Task<IEnumerable<AssistCenterOfficer>> GetAllAssistCenterOfficer(bool trackChanges);
        Task<PaginatorDto<IEnumerable<AssistCenterOfficer>>> GetAssistantOfficersAsync(
    string chairmanId, PaginationFilter paginationFilter, bool trackChanges);
        Task<AssistCenterOfficer> GetByIdAsync(string officerId, bool trackChanges);
        Task<AssistCenterOfficer> GetAssistantOfficerByIdAsync(string officerId, bool trackChanges);
    }
}
