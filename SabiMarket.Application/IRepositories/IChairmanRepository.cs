﻿using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Domain.Entities.Administration;
using System.Linq.Expressions;

namespace SabiMarket.Application.IRepositories
{
    public interface IChairmanRepository : IGeneralRepository<Chairman>
    {
        Task<Chairman> GetChairmanByIdAsync(string userId, bool trackChanges);
        Task<Chairman> GetChairmanByMarketIdAsync(string marketId, bool trackChanges);
        Task<PaginatorDto<IEnumerable<Chairman>>> GetChairmenWithPaginationAsync(
       PaginationFilter paginationFilter, bool trackChanges, string? searchTerm);
        Task<bool> ChairmanExistsAsync(string userId, string marketId);
        void CreateChairman(Chairman chairman);
        void DeleteChairman(Chairman chairman);

        // Additional Methods
        Task<IEnumerable<Chairman>> SearchChairmenAsync(string searchTerm, PaginationFilter paginationFilter, bool trackChanges);
        Task<bool> MarketHasChairmanAsync(string marketId, bool trackChanges);
        void UpdateChairman(Chairman chairman); 
        Task<IEnumerable<Chairman>> GetReportsByChairmanIdAsync(string chairmanId);
        Task<Chairman> GetChairmanById(string UserId, bool trackChanges);
        Task<int> CountAsync(Expression<Func<Chairman, bool>> predicate);
        Task<ChairmanDashboardStatsDto> GetChairmanDashboardStatsAsync(string chairmanId);
    }

}
