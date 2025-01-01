using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;

namespace SabiMarket.Infrastructure.Services
{
    public class ServiceManager : IServiceManager
    {
        private readonly Lazy<IWaivedProductService> _waivedProductService;
        public ServiceManager(IRepositoryManager repositoryManager)
        {
            _waivedProductService = new Lazy<IWaivedProductService>(() => new WaivedProductService(repositoryManager));
        }
        public IWaivedProductService IWaivedProductService => _waivedProductService.Value;
    }
}
