using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Infrastructure.Data;

namespace SabiMarket.Infrastructure.Services
{
    public class ServiceManager : IServiceManager
    {
        private readonly Lazy<IWaivedProductService> _waivedProductService;
        private readonly Lazy<ISubscriptionService> _subscriptionService;
        public ServiceManager(IRepositoryManager repositoryManager, IHttpContextAccessor contextAccessor, ApplicationDbContext applicationDbContext)
        {
            _waivedProductService = new Lazy<IWaivedProductService>(() => new WaivedProductService(repositoryManager));
            _subscriptionService = new Lazy<ISubscriptionService>(() => new SubscriptionService(contextAccessor, repositoryManager, applicationDbContext));
        }
        public IWaivedProductService IWaivedProductService => _waivedProductService.Value;
        public ISubscriptionService SubscriptionService => _subscriptionService.Value;
    }
}
