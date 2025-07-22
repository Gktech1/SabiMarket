using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Enum;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Services
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ApplicationDbContext _context;
        public SubscriptionPlanService(IRepositoryManager repositoryManager, ApplicationDbContext context)
        {
            _repositoryManager = repositoryManager;
            _context = context;
        }

        public async Task<BaseResponse<string>> CreateSubscriptionPlan(CreateSubscriptionPlanDto dto)
        {
            var subscription = new SubscriptionPlan
            {
                Frequency = dto.Frequency,
                Amount = dto.Amount,
                IsActive = true,
                UserType = dto.UserType
            };

            _repositoryManager.SubscriptionPlanRepository.AddSubscriptionPlan(subscription);
            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success("Success", "Subscription Created Successfully.");
        }
        public async Task<BaseResponse<PaginatorDto<IEnumerable<GetSubscriptionDto>>>> GetAllSubscriptionPlans(PaginationFilter filter, string? searchString, string? frequencyFilter, DateTime? dateCreatedFilter)
        {
            var pagedResult = await _repositoryManager.SubscriptionPlanRepository
                .GetPagedSubscriptionPlan(filter, searchString, frequencyFilter, dateCreatedFilter);


            if (pagedResult == null || !pagedResult.PageItems.Any())
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<GetSubscriptionDto>>>(
                    new NotFoundException("No Record Found."), "Record not found.");
            }

            var result = pagedResult.PageItems.Select(plan => new GetSubscriptionDto
            {
                Id = plan.Id,
                Frequency = plan.Frequency,
                Currency = (int)plan.Currency,
                Amount = plan.Amount,
                UserType = plan.UserType,
                DateCreated = plan.CreatedAt
            }).ToList();

            var response = new PaginatorDto<IEnumerable<GetSubscriptionDto>>
            {
                PageItems = result,
                CurrentPage = pagedResult.CurrentPage,
                PageSize = pagedResult.PageSize,
                TotalItems = pagedResult.TotalItems,
                NumberOfPages = pagedResult.NumberOfPages
            };

            return ResponseFactory.Success(response);
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<GetSubscriptionUserDto>>>> GetSubscribersBySubscriptionPlan(PaginationFilter filter, string? subscriptionPlanId, DateTime? createdAtFilter, int? currencyTypeFilter)
        {
            var subscriptions = _context.Subscriptions
                .Where(x => x.Id == subscriptionPlanId)
                .Include(s => s.Subscriber)
                .AsQueryable();

            // ✅ Filter by CreatedAt (date-only)
            if (createdAtFilter.HasValue)
            {
                var dateOnly = createdAtFilter.Value.Date;
                subscriptions = subscriptions.Where(s => s.CreatedAt.Date == dateOnly);
            }

            // ✅ Filter by CurrencyType enum
            if (currencyTypeFilter.HasValue)
            {
                subscriptions = subscriptions.Where(s => s.SubscriptionPlan.Currency == (CurrencyTypeEnum)currencyTypeFilter.Value);
            }

            if (!await subscriptions.AnyAsync())
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<GetSubscriptionUserDto>>>(
                    new NotFoundException("No subscribers found."),
                    "No subscribers found for the provided plan.");
            }

            var result = await subscriptions.Select(s => new GetSubscriptionUserDto
            {
                UserId = s.Subscriber.Id,
                FullName = $"{s.Subscriber.FirstName} {s.Subscriber.LastName}",
                Email = s.Subscriber.Email,
                IsActive = s.IsActive,
                SubscriptionStartDate = s.SubscriptionStartDate,
                SubscriptionEndDate = s.SubscriptionEndDate
            }).Paginate(filter);

            return ResponseFactory.Success(result);
        }


        public async Task<BaseResponse<PaginatorDto<IEnumerable<SubscriptionPlan>>>> GetAllSubscriptionPlans(PaginationFilter filter)
        {
            var subscriptionPlan = await _repositoryManager.SubscriptionPlanRepository.GetPagedSubscriptionPlan(filter);
            if (subscriptionPlan == null)
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<SubscriptionPlan>>>(new NotFoundException("No Record Found."), "Record not found.");
            }

            return ResponseFactory.Success<PaginatorDto<IEnumerable<SubscriptionPlan>>>(subscriptionPlan);
        }
        public async Task<BaseResponse<GetSubScriptionPlanDto>> GetSubscriptionPlanById(string Id)
        {
            var subscriptionPlan = await _repositoryManager.SubscriptionPlanRepository.GetSubscriptionPlanById(Id, false);

            if (subscriptionPlan == null)
            {
                return ResponseFactory.Fail<GetSubScriptionPlanDto>(new NotFoundException("No Record Found."), "Record not found.");
            }
            var count = await _context.Subscriptions.CountAsync(s => s.SubscriptionPlanId == Id);
            var activeCount = await _context.Subscriptions.CountAsync(s => s.SubscriptionPlanId == Id && s.IsActive);
            var InactiveCount = await _context.Subscriptions.CountAsync(s => s.SubscriptionPlanId == Id && !s.IsActive);
            var result = new GetSubScriptionPlanDto
            {
                Amount = subscriptionPlan.Amount,
                CreatedAt = subscriptionPlan.CreatedAt,
                IsActive = subscriptionPlan.IsActive,
                UpdatedAt = subscriptionPlan?.UpdatedAt,
                Frequency = subscriptionPlan.Frequency,
                NumberOfSubscribers = count,
                NumberOfActiveSubscribers = activeCount,
                NumberOfInActiveSubscribers = InactiveCount
            };

            return ResponseFactory.Success(result);
        }

        public async Task<BaseResponse<string>> UpdateSubscriptionPlan(UpdateSubscriptionPlanDto dto)
        {
            var plan = await _repositoryManager.SubscriptionPlanRepository.GetSubscriptionPlanById(dto.Id, true);
            if (plan == null)
            {
                return ResponseFactory.Fail<string>(new NotFoundException("No Record Found."), "Record not found.");
            }

            plan.Frequency = dto.Frequency;
            plan.Amount = dto.Amount;
            plan.UserType = dto.UserType;

            _repositoryManager.SubscriptionPlanRepository.UpdateSubscriptionPlan(plan);
            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success("Success");

        }
        public async Task<BaseResponse<string>> DeleteSubscriptionPlan(string Id)
        {
            var plan = await _repositoryManager.SubscriptionPlanRepository.GetSubscriptionPlanById(Id, false);
            if (plan == null)
            {
                return ResponseFactory.Fail<string>(new NotFoundException("No Record Found."), "Record not found.");
            }

            _repositoryManager.SubscriptionPlanRepository.DeleteSubscriptionPlan(plan);
            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success("Success");

        }

    }
}
