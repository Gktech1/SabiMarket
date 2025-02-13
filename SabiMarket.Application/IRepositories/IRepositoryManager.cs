using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories.SowFoodIRepositories;
using SabiMarket.Infrastructure.Repositories;

namespace SabiMarket.Application.IRepositories
{
    public interface IRepositoryManager
    {
        public ILevyPaymentRepository LevyPaymentRepository { get; }
        public IMarketRepository MarketRepository { get; }
        public IWaivedProductRepository WaivedProductRepository { get; }
        public ISubscriptionRepository SubscriptionRepository { get; }
        public ISubscriptionPlanRepository SubscriptionPlanRepository { get; }

        public ICaretakerRepository CaretakerRepository { get; }

        public IGoodBoyRepository GoodBoyRepository { get; }

        public ITraderRepository TraderRepository { get; }

        public ILocalGovernmentRepository LocalGovernmentRepository { get; }

        public IVendorRepository VendorRepository { get; }

        public IChairmanRepository ChairmanRepository { get; }
        public IAssistCenterOfficerRepository AssistCenterOfficerRepository { get; }
        public IAuditLogRepository AuditLogRepository { get; }

        public IReportRepository ReportRepository { get; }

        public IAdminRepository AdminRepository { get; }
        public ISowFoodStaffRepository StaffRepository { get; }

        Task SaveChangesAsync();
    }
}
