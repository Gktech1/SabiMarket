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
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.MarketName))
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
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
    src.User != null
        ? string.IsNullOrEmpty(src.FullName) ? $"{src.User.FirstName} {src.User.LastName}" : src.FullName
                    : string.Empty))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User != null ? src.User.PhoneNumber : string.Empty))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.User != null && src.User.IsActive))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email ?? string.Empty))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber ?? string.Empty))
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.MarketName))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.User.IsActive))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.LocalGovernmentId, opt => opt.MapFrom(src => src.LocalGovernmentId))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.DefaultPassword, opt => opt.Ignore());

        CreateMap<Chairman, ChairmanResponseDto>();
       
        CreateMap<Report, ReportExportDto>()
               .ForMember(dest => dest.TotalMarkets, opt => opt.MapFrom(src => src.MarketCount))
               .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.TotalRevenueGenerated))
               .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.TotalTraders))
               .ForMember(dest => dest.TotalCaretakers, opt => opt.MapFrom(src => src.TotalCaretakers))
               .ForMember(dest => dest.TraderComplianceRate, opt => opt.MapFrom(src => src.ComplianceRate))
               .ForMember(dest => dest.TotalTransactions, opt => opt.MapFrom(src => src.PaymentTransactions))
               .ForMember(dest => dest.MarketDetails, opt => opt.Ignore())
               .ForMember(dest => dest.RevenueByPaymentMethod, opt => opt.Ignore())
               .ForMember(dest => dest.TransactionsByMarket, opt => opt.Ignore())
               .ForMember(dest => dest.RevenueByMonth, opt => opt.Ignore())
               .ForMember(dest => dest.LGADetails, opt => opt.Ignore())
               .ForMember(dest => dest.ComplianceByMarket, opt => opt.Ignore())
               .AfterMap((src, dest, context) =>
               {
                   // For single-report mapping, we need to manually create the collections
                   // This will be overridden when mapping collections with custom methods
                   if (dest.MarketDetails == null)
                   {
                       dest.MarketDetails = new List<ReportExportDto.MarketSummary>();

                       // Only add market details if we have market data
                       if (!string.IsNullOrEmpty(src.MarketId))
                       {
                           dest.MarketDetails.Add(new ReportExportDto.MarketSummary
                           {
                               MarketId = src.MarketId,
                               MarketName = src.MarketName,
                               Location = src.Market?.Location,
                               LGAName = src.Market?.LocalGovernment?.Name,
                               TotalTraders = src.TotalTraders,
                               Revenue = src.TotalLevyCollected,
                               ComplianceRate = src.ComplianceRate,
                               TransactionCount = src.PaymentTransactions
                           });
                       }
                   }
               });
        // Ensure collection mapping
        CreateMap<IEnumerable<Chairman>, IEnumerable<ChairmanResponseDto>>().ConvertUsing((src, dest, context) =>
            src.Select(chairman => context.Mapper.Map<ChairmanResponseDto>(chairman)));
        CreateMap<IEnumerable<Report>, ReportExportDto>()
                       .ForMember(dest => dest.MarketDetails, opt => opt.MapFrom((src, _, __, context) => MapMarketDetails(src)))
                       .ForMember(dest => dest.RevenueByMonth, opt => opt.MapFrom((src, _, __, context) => MapRevenueByMonth(src)))
                       .ForMember(dest => dest.ComplianceByMarket, opt => opt.MapFrom((src, _, __, context) => MapComplianceByMarket(src)))
                       .ForMember(dest => dest.TotalMarkets, opt => opt.MapFrom(src => src.Select(r => r.MarketId).Distinct().Count()))
                       .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.Sum(r => r.TotalLevyCollected)))
                       .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.Sum(r => r.TotalTraders)))
                       .ForMember(dest => dest.TotalTransactions, opt => opt.MapFrom(src => src.Sum(r => r.PaymentTransactions)))
                       .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Min(r => r.StartDate)))
                       .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Max(r => r.EndDate)))
                       .ForMember(dest => dest.TraderComplianceRate, opt => opt.MapFrom(src =>
                           src.Sum(r => r.TotalTraders) > 0
                               ? (decimal)src.Sum(r => r.CompliantTraders) / src.Sum(r => r.TotalTraders) * 100
                               : 0))
                       .ForMember(dest => dest.DailyAverageRevenue, opt => opt.MapFrom((src, _, __, context) =>
                       {
                           var startDate = src.Min(r => r.StartDate);
                           var endDate = src.Max(r => r.EndDate);
                           var totalDays = (endDate - startDate).Days + 1;
                           var totalRevenue = src.Sum(r => r.TotalLevyCollected);
                           return totalDays > 0 ? totalRevenue / totalDays : 0;
                       }));
        CreateMap<LevySetupRequestDto, LevyPayment>()
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.PaymentFrequencyDays))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(_ => PaymentStatusEnum.Pending)) // Defaulting to Pending
            .ForMember(dest => dest.PaymentMethod, opt => opt.Ignore()) // Assuming it's set later
            .ForMember(dest => dest.TraderId, opt => opt.Ignore()) // Assuming it comes from context
            .ForMember(dest => dest.GoodBoyId, opt => opt.Ignore()) // Assuming it comes from context
            .ForMember(dest => dest.ChairmanId, opt => opt.Ignore()) // Assuming it comes from context
            .ForMember(dest => dest.TransactionReference, opt => opt.Ignore()) // Assuming it's set elsewhere
            .ForMember(dest => dest.PaymentDate, opt => opt.Ignore()) // Set on processing
            .ForMember(dest => dest.CollectionDate, opt => opt.Ignore()) // Set on processing
            .ForMember(dest => dest.Notes, opt => opt.Ignore()) // Optional
            .ForMember(dest => dest.QRCodeScanned, opt => opt.Ignore()) // Optional
            .ForMember(dest => dest.HasIncentive, opt => opt.Ignore()) // Optional
            .ForMember(dest => dest.IncentiveAmount, opt => opt.Ignore()); // Optional

        CreateMap<Admin, AdminDashboardStatsDto>()
           .ForMember(dest => dest.RegisteredLGAs, opt => opt.MapFrom(src => src.RegisteredLGAs))
           .ForMember(dest => dest.ActiveChairmen, opt => opt.MapFrom(src => src.ActiveChairmen))
           .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.TotalRevenue));


        // Ensure Market mapping exists
        CreateMap<Market, MarketResponseDto>();

        // Ensure GoodBoy and Trader mappings exist
        CreateMap<GoodBoy, GoodBoyResponseDto>();
        CreateMap<Trader, TraderResponseDto>();

        /*CreateMap<Market, MarketResponseDto>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MarketName))
           .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
           .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
           .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.TotalTraders))
           .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.MarketCapacity))
           .ForMember(dest => dest.CaretakerId, opt => opt.MapFrom(src => src.CaretakerId))
           .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
           .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));*/


        CreateMap<AssistCenterOfficer, AssistantOfficerResponseDto>()
              .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}".Trim()))
              .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
              .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
              .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
              .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.MarketName));

        CreateMap<CreateAssistantOfficerRequestDto, AssistCenterOfficer>()
         .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
         // No need to map UserLevel as it's not in the new DTO
         .ForMember(dest => dest.User, opt => opt.Ignore()) // Handle User creation separately
         .ForMember(dest => dest.Market, opt => opt.Ignore())
         .ForMember(dest => dest.Chairman, opt => opt.Ignore())
         .ForMember(dest => dest.LocalGovernment, opt => opt.Ignore())
         .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Set manually
         .ForMember(dest => dest.ChairmanId, opt => opt.Ignore()) // Set manually
         .ForMember(dest => dest.LocalGovernmentId, opt => opt.Ignore()) // Set manually
         .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(src => false))
         .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
         .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
         .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<AssistCenterOfficer, AssistantOfficerResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : ""))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src =>
                    src.User != null ? src.User.Email : ""))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src =>
                    src.User != null ? src.User.PhoneNumber : ""))
                .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
                .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src =>
                    src.Market != null ? src.Market.MarketName : ""))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src =>
                    src.User != null ? src.User.Gender : ""))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.DefaultPassword, opt => opt.Ignore()); // Set manually in service

        // If you need to map from AssistCenterOfficer to CreateAssistantOfficerRequestDto
        CreateMap<AssistCenterOfficer, AssistantOfficerResponseDto>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                   src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : ""))
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src =>
                   src.User != null ? src.User.Email : ""))
               .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src =>
                   src.User != null ? src.User.PhoneNumber : ""))
               .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
               .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src =>
                   src.Market != null ? src.Market.MarketName : ""))
               .ForMember(dest => dest.Gender, opt => opt.MapFrom(src =>
                   src.User != null ? src.User.Gender : ""))
               .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
               .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
               .ForMember(dest => dest.DefaultPassword, opt => opt.Ignore()); // This is set manually after mapping

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
          .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.MarketName));

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
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
              .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.MarketName))
              .ForMember(dest => dest.MarketType, opt => opt.MapFrom(src => src.MarketType))
              .ForMember(dest => dest.CaretakerId, opt => opt.MapFrom(src => src.CaretakerId))
              .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
              .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => 0))
              .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => 0))
              .ForMember(dest => dest.PaymentTransactions, opt => opt.MapFrom(src => 0))
              .ForMember(dest => dest.ComplianceRate, opt => opt.MapFrom(src => 0))
              .ForMember(dest => dest.CompliantTraders, opt => opt.MapFrom(src => 0))
              .ForMember(dest => dest.NonCompliantTraders, opt => opt.MapFrom(src => 0))
              .ForMember(dest => dest.OccupancyRate, opt => opt.MapFrom(src => 0));

        // UpdateMarketRequestDto -> Market
        CreateMap<UpdateMarketRequestDto, Market>()
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.MarketName))
            .ForMember(dest => dest.MarketType, opt => opt.MapFrom(src => src.MarketType))
            .ForMember(dest => dest.CaretakerId, opt => opt.MapFrom(src => src.CaretakerId))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Market -> MarketResponseDto
        /*CreateMap<Market, MarketResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MarketName))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.TotalTraders))
            .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
            .ForMember(dest => dest.ContactPhone, opt => opt.Ignore()) // Adjust if needed
            .ForMember(dest => dest.ContactEmail, opt => opt.Ignore()) // Adjust if needed
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt))
            .ForMember(dest => dest.CaretakerId, opt => opt.MapFrom(src => src.Caretaker != null ? src.Caretaker.Id : null)); // Map single caretaker ID*/

        CreateMap<Market, MarketResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.MarketName ?? "Unnamed Market"))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location ?? "No Location"))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? "No description available"))
            .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.TotalTraders))
            .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
            .ForMember(dest => dest.ContactPhone, opt => opt.Ignore())
            .ForMember(dest => dest.ContactEmail, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt))
            .ForMember(dest => dest.CaretakerId, opt => opt.MapFrom(src => src.CaretakerId));

        CreateMap<Market, MarketRevenueDto>()
           .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.MarketName))
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
           .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.MarketName))
           .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
           .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));


        /* CreateMap<Market, MarketResponseDto>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MarketName))
           .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
           .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
           .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.TotalTraders))
           .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
           .ForMember(dest => dest.ContactPhone, opt => opt.Ignore()) // Adjust if needed
           .ForMember(dest => dest.ContactEmail, opt => opt.Ignore()) // Adjust if needed
           .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
           .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt))
           .ForMember(dest => dest.CaretakerId, opt => opt.MapFrom(src => src.CaretakerId)); // Single caretaker ID mapping*/

        CreateMap<Caretaker, CaretakerResponseDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.User.IsActive))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.Market, opt => opt.MapFrom(src => src.Markets.FirstOrDefault()))  // Assuming one Market is related, adjust as needed
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(src => src.IsBlocked))
            .ForMember(dest => dest.DefaultPassword, opt => opt.Ignore()); // DefaultPassword should be handled separately if needed.

       /* CreateMap<Caretaker, CaretakerResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.MarketId, opt => opt.Ignore()) // Ignored because multiple markets exist
            .ForMember(dest => dest.Market, opt => opt.MapFrom(src => src.Markets.FirstOrDefault())) // Maps the first market
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsBlocked));
*/

        // CreateMarketRequestDto to Market
        CreateMap<CreateMarketRequestDto, Market>()
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.MarketName))
            .ForMember(dest => dest.MarketType, opt => opt.MapFrom(src => src.MarketType.ToString()))
            .ForMember(dest => dest.CaretakerId, opt => opt.MapFrom(src => src.CaretakerId))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            // Set default values for required fields not in DTO
            .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.MarketCapacity, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.OccupancyRate, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.ComplianceRate, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.CompliantTraders, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.NonCompliantTraders, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.PaymentTransactions, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => 0))
            // Ignore navigation properties and other fields that should be set separately
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Traders, opt => opt.Ignore())
            .ForMember(dest => dest.MarketSections, opt => opt.Ignore())
            .ForMember(dest => dest.Chairman, opt => opt.Ignore())
            .ForMember(dest => dest.LocalGovernment, opt => opt.Ignore())
            .ForMember(dest => dest.Caretaker, opt => opt.Ignore());

        // UpdateMarketRequestDto to Market
        CreateMap<UpdateMarketRequestDto, Market>()
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.MarketName))
            .ForMember(dest => dest.MarketType, opt => opt.MapFrom(src => src.MarketType.ToString()))
            .ForMember(dest => dest.CaretakerId, opt => opt.MapFrom(src => src.CaretakerId))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            // Ignore fields that shouldn't be updated through this DTO
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.TotalTraders, opt => opt.Ignore())
            .ForMember(dest => dest.MarketCapacity, opt => opt.Ignore())
            .ForMember(dest => dest.OccupancyRate, opt => opt.Ignore())
            .ForMember(dest => dest.ComplianceRate, opt => opt.Ignore())
            .ForMember(dest => dest.CompliantTraders, opt => opt.Ignore())
            .ForMember(dest => dest.NonCompliantTraders, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentTransactions, opt => opt.Ignore())
            .ForMember(dest => dest.TotalRevenue, opt => opt.Ignore())
            .ForMember(dest => dest.Traders, opt => opt.Ignore())
            .ForMember(dest => dest.MarketSections, opt => opt.Ignore())
            .ForMember(dest => dest.Chairman, opt => opt.Ignore())
            .ForMember(dest => dest.LocalGovernment, opt => opt.Ignore())
            .ForMember(dest => dest.Caretaker, opt => opt.Ignore());

        CreateMap<GoodBoy, GoodBoyResponseDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.MarketName))
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

        CreateMap<Market, MarketDetailsDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MarketName))
            .ForMember(dest => dest.Caretakers, opt => opt.MapFrom(src => src.Caretaker != null ?
                new List<Caretaker> { src.Caretaker } : new List<Caretaker>()))
            .ForMember(dest => dest.Traders, opt => opt.MapFrom(src => src.Traders));

        CreateMap<GoodBoy, GoodBoyResponseDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.MarketName))
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

        /*CreateMap<Market, MarketDetailsDto>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MarketName))
           .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
           .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
           .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.TotalTraders))
           .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
           .ForMember(dest => dest.ContactPhone, opt => opt.Ignore()) // No matching property in source
           .ForMember(dest => dest.ContactEmail, opt => opt.Ignore()) // No matching property in source
           .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
           .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
           .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.TotalRevenue))
           .ForMember(dest => dest.ComplianceRate, opt => opt.MapFrom(src => src.ComplianceRate));*/

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

        // CaretakerForCreationRequestDto -> Caretaker
        CreateMap<CaretakerForCreationRequestDto, Caretaker>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => new ApplicationUser
            {
                UserName = src.EmailAddress,
                Email = src.EmailAddress,
                PhoneNumber = src.PhoneNumber,
                EmailConfirmed = true
            }))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Will be set after user creation
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Markets, opt => opt.Ignore())
            .ForMember(dest => dest.GoodBoys, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedTraders, opt => opt.Ignore())
            .ForMember(dest => dest.Chairman, opt => opt.Ignore())
            .ForMember(dest => dest.LocalGovernment, opt => opt.Ignore());

        // Caretaker -> CaretakerResponseDto
        /* CreateMap<Caretaker, CaretakerResponseDto>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
             .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.UserName))
             .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.User.Email))
             .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
             .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Markets.FirstOrDefault().MarketName))
             .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(src => src.IsBlocked))
             .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
             .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
 */

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


    private List<ReportExportDto.MarketSummary> MapMarketDetails(IEnumerable<Report> reports)
    {
        var marketDetails = new List<ReportExportDto.MarketSummary>();

        // Group by market to get aggregated stats
        var marketGroups = reports.Where(r => !string.IsNullOrEmpty(r.MarketId))
                                 .GroupBy(r => r.MarketId);

        foreach (var marketGroup in marketGroups)
        {
            var firstReport = marketGroup.First();

            marketDetails.Add(new ReportExportDto.MarketSummary
            {
                MarketId = firstReport.MarketId,
                MarketName = firstReport.MarketName,
                Location = firstReport.Market?.Location,
                LGAName = firstReport.Market?.LocalGovernment?.Name,
                TotalTraders = marketGroup.Sum(r => r.TotalTraders),
                Revenue = marketGroup.Sum(r => r.TotalLevyCollected),
                ComplianceRate = marketGroup.Any(r => r.TotalTraders > 0)
                    ? marketGroup.Sum(r => r.CompliantTraders) / (decimal)marketGroup.Sum(r => r.TotalTraders) * 100
                    : 0,
                TransactionCount = marketGroup.Sum(r => r.PaymentTransactions)
            });
        }

        return marketDetails;
    }

    private List<ReportExportDto.MarketMonthlyRevenue> MapRevenueByMonth(IEnumerable<Report> reports)
    {
        var revenueByMonth = new List<ReportExportDto.MarketMonthlyRevenue>();

        // Group by market
        var marketGroups = reports.Where(r => !string.IsNullOrEmpty(r.MarketId))
                                 .GroupBy(r => r.MarketId);

        foreach (var marketGroup in marketGroups)
        {
            var firstReport = marketGroup.First();
            var monthlyData = new List<ReportExportDto.MonthlyData>();

            // Group by year and month
            var monthGroups = marketGroup.GroupBy(r => new { r.Year, r.Month });

            foreach (var monthGroup in monthGroups)
            {
                var monthName = new DateTime(monthGroup.Key.Year, monthGroup.Key.Month, 1)
                                    .ToString("MMM");

                monthlyData.Add(new ReportExportDto.MonthlyData
                {
                    Month = monthName,
                    MonthNumber = monthGroup.Key.Month,
                    Year = monthGroup.Key.Year,
                    Revenue = monthGroup.Sum(r => r.MonthlyRevenue),
                    TransactionCount = monthGroup.Sum(r => r.PaymentTransactions)
                });
            }

            revenueByMonth.Add(new ReportExportDto.MarketMonthlyRevenue
            {
                MarketId = firstReport.MarketId,
                MarketName = firstReport.MarketName,
                MonthlyData = monthlyData.OrderBy(m => m.Year).ThenBy(m => m.MonthNumber).ToList()
            });
        }

        return revenueByMonth;
    }

    private List<ReportExportDto.MarketCompliance> MapComplianceByMarket(IEnumerable<Report> reports)
    {
        var complianceByMarket = new List<ReportExportDto.MarketCompliance>();

        // Group by market
        var marketGroups = reports.Where(r => !string.IsNullOrEmpty(r.MarketId))
                                 .GroupBy(r => r.MarketId);

        foreach (var marketGroup in marketGroups)
        {
            var firstReport = marketGroup.First();
            var totalTraders = marketGroup.Sum(r => r.TotalTraders);
            var compliantTraders = marketGroup.Sum(r => r.CompliantTraders);

            complianceByMarket.Add(new ReportExportDto.MarketCompliance
            {
                MarketId = firstReport.MarketId,
                MarketName = firstReport.MarketName,
                CompliancePercentage = totalTraders > 0 ? (decimal)compliantTraders / totalTraders * 100 : 0,
                CompliantTraders = compliantTraders,
                NonCompliantTraders = totalTraders - compliantTraders
            });
        }

        return complianceByMarket;
    }
}


