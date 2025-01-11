using SabiMarket.Application.Interfaces;

namespace SabiMarket.Application.IRepositories
{
    public interface IRepositoryManager
    {
        public ILevyPaymentRepository LevyPaymentRepository { get; }
        public IMarketRepository MarketRepository { get; }
        public IWaivedProductRepository WaivedProductRepository { get; }
        public ISubscriptionRepository SubscriptionRepository { get; }

        public ICaretakerRepository CaretakerRepository { get; }

        public IGoodBoyRepository GoodBoyRepository { get; }

        public ITraderRepository TraderRepository { get; }

        Task SaveChangesAsync();
    }
}
