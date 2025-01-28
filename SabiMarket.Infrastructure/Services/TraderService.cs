using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Exceptions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using SabiMarket.Application.DTOs;
using Microsoft.Extensions.Configuration;
using SabiMarket.Infrastructure.Configuration;
using SabiMarket.Domain.Entities;
using SabiMarket.Infrastructure.Utilities;
using SabiMarket.Application.Interfaces;
using SabiMarket.Infrastructure.Helpers;

namespace SabiMarket.Infrastructure.Services
{
    public class TraderService : ITraderService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILogger<TraderService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUser;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IValidator<CreateTraderRequestDto> _createTraderValidator;
        private readonly IValidator<UpdateTraderProfileDto> _updateProfileValidator;
        private readonly IConfiguration _configuration;

        public TraderService(
            IRepositoryManager repository,
            ILogger<TraderService> logger,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUser,
            IHttpContextAccessor httpContextAccessor,
            IValidator<CreateTraderRequestDto> createTraderValidator,
            IValidator<UpdateTraderProfileDto> updateProfileValidator,
            IConfiguration configuration)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _currentUser = currentUser;
            _httpContextAccessor = httpContextAccessor;
            _createTraderValidator = createTraderValidator;
            _updateProfileValidator = updateProfileValidator;
            _configuration = configuration;
        }

        private string GetCurrentIpAddress()
        {
            return _httpContextAccessor.GetRemoteIPAddress();
        }

        private async Task CreateAuditLog(string activity, string details, string module = "Trader Management")
        {
            var userId = _currentUser.GetUserId();
            var auditLog = new AuditLog
            {
                UserId = userId,
                Activity = activity,
                Module = module,
                Details = details,
                IpAddress = GetCurrentIpAddress()
            };
            auditLog.SetDateTime(DateTime.UtcNow);

            _repository.AuditLogRepository.Create(auditLog);
            await _repository.SaveChangesAsync();
        }

        public async Task<BaseResponse<TraderResponseDto>> GetTraderById(string traderId)
        {
            try
            {
                var trader = await _repository.TraderRepository.GetTraderDetails(traderId);
                if (trader == null)
                {
                    await CreateAuditLog(
                        "Trader Lookup Failed",
                        $"Failed to find trader with ID: {traderId}",
                        "Trader Query"
                    );
                    return ResponseFactory.Fail<TraderResponseDto>(new NotFoundException("Trader not found"));
                }

                var traderDto = _mapper.Map<TraderResponseDto>(trader);

                await CreateAuditLog(
                    "Trader Details Retrieved",
                    $"Retrieved trader details for ID: {traderId}",
                    "Trader Query"
                );

                return ResponseFactory.Success(traderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trader");
                return ResponseFactory.Fail<TraderResponseDto>(ex);
            }
        }

        private async Task<PaymentScheduleDto> GetPaymentSchedule(Trader trader, DateTime? lastPaymentDate)
        {
            var marketConfig = _configuration
                .GetSection("PaymentConfiguration:Market:MarketSpecific")
                .Get<List<MarketSpecificConfig>>()?
                .FirstOrDefault(m => m.MarketId == trader.MarketId);

            var paymentConfig = marketConfig?.BusinessTypes?
                .FirstOrDefault(b => b.Type.Equals(trader.BusinessType, StringComparison.OrdinalIgnoreCase));

            if (paymentConfig == null)
            {
                paymentConfig = new BusinessTypeConfig
                {
                    FrequencyInDays = marketConfig?.FrequencyInDays ??
                        _configuration.GetValue<int>("PaymentConfiguration:Market:DefaultFrequencyInDays"),
                    Amount = marketConfig?.Amount ??
                        _configuration.GetValue<decimal>("PaymentConfiguration:Market:DefaultAmount")
                };
            }

            return new PaymentScheduleDto
            {
                Frequency = paymentConfig.FrequencyInDays switch
                {
                    1 => "daily",
                    7 => "weekly",
                    30 => "monthly",
                    _ => $"{paymentConfig.FrequencyInDays} days"
                },
                Amount = paymentConfig.Amount,
                NextDueDate = lastPaymentDate?.AddDays(paymentConfig.FrequencyInDays) ?? DateTime.UtcNow
            };
        }

        public async Task<BaseResponse<TraderDashboardDto>> GetDashboardStats(string traderId)
        {
            try
            {
                var trader = await _repository.TraderRepository.GetTraderDetails(traderId);
                if (trader == null)
                {
                    await CreateAuditLog(
                        "Dashboard Stats Failed",
                        $"Failed to find trader with ID: {traderId}",
                        "Trader Dashboard"
                    );
                    return ResponseFactory.Fail<TraderDashboardDto>(new NotFoundException("Trader not found"));
                }

                var lastPayment = trader.LevyPayments
                    .OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefault();

                var paymentSchedule = await GetPaymentSchedule(trader, lastPayment?.PaymentDate);

                var stats = new TraderDashboardDto
                {
                    NextPaymentDate = paymentSchedule.NextDueDate,
                    TotalLeviesPaid = trader.LevyPayments.Sum(p => p.Amount),
                    PaymentHistory = trader.LevyPayments
                        .OrderByDescending(p => p.PaymentDate)
                        .Take(5)
                        .Select(p => new PaymentHistoryDto
                        {
                            PaymentType = (int)p.PaymentMethod,
                            PaymentDate = p.PaymentDate,
                            Amount = p.Amount
                        })
                        .ToList(),
                    PaymentSchedule = paymentSchedule
                };

                await CreateAuditLog(
                    "Dashboard Stats Retrieved",
                    $"Retrieved dashboard stats for trader {traderId}. Payment Schedule: {paymentSchedule.Frequency} - ₦{paymentSchedule.Amount}",
                    "Trader Dashboard"
                );

                return ResponseFactory.Success(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trader dashboard stats");
                return ResponseFactory.Fail<TraderDashboardDto>(ex);
            }
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>> GetTraders(
            TraderFilterRequestDto filterDto, PaginationFilter paginationFilter)
        {
            try
            {
                var paginatedTraders = await _repository.TraderRepository
                    .GetTradersByMarketAsync(filterDto.MarketId, paginationFilter);

                var traderDtos = _mapper.Map<IEnumerable<TraderResponseDto>>(paginatedTraders.PageItems);

                var result = new PaginatorDto<IEnumerable<TraderResponseDto>>
                {
                    PageItems = traderDtos,
                    PageSize = paginatedTraders.PageSize,
                    CurrentPage = paginatedTraders.CurrentPage,
                    NumberOfPages = paginatedTraders.NumberOfPages
                };

                await CreateAuditLog(
                    "Traders List Retrieved",
                    $"Retrieved traders list - Page {paginationFilter.PageNumber}, Market: {filterDto.MarketId}, Business Type: {filterDto.BusinessType}",
                    "Trader Query"
                );

                return ResponseFactory.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving traders");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<TraderResponseDto>>>(ex);
            }
        }

        public async Task<BaseResponse<TraderResponseDto>> CreateTrader(CreateTraderRequestDto traderDto)
        {
            try
            {
                var validationResult = await _createTraderValidator.ValidateAsync(traderDto);
                if (!validationResult.IsValid)
                {
                    await CreateAuditLog(
                        "Trader Creation Failed",
                        $"Validation failed for new trader creation with email: {traderDto.Email}",
                        "Trader Creation"
                    );
                    return ResponseFactory.Fail<TraderResponseDto>(new FluentValidation.ValidationException(validationResult.Errors));
                }

                var existingUser = await _userManager.FindByEmailAsync(traderDto.Email);
                if (existingUser != null)
                {
                    await CreateAuditLog(
                        "Trader Creation Failed",
                        $"Email already exists: {traderDto.Email}",
                        "Trader Creation"
                    );
                    return ResponseFactory.Fail<TraderResponseDto>("Email already exists");
                }

                var user = _mapper.Map<ApplicationUser>(traderDto);
                var createUserResult = await _userManager.CreateAsync(user);
                if (!createUserResult.Succeeded)
                {
                    await CreateAuditLog(
                        "Trader Creation Failed",
                        $"Failed to create user account for: {traderDto.Email}",
                        "Trader Creation"
                    );
                    return ResponseFactory.Fail<TraderResponseDto>("Failed to create user");
                }

                var trader = new Trader
                {
                    UserId = user.Id,
                    MarketId = traderDto.MarketId,
                    CaretakerId = traderDto.CaretakerId,
                    BusinessName = traderDto.BusinessName,
                    BusinessType = traderDto.BusinessType,
                    TIN = $"OSH/LAG/{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
                };

                _repository.TraderRepository.AddTrader(trader);
                await _repository.SaveChangesAsync();
                await _userManager.AddToRoleAsync(user, UserRoles.Trader);

                await CreateAuditLog(
                    "Created Trader Account",
                    $"Created trader account for {user.Email} ({user.FirstName} {user.LastName})",
                    "Trader Creation"
                );

                var response = _mapper.Map<TraderResponseDto>(trader);
                return ResponseFactory.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trader");
                return ResponseFactory.Fail<TraderResponseDto>(ex);
            }
        }

        public async Task<BaseResponse<bool>> UpdateTraderProfile(string traderId, UpdateTraderProfileDto profileDto)
        {
            try
            {
                var validationResult = await _updateProfileValidator.ValidateAsync(profileDto);
                if (!validationResult.IsValid)
                {
                    await CreateAuditLog(
                        "Profile Update Failed",
                        $"Validation failed for trader profile update ID: {traderId}",
                        "Profile Management"
                    );
                    return ResponseFactory.Fail<bool>(new FluentValidation.ValidationException(validationResult.Errors));
                }

                var trader = await _repository.TraderRepository.GetTraderById(traderId, true);
                if (trader == null)
                {
                    await CreateAuditLog(
                        "Profile Update Failed",
                        $"Trader not found for ID: {traderId}",
                        "Profile Management"
                    );
                    return ResponseFactory.Fail<bool>(new NotFoundException("Trader not found"));
                }

                var user = await _userManager.FindByIdAsync(trader.UserId);
                if (user == null)
                {
                    await CreateAuditLog(
                        "Profile Update Failed",
                        $"User not found for trader ID: {traderId}",
                        "Profile Management"
                    );
                    return ResponseFactory.Fail<bool>(new NotFoundException("User not found"));
                }

                // Track changes for audit log
                var changes = new List<string>();
                if (trader.BusinessName != profileDto.BusinessName)
                    changes.Add($"Business Name: {trader.BusinessName} → {profileDto.BusinessName}");
                if (trader.SectionId != profileDto.SectionId)
                    changes.Add($"Section ID: {trader.SectionId} → {profileDto.SectionId}");
                if (user.PhoneNumber != profileDto.PhoneNumber)
                    changes.Add($"Phone: {user.PhoneNumber} → {profileDto.PhoneNumber}");

                trader.BusinessName = profileDto.BusinessName;
                trader.SectionId = profileDto.SectionId;
                user.PhoneNumber = profileDto.PhoneNumber;

                _repository.TraderRepository.UpdateTrader(trader);
                await _userManager.UpdateAsync(user);
                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Updated Trader Profile",
                    $"Updated profile for {user.Email}. Changes: {string.Join(", ", changes)}",
                    "Profile Management"
                );

                return ResponseFactory.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trader profile");
                return ResponseFactory.Fail<bool>(ex);
            }
        }
    }
}