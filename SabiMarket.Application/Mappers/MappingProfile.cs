﻿using AutoMapper;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
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
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.Name))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.LastLoginAt))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

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

        // Main RoleResponseDto mapping
        CreateMap<ApplicationRole, RoleResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.AllPermissions, opt => opt.MapFrom(src =>
                src.Permissions != null
                    ? src.Permissions
                        .Where(p => p.IsGranted)
                        .Select(p => p.Name)
                        .ToList()
                    : new List<string>()))
            .ForMember(dest => dest.VisiblePermissions, opt => opt.MapFrom(src =>
                src.Permissions != null
                    ? src.Permissions
                        .Where(p => p.IsGranted)
                        .Select(p => p.Name)
                        .Take(RolePermissionConstants.VisiblePermissionsCount)
                        .ToList()
                    : new List<string>()))
            .ForMember(dest => dest.AdditionalPermissionsCount, opt => opt.MapFrom(src =>
                src.Permissions != null
                    ? Math.Max(0, src.Permissions.Count(p => p.IsGranted) - RolePermissionConstants.VisiblePermissionsCount)
                    : 0))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.LastModifiedAt, opt => opt.MapFrom(src => src.LastModifiedAt))
            .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.LastModifiedBy));

        CreateMap<ApplicationRole, RoleResponseDto>()
                .AfterMap((src, dest) =>
                {
                    // Explicitly populate AllPermissions
                    dest.AllPermissions = src.Permissions?
                        .Where(p => p.IsGranted)
                        .Select(p => p.Name)
                        .ToList() ?? new List<string>();

                    // VisiblePermissions and AdditionalPermissionsCount will be computed automatically
                    // since they are get-only properties based on AllPermissions
                });

        // For debugging purposes, you can add this test mapping
        CreateMap<RolePermission, string>()
            .ConvertUsing(rp => rp.Name);
        // RolePermissionDto mapping
        CreateMap<RolePermission, RoleResponseDto.RolePermissionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.IsGranted, opt => opt.MapFrom(src => src.IsGranted));

        // AdminRoleResponseDto mapping
        CreateMap<ApplicationRole, RoleResponseDto.AdminRoleResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src =>
                src.Permissions != null
                    ? src.Permissions
                        .Where(p => p.IsGranted)
                        .Select(p => p.Name)
                        .ToList()
                    : new List<string>()));

        /* // Main RoleResponseDto mapping
         CreateMap<ApplicationRole, RoleResponseDto>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.AllPermissions, opt => opt.MapFrom(src =>
                 src.Permissions
                     .Where(p => p.IsGranted)
                     .Select(p => p.Name)
                     .ToList()))
             .ForMember(dest => dest.VisiblePermissions, opt => opt.Ignore()) // Computed property
             .ForMember(dest => dest.AdditionalPermissionsCount, opt => opt.Ignore()) // Computed property
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
             .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
             .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
             .ForMember(dest => dest.LastModifiedAt, opt => opt.MapFrom(src => src.LastModifiedAt))
             .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.LastModifiedBy));

         // RolePermissionDto mapping
         CreateMap<RolePermission, RoleResponseDto.RolePermissionDto>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.IsGranted, opt => opt.MapFrom(src => src.IsGranted));

         // AdminRoleResponseDto mapping
         CreateMap<ApplicationRole, RoleResponseDto.AdminRoleResponseDto>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
             .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src =>
                 src.Permissions
                     .Where(p => p.IsGranted)
                     .Select(p => p.Name)
                     .ToList()));

         // Reverse mappings for create/update scenarios
         CreateMap<CreateRoleRequestDto, ApplicationRole>()
             .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.Permissions, opt => opt.Ignore()) // Handle in service layer
             .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

         CreateMap<UpdateRoleRequestDto, ApplicationRole>()
             .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.Permissions, opt => opt.Ignore()) // Handle in service layer
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
             .ForMember(dest => dest.LastModifiedAt, opt => opt.MapFrom(src => DateTime.UtcNow));*/

        /*
                CreateMap<ApplicationRole, RoleResponseDto>()
                      .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                      .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                      .ForMember(dest => dest.AllPermissions, opt => opt.MapFrom(src =>
                          src.Permissions.Where(p => p.IsGranted).Select(p => p.Name).ToList()))
                      .ForMember(dest => dest.VisiblePermissions, opt => opt.Ignore()) // This is computed
                      .ForMember(dest => dest.AdditionalPermissionsCount, opt => opt.Ignore()) // This is computed
                      .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                      .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                      .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                      .ForMember(dest => dest.LastModifiedAt, opt => opt.MapFrom(src => src.LastModifiedAt))
                      .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.LastModifiedBy));
        */

        CreateMap<UpdateGoodBoyProfileDto, ApplicationUser>();
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

