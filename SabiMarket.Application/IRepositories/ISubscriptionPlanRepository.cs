using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Application.IRepositories
{
    public interface ISubscriptionPlanRepository
    {
        void AddSubscriptionPlan(SubscriptionPlan plan);
        Task<IEnumerable<SubscriptionPlan>> GetAllSubscriptionPlanForExport(bool trackChanges);
        Task<PaginatorDto<IEnumerable<SubscriptionPlan>>> GetPagedSubscriptionPlan(PaginationFilter paginationFilter);
        Task<SubscriptionPlan> GetSubscriptionPlanById(string id, bool trackChanges);
        Task<PaginatorDto<IEnumerable<SubscriptionPlan>>> SearchSubscriptionPlan(string searchString, PaginationFilter paginationFilter);
        Task<PaginatorDto<IEnumerable<SubscriptionPlan>>> GetPagedSubscriptionPlan(PaginationFilter paginationFilter, string? searchString);
        void UpdateSubscriptionPlan(SubscriptionPlan plan);
        void DeleteSubscriptionPlan(SubscriptionPlan plan);
    }
}
