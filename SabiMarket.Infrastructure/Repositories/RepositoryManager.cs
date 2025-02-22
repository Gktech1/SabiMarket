using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Application.IRepositories.SowFoodIRepositories;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Repositories.SowFoodLinkedRepositories;

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
        private readonly Lazy<ILocalGovernmentRepository> _localgovernmentRepository;
        private readonly Lazy<IVendorRepository> _vendorRepository;
        private readonly Lazy<IChairmanRepository> _chairmanRepository;
        private readonly Lazy<IAssistCenterOfficerRepository> _assistofficerRepository;
        private readonly Lazy<IAuditLogRepository> _auditlogRepository;
        private readonly Lazy<IReportRepository> _reportRepository;
        private readonly Lazy<IAdminRepository> _adminRepository;
        //private readonly Lazy<ISowFoodStaffRepository> _staffRepository;
        private readonly Lazy<ICustomerRepository> _customerRepository;
        private readonly Lazy<IAdvertisementRepository> _advertisementRepository;
        #region Sow Food
        private readonly Lazy<ISowFoodCompanyRepository> _sowFoodCompanyRepository;
        private readonly Lazy<ISowFoodCompanyCustomerRepository> _sowFoodCompanyCustomerRepository;
        private readonly Lazy<ISowFoodCompanyProductionItemRepository> _sowFoodCompanyProductionItemRepository;
        private readonly Lazy<ISowFoodCompanySalesRecordRepository> _sowFoodCompanySalesRecordRepository;
        private readonly Lazy<ISowFoodCompanyShelfItemRepository> _sowFoodCompanyShelfItemRepository;
        private readonly Lazy<ISowFoodCompanyStaffAppraiserRepository> _sowFoodCompanyStaffAppraiserRepository;
        private readonly Lazy<ISowFoodCompanyStaffAttendanceRepository> _sowFoodCompanyStaffAttendanceRepository;
        private readonly Lazy<ISowFoodCompanyStaffRepository> _sowFoodCompanyStaffRepository;
        #endregion


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
            _localgovernmentRepository = new Lazy<ILocalGovernmentRepository>(() => new LocalGovernmentRepository(_context));
            _vendorRepository = new Lazy<IVendorRepository>(() => new VendorRepository(_context));
            _chairmanRepository = new Lazy<IChairmanRepository>(() => new ChairmanRepository(_context));
            _assistofficerRepository = new Lazy<IAssistCenterOfficerRepository>(() => new AssistCenterOfficerRepository(_context));
            _auditlogRepository = new Lazy<IAuditLogRepository>(() => new AuditLogRepository(_context));
            _reportRepository = new Lazy<IReportRepository>(() => new ReportRepository(_context));
            _adminRepository = new Lazy<IAdminRepository>(() => new AdminRepository(_context));
            //_staffRepository = new Lazy<ISowFoodStaffRepository>(() => new SowFoodStaffRepository(_context));
            _advertisementRepository = new Lazy<IAdvertisementRepository>(() => new AdvertisementRepository(_context));
            _customerRepository = new Lazy<ICustomerRepository>(() => new CustomerRepository(_context));

            _sowFoodCompanyRepository = new Lazy<ISowFoodCompanyRepository>(() => new SowFoodCompanyRepository(_context));
            _sowFoodCompanyCustomerRepository = new Lazy<ISowFoodCompanyCustomerRepository>(() => new SowFoodCompanyCustomerRepository(_context));
            _sowFoodCompanyProductionItemRepository = new Lazy<ISowFoodCompanyProductionItemRepository>(() => new SowFoodCompanyProductionItemRepository(_context));
            _sowFoodCompanySalesRecordRepository = new Lazy<ISowFoodCompanySalesRecordRepository>(() => new SowFoodCompanySalesRecordRepository(_context));
            _sowFoodCompanyShelfItemRepository = new Lazy<ISowFoodCompanyShelfItemRepository>(() => new SowFoodCompanyShelfItemRepository(_context));
            _sowFoodCompanyStaffAppraiserRepository = new Lazy<ISowFoodCompanyStaffAppraiserRepository>(() => new SowFoodCompanyStaffAppraiserRepository(_context));
            _sowFoodCompanyStaffAttendanceRepository = new Lazy<ISowFoodCompanyStaffAttendanceRepository>(() => new SowFoodCompanyStaffAttendanceRepository(_context));
            _sowFoodCompanyStaffRepository = new Lazy<ISowFoodCompanyStaffRepository>(() => new SowFoodCompanyStaffRepository(_context));

        }

        public ILevyPaymentRepository LevyPaymentRepository => _levyPaymentRepository.Value;
        public IMarketRepository MarketRepository => _marketRepository.Value;
        public IWaivedProductRepository WaivedProductRepository => _waivedProductRepository.Value;
        public ISubscriptionRepository SubscriptionRepository => _subscriptionRepository.Value;
        public ISubscriptionPlanRepository SubscriptionPlanRepository => _subscriptionPlanRepository.Value;
        public ICaretakerRepository CaretakerRepository => _cretakerRepository.Value;

        public IGoodBoyRepository GoodBoyRepository => _goodboyRepository.Value;

        public ITraderRepository TraderRepository => _traderRepository.Value;

        public ILocalGovernmentRepository LocalGovernmentRepository => _localgovernmentRepository.Value;

        public IChairmanRepository ChairmanRepository => _chairmanRepository.Value;

        public IVendorRepository VendorRepository => _vendorRepository.Value;

        public IAssistCenterOfficerRepository AssistCenterOfficerRepository => _assistofficerRepository.Value;

        public IAuditLogRepository AuditLogRepository => _auditlogRepository.Value;

        public IReportRepository ReportRepository => _reportRepository.Value;
        public IAdminRepository AdminRepository => _adminRepository.Value;
        //public ISowFoodStaffRepository StaffRepository => _staffRepository.Value;
        public IAdvertisementRepository AdvertisementRepository => _advertisementRepository.Value;
        public ICustomerRepository CustomerRepository => _customerRepository.Value;

        public ISowFoodCompanyRepository SowFoodCompanyRepository => _sowFoodCompanyRepository.Value;

        public ISowFoodCompanyCustomerRepository SowFoodCompanyCustomerRepository => _sowFoodCompanyCustomerRepository.Value;

        public ISowFoodCompanyProductionItemRepository SowFoodCompanyProductionItemRepository => _sowFoodCompanyProductionItemRepository.Value;

        public ISowFoodCompanySalesRecordRepository SowFoodCompanySalesRecordRepository => _sowFoodCompanySalesRecordRepository.Value;

        public ISowFoodCompanyShelfItemRepository SowFoodCompanyShelfItemRepository => _sowFoodCompanyShelfItemRepository.Value;

        public ISowFoodCompanyStaffAppraiserRepository SowFoodCompanyStaffAppraiserRepository => _sowFoodCompanyStaffAppraiserRepository.Value;

        public ISowFoodCompanyStaffAttendanceRepository SowFoodCompanyStaffAttendanceRepository => _sowFoodCompanyStaffAttendanceRepository.Value;

        public ISowFoodCompanyStaffRepository SowFoodCompanyStaffRepository => _sowFoodCompanyStaffRepository.Value;
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
