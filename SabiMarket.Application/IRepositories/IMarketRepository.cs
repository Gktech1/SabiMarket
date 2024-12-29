using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;

namespace SabiMarket.Application.IRepositories
{
    public interface IMarketRepository
    {
        void AddMarket(Market market);
        Task<IEnumerable<Market>> GetAllMarketForExport(bool trackChanges);
        Task<Market> GetMarketById(string id, bool trackChanges);
        Task<PaginatorDto<IEnumerable<Market>>> GetPagedMarket(PaginationFilter paginationFilter);
        Task<PaginatorDto<IEnumerable<Market>>> SearchMarket(string searchString, PaginationFilter paginationFilter);
    }
}
