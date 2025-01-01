using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Application.IRepositories;
using SabiMarket.Infrastructure.Data;

namespace SabiMarket.Infrastructure.Repositories
{
    public class RepositoryManager : IRepositoryManager
    {
        private readonly ApplicationDbContext _context;
        private readonly Lazy<ILevyPaymentRepository> _levyPaymentRepository;
        private readonly Lazy<IMarketRepository> _marketRepository;
        private readonly Lazy<IWaivedProductRepository> _waivedProductRepository;
        public RepositoryManager(ApplicationDbContext context)
        {
            _context = context;
            _levyPaymentRepository = new Lazy<ILevyPaymentRepository>(() => new LevyPaymentRepository(_context));
            _marketRepository = new Lazy<IMarketRepository>(() => new MarketRepository(_context));
            _waivedProductRepository = new Lazy<IWaivedProductRepository>(() => new WaivedProductRepository(_context));
        }

        public ILevyPaymentRepository LevyPaymentRepository => _levyPaymentRepository.Value;
        public IMarketRepository MarketRepository => _marketRepository.Value;
        public IWaivedProductRepository WaivedProductRepository => _waivedProductRepository.Value;

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
