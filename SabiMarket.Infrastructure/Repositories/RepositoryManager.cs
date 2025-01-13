using SabiMarket.Application.Interfaces;
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
        private readonly Lazy<ISubscriptionPlanRepository> _subscriptionPlanRepository;
        private readonly Lazy<ISubscriptionRepository> _subscriptionRepository;
        private readonly Lazy<ICaretakerRepository> _cretakerRepository;
        private readonly Lazy<IGoodBoyRepository> _goodboyRepository;
        private readonly Lazy<ITraderRepository> _traderRepository;
        public RepositoryManager(ApplicationDbContext context)
        {
            _context = context;
            _levyPaymentRepository = new Lazy<ILevyPaymentRepository>(() => new LevyPaymentRepository(_context));
            _marketRepository = new Lazy<IMarketRepository>(() => new MarketRepository(_context));
            _waivedProductRepository = new Lazy<IWaivedProductRepository>(() => new WaivedProductRepository(_context));
            _subscriptionRepository = new Lazy<ISubscriptionRepository>(() => new SubscriptionRepository(_context));
            _subscriptionPlanRepository = new Lazy<ISubscriptionPlanRepository>(() => new SubscriptionPlanRepository(_context));
            _cretakerRepository = new Lazy<ICaretakerRepository>(() => new CaretakerRepository(_context));
            _goodboyRepository = new Lazy<IGoodBoyRepository>(() => new GoodBoyRepository(_context));
            _traderRepository = new Lazy<ITraderRepository>(() => new TraderRepository(_context));
        }

        public ILevyPaymentRepository LevyPaymentRepository => _levyPaymentRepository.Value;
        public IMarketRepository MarketRepository => _marketRepository.Value;
        public IWaivedProductRepository WaivedProductRepository => _waivedProductRepository.Value;
        public ISubscriptionRepository SubscriptionRepository => _subscriptionRepository.Value;
        public ISubscriptionPlanRepository SubscriptionPlanRepository => _subscriptionPlanRepository.Value;
        public ICaretakerRepository CaretakerRepository => _cretakerRepository.Value;

        public IGoodBoyRepository GoodBoyRepository => _goodboyRepository.Value;

        public ITraderRepository TraderRepository => _traderRepository.Value;

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
