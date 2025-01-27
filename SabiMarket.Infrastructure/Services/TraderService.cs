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
using SabiMarket.Domain.Entities;
using SabiMarket.Infrastructure.Helpers;
using SabiMarket.Infrastructure.Utilities;

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

        public TraderService(
            IRepositoryManager repository,
            ILogger<TraderService> logger,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUser,
            IHttpContextAccessor httpContextAccessor,
            IValidator<CreateTraderRequestDto> createTraderValidator,
            IValidator<UpdateTraderProfileDto> updateProfileValidator)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _currentUser = currentUser;
            _httpContextAccessor = httpContextAccessor;
            _createTraderValidator = createTraderValidator;
            _updateProfileValidator = updateProfileValidator;
        }

        private string GetCurrentIpAddress() => _httpContextAccessor.GetRemoteIPAddress();

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

        public async Task<BaseResponse<TraderDashboardDto>> GetDashboardStats(string traderId)
        {
            try
            {
                var trader = await _repository.TraderRepository
                    .FindByCondition(t => t.Id == traderId, false)
                    .Include(t => t.LevyPayments)
                    .FirstOrDefaultAsync();

                if (trader == null)
                    return ResponseFactory.Fail<TraderDashboardDto>(new NotFoundException("Trader not found"));

                var lastPayment = trader.LevyPayments
                    .OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefault();

                var stats = new TraderDashboardDto
                {
                    NextPaymentDate = lastPayment?.PaymentDate.AddDays(2) ?? DateTime.UtcNow,
                    TotalLeviesPaid = trader.LevyPayments.Sum(p => p.Amount),
                    RecentPayments = _mapper.Map<List<LevyPaymentDto>>(
                        trader.LevyPayments.OrderByDescending(p => p.PaymentDate).Take(5))
                };

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
                var query = _repository.TraderRepository.FindAll(false);

                if (!string.IsNullOrEmpty(filterDto.MarketId))
                    query = query.Where(t => t.MarketId == filterDto.MarketId);

                if (!string.IsNullOrEmpty(filterDto.CaretakerId))
                    query = query.Where(t => t.CaretakerId == filterDto.CaretakerId);

                if (!string.IsNullOrEmpty(filterDto.BusinessType))
                    query = query.Where(t => t.BusinessType == filterDto.BusinessType);

                var paginatedTraders = await query.Paginate(paginationFilter);
                var traderDtos = _mapper.Map<IEnumerable<TraderResponseDto>>(paginatedTraders.PageItems);

                var result = new PaginatorDto<IEnumerable<TraderResponseDto>>
                {
                    PageItems = traderDtos,
                    PageSize = paginatedTraders.PageSize,
                    CurrentPage = paginatedTraders.CurrentPage,
                    NumberOfPages = paginatedTraders.NumberOfPages
                };

                await CreateAuditLog(
                    "Trader List Query",
                    $"Retrieved traders list - Page {paginationFilter.PageNumber}, Filters: {JsonSerializer.Serialize(filterDto)}"
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
                    return ResponseFactory.Fail<TraderResponseDto>(new FluentValidation.ValidationException(validationResult.Errors));

                var existingUser = await _userManager.FindByEmailAsync(traderDto.Email);
                if (existingUser != null)
                    return ResponseFactory.Fail<TraderResponseDto>("Email already exists");

                var user = _mapper.Map<ApplicationUser>(traderDto);
                var createUserResult = await _userManager.CreateAsync(user);
                if (!createUserResult.Succeeded)
                    return ResponseFactory.Fail<TraderResponseDto>("Failed to create user");

                var trader = new Trader
                {
                    Id = Guid.NewGuid().ToString(),    
                    UserId = user.Id,
                    MarketId = traderDto.MarketId,
                    CaretakerId = traderDto.CaretakerId,
                    BusinessName = traderDto.BusinessName,
                    BusinessType = traderDto.BusinessType,
                    TIN = GenerateTIN()
                };

                _repository.TraderRepository.Create(trader);
                await _repository.SaveChangesAsync();
                await _userManager.AddToRoleAsync(user, UserRoles.Trader);

                await CreateAuditLog(
                    "Created Trader Account",
                    $"Created trader account for {user.Email} ({user.FirstName} {user.LastName})"
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

        private string GenerateTIN()
        {
            return $"OSH/LAG/{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        public async Task<BaseResponse<bool>> UpdateTraderProfile(string traderId, UpdateTraderProfileDto profileDto)
        {
            try
            {
                var validationResult = await _updateProfileValidator.ValidateAsync(profileDto);
                if (!validationResult.IsValid)
                    return ResponseFactory.Fail<bool>(new FluentValidation.ValidationException(validationResult.Errors));

                var trader = await _repository.TraderRepository
                    .FindByCondition(t => t.Id == traderId, true)
                    .FirstOrDefaultAsync();

                if (trader == null)
                    return ResponseFactory.Fail<bool>(new NotFoundException("Trader not found"));

                var user = await _userManager.FindByIdAsync(trader.UserId);
                if (user == null)
                    return ResponseFactory.Fail<bool>(new NotFoundException("User not found"));

                var changes = new List<string>();
                if (trader.BusinessName != profileDto.BusinessName)
                {
                    changes.Add($"Business Name: {trader.BusinessName} → {profileDto.BusinessName}");
                    trader.BusinessName = profileDto.BusinessName;
                }

                if (user.PhoneNumber != profileDto.PhoneNumber)
                {
                    changes.Add($"Phone: {user.PhoneNumber} → {profileDto.PhoneNumber}");
                    user.PhoneNumber = profileDto.PhoneNumber;
                }

                var updateUserResult = await _userManager.UpdateAsync(user);
                if (!updateUserResult.Succeeded)
                    return ResponseFactory.Fail<bool>("Failed to update user");

                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Updated Trader Profile",
                    $"Updated profile for {user.Email}. Changes: {string.Join(", ", changes)}"
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