using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using System.Threading.Tasks;

namespace SabiMarket.Application.IRepositories
{
    public interface IMarketRepository
    {
        void AddMarket(Market market);
        void DeleteMarket(Market market);
        Task<IEnumerable<Market>> GetAllMarketForExport(bool trackChanges);
        Task<Market> GetMarketById(string id, bool trackChanges);
        Task<Market> GetMarketByUserId(string userId, bool trackChanges);
        Task<PaginatorDto<IEnumerable<Market>>> GetPagedMarket(PaginationFilter paginationFilter);
        Task<PaginatorDto<IEnumerable<Market>>> SearchMarket(string searchString, PaginationFilter paginationFilter);
        Task<Market> GetMarketByIdAsync(string marketId, bool trackChanges);
        Task<Market> GetMarketRevenueAsync(string marketId, DateTime startDate, DateTime endDate);

    }
}
