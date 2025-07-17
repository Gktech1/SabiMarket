using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{
    public class SubscriptionPlanRepository : GeneralRepository<SubscriptionPlan>, ISubscriptionPlanRepository
    {
        public SubscriptionPlanRepository(ApplicationDbContext context) : base(context) { }

        public void AddSubscriptionPlan(SubscriptionPlan plan) => Create(plan);
        public void UpdateSubscriptionPlan(SubscriptionPlan plan) => Update(plan);
        public void DeleteSubscriptionPlan(SubscriptionPlan plan) => Delete(plan);

        public async Task<IEnumerable<SubscriptionPlan>> GetAllSubscriptionPlanForExport(bool trackChanges) => await FindAll(trackChanges).ToListAsync();

        public async Task<SubscriptionPlan> GetSubscriptionPlanById(string id, bool trackChanges) => await FindByCondition(x => x.Id == id, trackChanges).FirstOrDefaultAsync();

        public async Task<PaginatorDto<IEnumerable<SubscriptionPlan>>> GetPagedSubscriptionPlan(PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                       .Paginate(paginationFilter);
        }
        public async Task<PaginatorDto<IEnumerable<SubscriptionPlan>>> GetPagedSubscriptionPlan(PaginationFilter paginationFilter, string? searchString, string? frequencyFilter, DateTime? dateCreatedFilter)
        {
            IQueryable<SubscriptionPlan>? query = FindAll(false).AsQueryable();

            // 🔍 Full-text search
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var loweredSearch = searchString.ToLower();
                query = query.Where(p =>
                    p.Frequency.ToLower().Contains(loweredSearch) ||
                    p.Amount.ToString().ToLower().Contains(loweredSearch) ||
                    p.UserType.ToLower().Contains(loweredSearch));
            }

            // ✅ Filter by Frequency (exact match)
            if (!string.IsNullOrWhiteSpace(frequencyFilter))
            {
                var loweredFreq = frequencyFilter.ToLower();
                query = query.Where(p => p.Frequency.ToLower() == loweredFreq);
            }

            // ✅ Filter by DateCreated (date-only comparison)
            if (dateCreatedFilter.HasValue)
            {
                var dateOnly = dateCreatedFilter.Value.Date;
                query = query.Where(p => p.CreatedAt.Date == dateOnly);
            }

            return await query.Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<SubscriptionPlan>>> SearchSubscriptionPlan(string searchString, PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                           .Where(a =>
                           a.Frequency.Contains(searchString))
                           .Paginate(paginationFilter);
        }

    }
}
