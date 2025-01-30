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
using SabiMarket.Domain.Enum;
using TraderDetailsDto = SabiMarket.Application.DTOs.Requests.TraderDetailsDto;

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

        public async Task<BaseResponse<TraderDto>> CreateTrader(CreateTraderDto createDto)
        {
            try
            {
                // Split TraderName into FirstName and LastName
                var nameParts = createDto.TraderName.Trim().Split(' ', 2);
                var firstName = nameParts[0];
                var lastName = nameParts.Length > 1 ? nameParts[1] : "";

                // Check if user with phone number already exists
                var existingUser = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == createDto.PhoneNumber);
                if (existingUser != null)
                {
                    return ResponseFactory.Fail<TraderDto>("Phone number already exists");
                }

                // Create user account
                var user = new ApplicationUser
                {
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = createDto.PhoneNumber,
                    UserName = createDto.PhoneNumber,
                    ProfileImageUrl = createDto.PhotoUrl,
                    Gender = createDto.Gender
                };

                var createUserResult = await _userManager.CreateAsync(user);
                if (!createUserResult.Succeeded)
                {
                    return ResponseFactory.Fail<TraderDto>("Failed to create user account");
                }

                // Create trader profile
                var trader = new Trader
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    MarketId = createDto.MarketId,
                    BusinessType = createDto.Occupancy,
                    PaymentFrequency = createDto.PaymentFrequency,
                    Status = TraderStatusEnum.Active,
                    TIN = $"OSH/LAG/{Guid.NewGuid().ToString().Substring(0, 5)}"
                };

                _repository.TraderRepository.Create(trader);
                await _repository.SaveChangesAsync();

                await _userManager.AddToRoleAsync(user, UserRoles.Trader);

                var traderDto = _mapper.Map<TraderDto>(trader);
                return ResponseFactory.Success(traderDto, "Trader created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trader");
                return ResponseFactory.Fail<TraderDto>(ex);
            }
        }

        public async Task<BaseResponse<TraderDto>> UpdateTrader(string traderId, UpdateTraderDto updateDto)
        {
            try
            {
                var trader = await _repository.TraderRepository
                    .FindByCondition(t => t.Id == traderId, true)
                    .Include(t => t.User)
                    .FirstOrDefaultAsync();

                if (trader == null)
                {
                    return ResponseFactory.Fail<TraderDto>(
                        new NotFoundException("Trader not found"));
                }

                // Check if phone number is being changed and if it's already in use
                if (trader.User.PhoneNumber != updateDto.PhoneNumber)
                {
                    var existingUser = await _userManager.Users
                        .FirstOrDefaultAsync(u =>
                        u.PhoneNumber == updateDto.PhoneNumber && u.Id != trader.UserId);

                    if (existingUser != null)
                    {
                        return ResponseFactory.Fail<TraderDto>("Phone number already exists");
                    }
                }

                // Split TraderName into FirstName and LastName
                var nameParts = updateDto.TraderName.Trim().Split(' ', 2);
                var firstName = nameParts[0];
                var lastName = nameParts.Length > 1 ? nameParts[1] : "";

                // Update user details
                var user = trader.User;
                user.FirstName = firstName;
                user.LastName = lastName;
                user.PhoneNumber = updateDto.PhoneNumber;
                user.UserName = updateDto.PhoneNumber;
                user.ProfileImageUrl = updateDto.PhotoUrl;
                user.Gender = updateDto.Gender;

                var updateUserResult = await _userManager.UpdateAsync(user);
                if (!updateUserResult.Succeeded)
                {
                    return ResponseFactory.Fail<TraderDto>("Failed to update user details");
                }

                // Update trader details
                trader.BusinessType = updateDto.Occupancy;
                trader.MarketId = updateDto.MarketId;
                trader.PaymentFrequency = updateDto.PaymentFrequency;

                _repository.TraderRepository.Update(trader);
                await _repository.SaveChangesAsync();

                var traderDto = _mapper.Map<TraderDto>(trader);
                return ResponseFactory.Success(traderDto, "Trader updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trader");
                return ResponseFactory.Fail<TraderDto>(ex);
            }
        }

        public async Task<BaseResponse<TraderDetailsDto>> GetTraderDetails(string traderId)
        {
            try
            {
                var trader = await _repository.TraderRepository
                    .FindByCondition(t => t.Id == traderId, false)
                    .Include(t => t.User)
                    .Include(t => t.Market)
                    .Include(t => t.LevyPayments)
                    .FirstOrDefaultAsync();

                if (trader == null)
                {
                    return ResponseFactory.Fail<TraderDetailsDto>(
                        new NotFoundException("Trader not found"));
                }

                // Calculate outstanding debt
                var outstandingDebt = await _repository.LevyPaymentRepository
                    .CalculateOutstandingDebt(traderId);

                var traderDetails = new TraderDetailsDto
                {
                    TraderName = $"{trader.User.FirstName} {trader.User.LastName}",
                    PhoneNumber = trader.User.PhoneNumber,
                    Gender = trader.User.Gender,
                    Occupancy = trader.BusinessType,
                    Market = trader.Market.MarketName,
                    DateAdded = trader.CreatedAt,
                    PaymentFrequency = trader.PaymentFrequency, // Just assign the enum directly
                    PaymentAmount = trader.PaymentAmount, // Add this if you want to show the amount separately
                    TraderIdentityNumber = trader.TIN,
                    OutstandingDebt = outstandingDebt,
                    PhotoUrl = trader.User.ProfileImageUrl,
                    QRCode = trader.QRCode
                };

                return ResponseFactory.Success(traderDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trader details");
                return ResponseFactory.Fail<TraderDetailsDto>(ex);
            }
        }
        public async Task<BaseResponse<PaginatorDto<IEnumerable<TraderDto>>>> GetTraders(
        string searchTerm,
        PaginationFilter paginationFilter)
        {
            try
            {
                IQueryable<Trader> query = _repository.TraderRepository.FindAll(false);

                // Apply includes
                query = query.Include(t => t.User)
                            .Include(t => t.Market);

                // Apply search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(t =>
                        (t.User.FirstName + " " + t.User.LastName).Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                        t.User.PhoneNumber.Contains(searchTerm));
                }

                var paginatedTraders = await query.Paginate(paginationFilter);
                var traderDtos = paginatedTraders.PageItems.Select(t => new TraderDto
                {
                    Id = t.Id,
                    TraderName = $"{t.User.FirstName} {t.User.LastName}",
                    Market = t.Market.Name
                });

                var result = new PaginatorDto<IEnumerable<TraderDto>>
                {
                    PageItems = traderDtos,
                    PageSize = paginatedTraders.PageSize,
                    CurrentPage = paginatedTraders.CurrentPage,
                    NumberOfPages = paginatedTraders.NumberOfPages
                };

                return ResponseFactory.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving traders");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<TraderDto>>>(ex);
            }
        }
        public async Task<BaseResponse<bool>> DeleteTrader(string traderId)
        {
            try
            {
                var trader = await _repository.TraderRepository
                    .FindByCondition(t => t.Id == traderId, true)
                    .Include(t => t.User)
                    .FirstOrDefaultAsync();

                if (trader == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Trader not found"));
                }

                // Soft delete by updating status
                trader.Status = TraderStatusEnum.Inactive;
                _repository.TraderRepository.Update(trader);

                await CreateAuditLog(
                    "Trader Deleted",
                    $"Deactivated trader account for {trader.User.Email}",
                    "Trader Management"
                );

                await _repository.SaveChangesAsync();
                return ResponseFactory.Success(true, "Trader deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting trader");
                return ResponseFactory.Fail<bool>(ex);
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