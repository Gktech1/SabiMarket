using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;

namespace SabiMarket.Application.Interfaces
{
    public interface ISubscriptionService
    {
        Task<BaseResponse<bool>> CheckActiveVendorSubscription(string userId);
        Task<BaseResponse<string>> CreateSubscription(CreateSubscriptionDto dto);
    }
}
