using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Enum;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IRepositoryManager _repositoryManager;
        private readonly ApplicationDbContext _applicationDbContext;
        public SubscriptionService(IHttpContextAccessor contextAccessor, IRepositoryManager repositoryManager, ApplicationDbContext applicationDbContext)
        {
            _contextAccessor = contextAccessor;
            _repositoryManager = repositoryManager;
            _applicationDbContext = applicationDbContext;
        }
        public async Task<BaseResponse<string>> CreateSubscription(CreateSubscriptionDto dto)
        {
            var loggedInUser = Helper.GetUserDetails(_contextAccessor);
            if (loggedInUser.Role.ToLower() != "officeattendant")
            {
                return ResponseFactory.Fail<string>(new UnauthorizedException("UnIdentified User"), "Active User is Not Authorized.");
            }
            var subscription = new Subscription
            {
                Amount = dto.Amount,
                SubscriptionActivatorId = loggedInUser.Id,
                PaymentMethod = dto.PaymentMethod,
                ProofOfPayment = dto.ProofOfPayment,
                SubscriptionStartDate = DateTime.UtcNow,
                SubscriptionEndDate = DateTime.UtcNow.AddMonths(1),
                SubscriberId = dto.SubscriberId,
                //SubscriptionType = dto.SubscriptionType
            };
            _repositoryManager.SubscriptionRepository.AddSubscription(subscription);
            await _repositoryManager.SaveChangesAsync();

            return ResponseFactory.Success<string>("Success");
        }

        public async Task<BaseResponse<bool>> CheckActiveVendorSubscription(string userId)
        {
            var getUser = await _applicationDbContext.Vendors.Where(x => x.UserId == userId).FirstOrDefaultAsync();
            if (getUser == null)
            {
                return ResponseFactory.Fail<bool>(new NotFoundException("User is not a Vendor."), "Vendor not found.");
            }
            if (!getUser.IsSubscriptionActive && getUser.SubscriptionEndDate < DateTime.UtcNow.AddHours(1))
            {
                return ResponseFactory.Success<bool>(false, "User does not have an active subscription");
            }
            return ResponseFactory.Success<bool>(true, "User has an active subscription.");

        }

    }
}
