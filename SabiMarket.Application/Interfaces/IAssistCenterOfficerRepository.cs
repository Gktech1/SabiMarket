using SabiMarket.Domain.Entities.MarketParticipants;

namespace SabiMarket.Application.Interfaces
{
    public interface IAssistCenterOfficerRepository
    {
        void AddAssistCenterOfficer(AssistCenterOfficer assistCenter);
        void UpdateAssistCenterOfficer(AssistCenterOfficer assistCenter);
        Task<IEnumerable<AssistCenterOfficer>> GetAllAssistCenterOfficer(bool trackChanges);
    }
}
