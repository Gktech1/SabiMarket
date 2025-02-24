using AutoMapper;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Enum;
using static RoleResponseDto;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<LevyPayment, LevyInfoResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.Name))
            .ForMember(dest => dest.MarketAddress, opt => opt.MapFrom(src => src.Market.Location))
            .ForMember(dest => dest.MarketType, opt => opt.MapFrom(src => "")) // Add proper mapping if MarketType exists
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.PaymentFrequencyDays, opt => opt.MapFrom(src => ConvertPeriodToDays(src.Period)))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.LastModifiedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => "")) // Map if needed
            .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => "")) // Map if needed
            .ForMember(dest => dest.TraderOccupancy, opt => opt.MapFrom(src => "")) // Add proper mapping if added
            .ForMember(dest => dest.ActiveTradersCount, opt => opt.MapFrom(src => src.Market.TotalTraders))
            .ForMember(dest => dest.PaidTradersCount, opt => opt.MapFrom(src =>
                src.Market.PaymentTransactions))
            .ForMember(dest => dest.DefaultersTradersCount, opt => opt.MapFrom(src =>
                src.Market.TotalTraders - src.Market.PaymentTransactions))
            .ForMember(dest => dest.ExpectedDailyRevenue, opt => opt.MapFrom(src =>
                src.Amount * src.Market.TotalTraders))
            .ForMember(dest => dest.ActualDailyRevenue, opt => opt.MapFrom(src =>
                src.Market.TotalRevenue));

        CreateMap<CreateChairmanRequestDto, Chairman>()
          .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
          .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
          .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
          .ForMember(dest => dest.User, opt => opt.Ignore());


        CreateMap<Chairman, ChairmanResponseDto>()
          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
          .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
          .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
          .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
          .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
          .ForMember(dest => dest.LocalGovernmentId, opt => opt.MapFrom(src => src.LocalGovernmentId))
          .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market != null ? src.Market.Name : string.Empty))
          .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.User.IsActive ? "Active" : "Inactive"))
          .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
          .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
        
        CreateMap<Admin, AdminDashboardStatsDto>()
           .ForMember(dest => dest.RegisteredLGAs, opt => opt.MapFrom(src => src.RegisteredLGAs))
           .ForMember(dest => dest.ActiveChairmen, opt => opt.MapFrom(src => src.ActiveChairmen))
           .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.TotalRevenue));

        CreateMap<Caretaker, CaretakerResponseDto>()
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
           .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
           .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsBlocked))
           .ForMember(dest => dest.Market, opt => opt.MapFrom(src => src.Market))
           .ForMember(dest => dest.GoodBoys, opt => opt.MapFrom(src => src.GoodBoys))
           .ForMember(dest => dest.AssignedTraders, opt => opt.MapFrom(src => src.AssignedTraders));

        CreateMap<AssistCenterOfficer, AssistantOfficerResponseDto>()
          .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}".Trim()))
          .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
          .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
          .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
          .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.Name));

        CreateMap<CreateAssistantOfficerRequestDto, AssistCenterOfficer>()
             .ForMember(dest => dest.UserLevel, opt => opt.MapFrom(src => src.Level))
             .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
             .ForMember(dest => dest.User, opt => opt.Ignore()) // Handle User creation separately
             .ForMember(dest => dest.Market, opt => opt.Ignore())
             .ForMember(dest => dest.Chairman, opt => opt.Ignore())
             .ForMember(dest => dest.LocalGovernment, opt => opt.Ignore());

        // If you need to map from AssistCenterOfficer to CreateAssistantOfficerRequestDto
        CreateMap<AssistCenterOfficer, CreateAssistantOfficerRequestDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.UserLevel))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId));

        CreateMap<UpdateProfileDto, Chairman>()
           .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EmailAddress))
           .ForMember(dest => dest.LocalGovernmentId, opt => opt.MapFrom(src => src.LocalGovernmentId))
           .ForMember(dest => dest.User, opt => opt.Ignore())
           .ForMember(dest => dest.Market, opt => opt.Ignore())
           .ForMember(dest => dest.LocalGovernment, opt => opt.Ignore());

        CreateMap<Chairman, UpdateProfileDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.User.Address))
            .ForMember(dest => dest.LocalGovernmentId, opt => opt.MapFrom(src => src.LocalGovernmentId))
            .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.User.ProfileImageUrl));

        CreateMap<AssistCenterOfficer, AssistantOfficerResponseDto>()
          .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}".Trim()))
          .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
          .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
          .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
          .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.Name));

        CreateMap<CreateLevyRequestDto, LevyPayment>()
           .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
           .ForMember(dest => dest.TraderId, opt => opt.MapFrom(src => src.TraderId))
           .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
           .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period))
           .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
           .ForMember(dest => dest.HasIncentive, opt => opt.MapFrom(src => src.HasIncentive))
           .ForMember(dest => dest.IncentiveAmount, opt => opt.MapFrom(src => src.IncentiveAmount))
           .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
           .ForMember(dest => dest.GoodBoyId, opt => opt.MapFrom(src => src.GoodBoyId))
           .ForMember(dest => dest.CollectionDate, opt => opt.MapFrom(src => src.CollectionDate))
           .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => PaymentStatusEnum.Pending))
           .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => DateTime.UtcNow))
           .ForMember(dest => dest.TransactionReference, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
           .ForMember(dest => dest.Trader, opt => opt.Ignore())
           .ForMember(dest => dest.Market, opt => opt.Ignore())
           .ForMember(dest => dest.GoodBoy, opt => opt.Ignore())
           .ForMember(dest => dest.Chairman, opt => opt.Ignore());

        CreateMap<UpdateLevyRequestDto, LevyPayment>()
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.TraderId, opt => opt.MapFrom(src => src.TraderId))
            .ForMember(dest => dest.GoodBoyId, opt => opt.MapFrom(src => src.GoodBoyId))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.Period, opt => opt.Condition(src => src.Period.HasValue))
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period.Value))
            .ForMember(dest => dest.PaymentMethod, opt => opt.Condition(src => src.PaymentMethod.HasValue))
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.Value))
            .ForMember(dest => dest.PaymentStatus, opt => opt.Condition(src => src.PaymentStatus.HasValue))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus.Value))
            .ForMember(dest => dest.HasIncentive, opt => opt.MapFrom(src => src.HasIncentive))
            .ForMember(dest => dest.IncentiveAmount, opt => opt.MapFrom(src => src.IncentiveAmount))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));


        CreateMap<CreateMarketRequestDto, Market>()
           .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
           .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
           .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
           .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
           .ForMember(dest => dest.MarketCapacity, opt => opt.MapFrom(src => src.Capacity))
           .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateTime.UtcNow))
           .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateTime.UtcNow.AddYears(1)))
           .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => 0))
           .ForMember(dest => dest.PaymentTransactions, opt => opt.MapFrom(src => 0))
           .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => 0))
           .ForMember(dest => dest.OccupancyRate, opt => opt.MapFrom(src => 0))
           .ForMember(dest => dest.ComplianceRate, opt => opt.MapFrom(src => 0))
           .ForMember(dest => dest.CompliantTraders, opt => opt.MapFrom(src => 0))
           .ForMember(dest => dest.NonCompliantTraders, opt => opt.MapFrom(src => 0))
           .ForMember(dest => dest.Chairman, opt => opt.Ignore())
           .ForMember(dest => dest.LocalGovernment, opt => opt.Ignore())
           .ForMember(dest => dest.Traders, opt => opt.Ignore())
           .ForMember(dest => dest.Caretakers, opt => opt.Ignore())
           .ForMember(dest => dest.Sections, opt => opt.Ignore());

        CreateMap<UpdateMarketRequestDto, Market>()
           .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
           .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
           .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
           .ForMember(dest => dest.Capacity, opt => opt.Condition(src => src.Capacity.HasValue))
           .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity.Value))
           .ForMember(dest => dest.MarketCapacity, opt => opt.Condition(src => src.Capacity.HasValue))
           .ForMember(dest => dest.MarketCapacity, opt => opt.MapFrom(src => src.Capacity.Value));

        CreateMap<Market, MarketRevenueDto>()
           .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Name))
           .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.TotalRevenue))
           .ForMember(dest => dest.PaymentMethods, opt => opt.Ignore()) // This should be calculated separately
           .ForMember(dest => dest.GrowthRate, opt => opt.Ignore())     // This needs calculation
           .ForMember(dest => dest.AverageDaily, opt => opt.Ignore())   // This needs calculation
           .ForMember(dest => dest.AverageMonthly, opt => opt.Ignore());// This needs calculation

        CreateMap<AssistCenterOfficer, AssistantOfficerResponseDto>()
           .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}".Trim()))
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
           .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
           .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
           .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.Name))
           .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
           .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

        CreateMap<Caretaker, CaretakerResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.Market, opt => opt.MapFrom(src => src.Market))
            .ForMember(dest => dest.GoodBoys, opt => opt.MapFrom(src => src.GoodBoys))
            .ForMember(dest => dest.AssignedTraders, opt => opt.MapFrom(src => src.AssignedTraders))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<Market, MarketResponseDto>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
           .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
           .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
           .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.TotalTraders))
           .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.MarketCapacity))
           .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
           .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
           .ForMember(dest => dest.CaretakerIds, opt => opt.MapFrom(src => src.Caretakers.Select(c => c.Id)));

        CreateMap<GoodBoy, GoodBoyResponseDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.Name))
            .ForMember(dest => dest.TraderId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TraderOccupancy, opt => opt.MapFrom(src => "Open Space"))
            .ForMember(dest => dest.PaymentFrequency, opt => opt.MapFrom(src => "2 days - N500"))
            .ForMember(dest => dest.LastPaymentDate, opt => opt.MapFrom(src =>
                src.LevyPayments.OrderByDescending(p => p.PaymentDate).FirstOrDefault().PaymentDate))
            .ForMember(dest => dest.LevyPayments, opt => opt.MapFrom(src => src.LevyPayments));

        CreateMap<CreateGoodBoyRequestDto, ApplicationUser>()
           .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
           .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<CreateGoodBoyRequestDto, GoodBoy>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => StatusEnum.Unlocked));

        CreateMap<GoodBoy, GoodBoyResponseDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.Name))
            .ForMember(dest => dest.TraderId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TraderOccupancy, opt => opt.MapFrom(src => "Open Space"))
            .ForMember(dest => dest.PaymentFrequency, opt => opt.MapFrom(src => "2 days - N500"))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => 500m))
            .ForMember(dest => dest.LastPaymentDate, opt => opt.MapFrom(src =>
                src.LevyPayments.OrderByDescending(p => p.PaymentDate).FirstOrDefault().PaymentDate));

        CreateMap<ProcessLevyPaymentDto, LevyPayment>()
        .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => DateTime.UtcNow))
        .ForMember(dest => dest.TransactionReference, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
        .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => PaymentStatusEnum.Paid));

        CreateMap<AuditLog, AuditLogResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.UserFullName, opt =>
                opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}".Trim()))
            .ForMember(dest => dest.UserRole, opt => opt.Ignore())  // We'll set this separately
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
            .ForMember(dest => dest.Time, opt => opt.MapFrom(src => src.Time))
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
            .ForMember(dest => dest.Activity, opt => opt.MapFrom(src => src.Activity))
            .ForMember(dest => dest.Module, opt => opt.MapFrom(src => src.Module))
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details))
            .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Completed"))
            .ForMember(dest => dest.Browser, opt => opt.Ignore())
            .ForMember(dest => dest.OperatingSystem, opt => opt.Ignore())
            .ForMember(dest => dest.Location, opt => opt.Ignore());

        CreateMap<Chairman, ReportResponseDto>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
               // Since Chairman doesn't have Description, we can set a default
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Office))
               .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.FullName))
               .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.TermStart))
               .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
               .ForMember(dest => dest.ChairmanId, opt => opt.MapFrom(src => src.Id));


        CreateMap<LocalGovernment, LGAResponseDto>()
                   .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                   .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                   .ForMember(dest => dest.StateId, opt => opt.MapFrom(src => src.State)) // Assuming State field contains StateId
                   .ForMember(dest => dest.StateName, opt => opt.MapFrom(src => src.State)) // You might need to adjust this if StateName comes from a different property
                   .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.LGA))
                   .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true)) // Set default or map from appropriate property
                   .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => "")) // Map from appropriate property if available
                   .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                   .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => "")) // Map from appropriate property if available
                   .ForMember(dest => dest.LastModifiedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                   // Statistics
                   .ForMember(dest => dest.TotalMarkets, opt => opt.MapFrom(src => src.Markets.Count))
                   .ForMember(dest => dest.ActiveMarkets, opt => opt.MapFrom(src => src.Markets.Count(m => m.IsActive)))
                   .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.Vendors.Count))
                   .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.CurrentRevenue));

        CreateMap<LevyPayment, LevyPaymentResponseDto>();

        CreateMap<ApplicationRole, RoleResponseDto>();
        CreateMap<RolePermission, RolePermissionDto>();

        CreateMap<LevyPayment, LevyPaymentResponseDto>();

        CreateMap<Market, MarketResponseDto>();
        CreateMap<GoodBoy, GoodBoyResponseDto>();
        CreateMap<Trader, TraderResponseDto>();


    }

    private int ConvertPeriodToDays(PaymentPeriodEnum period) => period switch
    {
        PaymentPeriodEnum.Daily => 1,
        PaymentPeriodEnum.Weekly => 7,
        PaymentPeriodEnum.Monthly => 30,
        PaymentPeriodEnum.Yearly => 365,
        _ => 1
    };
}

