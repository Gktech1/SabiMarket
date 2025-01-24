using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Enum;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Infrastructure.Utilities;
using System.Text.Json;
using SabiMarket.Application.Interfaces;
using ValidationException = FluentValidation.ValidationException;

namespace SabiMarket.Infrastructure.Services
{
    public class GoodBoysService : IGoodBoysService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILogger<GoodBoysService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUser;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IValidator<CreateGoodBoyRequestDto> _createGoodBoyValidator;
        private readonly IValidator<UpdateGoodBoyProfileDto> _updateProfileValidator;

        public GoodBoysService(
            IRepositoryManager repository,
            ILogger<GoodBoysService> logger,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUser,
            IHttpContextAccessor httpContextAccessor,
            IValidator<CreateGoodBoyRequestDto> createGoodBoyValidator,
            IValidator<UpdateGoodBoyProfileDto> updateProfileValidator)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _currentUser = currentUser;
            _httpContextAccessor = httpContextAccessor;
            _createGoodBoyValidator = createGoodBoyValidator;
            _updateProfileValidator = updateProfileValidator;
        }

        private string GetCurrentIpAddress()
        {
            return _httpContextAccessor.GetRemoteIPAddress();
        }

        private async Task CreateAuditLog(string activity, string details, string module = "GoodBoys Management")
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

        public async Task<BaseResponse<GoodBoyResponseDto>> GetGoodBoyById(string goodBoyId)
        {
            try
            {
                var goodBoy = await _repository.GoodBoyRepository.GetGoodBoyById(goodBoyId, trackChanges: false);
                if (goodBoy == null)
                {
                    await CreateAuditLog(
                        "GoodBoy Lookup Failed",
                        $"Failed to find GoodBoy with ID: {goodBoyId}",
                        "GoodBoy Query"
                    );
                    return ResponseFactory.Fail<GoodBoyResponseDto>(
                        new NotFoundException("GoodBoy not found"),
                        "GoodBoy not found");
                }

                var goodBoyDto = _mapper.Map<GoodBoyResponseDto>(goodBoy);

                await CreateAuditLog(
                    "GoodBoy Lookup",
                    $"Retrieved GoodBoy details for ID: {goodBoyId}",
                    "GoodBoy Query"
                );

                return ResponseFactory.Success(goodBoyDto, "GoodBoy retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GoodBoy");
                return ResponseFactory.Fail<GoodBoyResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<GoodBoyResponseDto>> CreateGoodBoy(CreateGoodBoyRequestDto goodBoyDto)
        {
            try
            {
                var validationResult = await _createGoodBoyValidator.ValidateAsync(goodBoyDto);
                if (!validationResult.IsValid)
                {
                    await CreateAuditLog(
                        "GoodBoy Creation Failed",
                        $"Validation failed for new GoodBoy creation with email: {goodBoyDto.Email}",
                        "GoodBoy Creation"
                    );
                    return ResponseFactory.Fail<GoodBoyResponseDto>(
                        "Validation failed");
                }

                var existingUser = await _userManager.FindByEmailAsync(goodBoyDto.Email);
                if (existingUser != null)
                {
                    await CreateAuditLog(
                        "GoodBoy Creation Failed",
                        $"Email already exists: {goodBoyDto.Email}",
                        "GoodBoy Creation"
                    );
                    return ResponseFactory.Fail<GoodBoyResponseDto>("Email already exists");
                }

                var user = _mapper.Map<ApplicationUser>(goodBoyDto);

                var createUserResult = await _userManager.CreateAsync(user);
                if (!createUserResult.Succeeded)
                {
                    await CreateAuditLog(
                        "GoodBoy Creation Failed",
                        $"Failed to create user account for: {goodBoyDto.Email}",
                        "GoodBoy Creation"
                    );
                    return ResponseFactory.Fail<GoodBoyResponseDto>(
                        "Failed to create user");
                }

                var goodBoy = new GoodBoy
                {
                    UserId = user.Id,
                    CaretakerId = goodBoyDto.CaretakerId,
                    MarketId = goodBoyDto.MarketId,
                    Status = StatusEnum.Blocked
                };

                _repository.GoodBoyRepository.AddGoodBoy(goodBoy);
                await _repository.SaveChangesAsync();

                await _userManager.AddToRoleAsync(user, UserRoles.Goodboy);

                await CreateAuditLog(
                    "Created GoodBoy Account",
                    $"Created GoodBoy account for {user.Email} ({user.FirstName} {user.LastName})"
                );

                var createdGoodBoy = _mapper.Map<GoodBoyResponseDto>(goodBoy);
                return ResponseFactory.Success(createdGoodBoy, "GoodBoy created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating GoodBoy");
                return ResponseFactory.Fail<GoodBoyResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> UpdateGoodBoyProfile(string goodBoyId, UpdateGoodBoyProfileDto profileDto)
        {
            try
            {
                var validationResult = await _updateProfileValidator.ValidateAsync(profileDto);
                if (!validationResult.IsValid)
                {
                    await CreateAuditLog(
                        "Profile Update Failed",
                        $"Validation failed for GoodBoy profile update ID: {goodBoyId}",
                        "Profile Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new FluentValidation.ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                var goodBoy = await _repository.GoodBoyRepository.GetGoodBoyById(goodBoyId, trackChanges: true);
                if (goodBoy == null)
                {
                    await CreateAuditLog(
                        "Profile Update Failed",
                        $"GoodBoy not found for ID: {goodBoyId}",
                        "Profile Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("GoodBoy not found"),
                        "GoodBoy not found");
                }

                var user = await _userManager.FindByIdAsync(goodBoy.UserId);
                if (user == null)
                {
                    await CreateAuditLog(
                        "Profile Update Failed",
                        $"User not found for GoodBoy ID: {goodBoyId}",
                        "Profile Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("User not found"),
                        "User not found");
                }

                // Track changes for audit log
                var changes = new List<string>();
                if (user.PhoneNumber != profileDto.PhoneNumber)
                    changes.Add($"Phone: {user.PhoneNumber} → {profileDto.PhoneNumber}");

                // Update user properties
                user.PhoneNumber = profileDto.PhoneNumber;

                var updateUserResult = await _userManager.UpdateAsync(user);
                if (!updateUserResult.Succeeded)
                {
                    await CreateAuditLog(
                        "Profile Update Failed",
                        $"Failed to update user properties for GoodBoy ID: {goodBoyId}",
                        "Profile Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        "Failed to update user");
                }

                _repository.GoodBoyRepository.UpdateGoodBoy(goodBoy);

                await CreateAuditLog(
                    "Updated GoodBoy Profile",
                    $"Updated profile for {user.Email}. Changes: {string.Join(", ", changes)}",
                    "Profile Management"
                );

                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "GoodBoy profile updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating GoodBoy profile");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>> GetGoodBoys(
            GoodBoyFilterRequestDto filterDto, PaginationFilter paginationFilter)
        {
            try
            {
                var query = _repository.GoodBoyRepository.FindAll(false);

                // Apply filters
                if (!string.IsNullOrEmpty(filterDto.MarketId))
                    query = query.Where(g => g.MarketId == filterDto.MarketId);

                if (!string.IsNullOrEmpty(filterDto.CaretakerId))
                    query = query.Where(g => g.CaretakerId == filterDto.CaretakerId);

                if (filterDto.Status.HasValue)
                    query = query.Where(g => g.Status == filterDto.Status.Value);

                var paginatedGoodBoys = await query.Paginate(paginationFilter);

                var goodBoyDtos = _mapper.Map<IEnumerable<GoodBoyResponseDto>>(paginatedGoodBoys.PageItems);
                var result = new PaginatorDto<IEnumerable<GoodBoyResponseDto>>
                {
                    PageItems = goodBoyDtos,
                    PageSize = paginatedGoodBoys.PageSize,
                    CurrentPage = paginatedGoodBoys.CurrentPage,
                    NumberOfPages = paginatedGoodBoys.NumberOfPages
                };

                await CreateAuditLog(
                    "GoodBoy List Query",
                    $"Retrieved GoodBoy list - Page {paginationFilter.PageNumber}, " +
                    $"Size {paginationFilter.PageSize}, Filters: {JsonSerializer.Serialize(filterDto)}",
                    "GoodBoy Query"
                );

                return ResponseFactory.Success(result, "GoodBoys retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GoodBoys");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>(
                    ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> ProcessLevyPayment(string goodBoyId, ProcessLevyPaymentDto paymentDto)
        {
            try
            {
                var goodBoy = await _repository.GoodBoyRepository.GetGoodBoyById(goodBoyId, trackChanges: false);
                if (goodBoy == null)
                {
                    await CreateAuditLog(
                        "Levy Payment Failed",
                        $"GoodBoy not found for ID: {goodBoyId}",
                        "Levy Payment"
                    );
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("GoodBoy not found"),
                        "GoodBoy not found");
                }

                var levyPayment = _mapper.Map<LevyPayment>(paymentDto);
                levyPayment.GoodBoyId = goodBoyId;

                _repository.LevyPaymentRepository.Create(levyPayment);

                await CreateAuditLog(
                    "Levy Payment Processed",
                    $"Processed levy payment of {paymentDto.Amount} for GoodBoy ID: {goodBoyId}",
                    "Levy Payment"
                );

                await _repository.SaveChangesAsync();
                return ResponseFactory.Success(true, "Levy payment processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing levy payment");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<TraderDetailsDto>> GetTraderDetails(string traderId)
        {
            try
            {
                var goodBoy = await _repository.GoodBoyRepository.GetGoodBoyById(traderId, trackChanges: false);
                if (goodBoy == null)
                {
                    await CreateAuditLog(
                        "Trader Details Lookup Failed",
                        $"Failed to find trader with ID: {traderId}",
                        "Trader Query"
                    );
                    return ResponseFactory.Fail<TraderDetailsDto>(
                        new NotFoundException("Trader not found"),
                        "Trader not found");
                }

                var traderDetails = _mapper.Map<TraderDetailsDto>(goodBoy);

                await CreateAuditLog(
                    "Trader Details Lookup",
                    $"Retrieved trader details for ID: {traderId}",
                    "Trader Query"
                );

                return ResponseFactory.Success(traderDetails, "Trader details retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trader details");
                return ResponseFactory.Fail<TraderDetailsDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<TraderQRValidationResponseDto>> ValidateTraderQRCode(ScanTraderQRCodeDto scanDto)
        {
            try
            {
                // Validate QR code format (OSH/LAG/23401)
                if (!scanDto.QRCodeData.StartsWith("OSH/LAG/"))
                {
                    return ResponseFactory.Fail<TraderQRValidationResponseDto>(
                        "Invalid trader QR code");
                }

                var traderId = scanDto.QRCodeData.Replace("OSH/LAG/", "");
                var trader = await _repository.GoodBoyRepository.GetGoodBoyById(traderId, trackChanges: false);

                if (trader == null)
                {
                    await CreateAuditLog(
                        "QR Code Validation Failed",
                        $"Invalid trader ID from QR Code: {traderId}",
                        "Payment Processing"
                    );
                    return ResponseFactory.Fail<TraderQRValidationResponseDto>(
                        new NotFoundException("Trader not found"),
                        "Invalid trader QR code");
                }

                // Check if scanning user is authorized
                var goodBoy = await _repository.GoodBoyRepository.GetGoodBoyByUserId(scanDto.ScannedByUserId);
                if (goodBoy == null)
                {
                    return ResponseFactory.Fail<TraderQRValidationResponseDto>(
                        new UnauthorizedException("Unauthorized scan attempt"),
                        "Unauthorized to scan trader QR codes");
                }

                var validationResponse = new TraderQRValidationResponseDto
                {
                    TraderId = trader.Id,
                    TraderName = $"{trader.User.FirstName} {trader.User.LastName}",
                    TraderOccupancy = "Open Space",
                    TraderIdentityNumber = $"OSH/LAG/{trader.Id}",
                    PaymentFrequency = "2 days - N500",
                    LastPaymentDate = trader.LevyPayments
                        .OrderByDescending(p => p.PaymentDate)
                        .FirstOrDefault()?.PaymentDate
                };

                await CreateAuditLog(
                    "Trader QR Code Scanned",
                    $"Trader QR Code scanned by GoodBoy: {goodBoy.Id} for Trader: {trader.Id}",
                    "Payment Processing"
                );

                return ResponseFactory.Success(validationResponse, "Trader QR code validated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating trader QR code");
                return ResponseFactory.Fail<TraderQRValidationResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> VerifyTraderPaymentStatus(string traderId)
        {
            try
            {
                var trader = await _repository.GoodBoyRepository.GetGoodBoyById(traderId, trackChanges: false);
                if (trader == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Trader not found"),
                        "Trader not found");
                }

                var lastPayment = trader.LevyPayments
                    .OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefault();

                if (lastPayment == null)
                {
                    return ResponseFactory.Success(false, "Payment required");
                }

                // Check if payment is within the 2-day window
                var daysSinceLastPayment = (DateTime.UtcNow - lastPayment.PaymentDate).TotalDays;
                var isPaymentValid = daysSinceLastPayment <= 2;

                return ResponseFactory.Success(isPaymentValid,
                    isPaymentValid ? "Payment is up to date" : "Payment required");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying trader payment status");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> UpdateTraderPayment(string traderId, ProcessLevyPaymentDto paymentDto)
        {
            try
            {
                var trader = await _repository.GoodBoyRepository.GetGoodBoyById(traderId, trackChanges: false);
                if (trader == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Trader not found"),
                        "Trader not found");
                }

                // Verify payment hasn't already been made today
                var existingPayment = trader.LevyPayments
                    .Any(p => p.PaymentDate.Date == DateTime.UtcNow.Date);

                if (existingPayment)
                {
                    return ResponseFactory.Fail<bool>(
                        new ValidationException("Payment already processed for today"),
                        "Payment already processed for today");
                }

                var levyPayment = _mapper.Map<LevyPayment>(paymentDto);
                levyPayment.GoodBoyId = traderId;

                _repository.LevyPaymentRepository.Create(levyPayment);

                await CreateAuditLog(
                    "Levy Payment Updated",
                    $"Updated levy payment for Trader: {traderId}, Amount: {paymentDto.Amount}",
                    "Payment Processing"
                );

                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "Payment processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trader payment");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }
    }
}
