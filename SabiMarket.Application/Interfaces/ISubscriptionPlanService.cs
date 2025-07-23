using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Application.Interfaces;

public interface ISubscriptionPlanService
{
    Task<BaseResponse<string>> CreateSubscriptionPlan(CreateSubscriptionPlanDto dto);
    Task<BaseResponse<PaginatorDto<IEnumerable<SubscriptionPlan>>>> GetAllSubscriptionPlans(PaginationFilter filter);
    Task<BaseResponse<GetSubScriptionPlanDto>> GetSubscriptionPlanById(string Id);
    Task<BaseResponse<string>> UpdateSubscriptionPlan(UpdateSubscriptionPlanDto dto);
    Task<BaseResponse<PaginatorDto<IEnumerable<GetSubscriptionDto>>>> GetAllSubscriptionPlans(PaginationFilter filter, string? searchString, string? frequencyFilter, DateTime? dateCreatedFilter);
    Task<BaseResponse<string>> DeleteSubscriptionPlan(string Id);
    Task<BaseResponse<PaginatorDto<IEnumerable<GetSubscriptionUserDto>>>> GetSubscribersBySubscriptionPlan(PaginationFilter filter, string? subscriptionPlanId, DateTime? createdAtFilter, int? currencyTypeFilter, string? statusFilter);
}
