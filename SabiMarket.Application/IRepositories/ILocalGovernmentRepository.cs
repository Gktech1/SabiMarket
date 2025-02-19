using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;

namespace SabiMarket.Infrastructure.Repositories
{
    public interface ILocalGovernmentRepository : IGeneralRepository<LocalGovernment>
    {
        Task<LocalGovernment> GetLocalGovernmentById(string id, bool trackChanges);
        Task<LocalGovernment> GetLocalGovernmentWithUsers(string id, bool trackChanges);
        Task<LocalGovernment> GetLocalGovernmentWithMarkets(string id, bool trackChanges);
        Task<PaginatorDto<IEnumerable<LocalGovernment>>> GetLocalGovernmentsWithPagination(
            PaginationFilter paginationFilter, bool trackChanges);
        Task<LocalGovernment> GetLocalGovernmentByName(string name, string state, bool trackChanges);
        Task<decimal> GetTotalRevenue(string localGovernmentId);
        Task<bool> LocalGovernmentExists(string name, string state);
        Task<bool> LocalGovernmentExist(string localgovernmentId);
        void CreateLocalGovernment(LocalGovernment localGovernment);
        void UpdateLocalGovernment(LocalGovernment localGovernment);
        void DeleteLocalGovernment(LocalGovernment localGovernment);
        IQueryable<LocalGovernment> GetFilteredLGAsQuery(LGAFilterRequestDto filterDto);
        Task<PaginatorDto<IEnumerable<LocalGovernment>>> GetLocalGovernmentAreas(
     string searchTerm,
     string officerName,  // Changed from chairmanName to officerName
     bool? isActive,
     string state,
     string orderBy,
     bool? isDescending,
     PaginationFilter paginationFilter);
    }
}
