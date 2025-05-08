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
using SabiMarket.Infrastructure.Utilities;
using System.Text.Json;
using SabiMarket.Application.Interfaces;
using ValidationException = FluentValidation.ValidationException;
using SabiMarket.Services.Dtos.Levy;

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
        //private readonly IValidator<UpdateGoodBoyProfileDto> _updateProfileValidator;

        public GoodBoysService(
            IRepositoryManager repository,
            ILogger<GoodBoysService> logger,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUser,
            IHttpContextAccessor httpContextAccessor,
            IValidator<CreateGoodBoyRequestDto> createGoodBoyValidator)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _currentUser = currentUser;
            _httpContextAccessor = httpContextAccessor;
            _createGoodBoyValidator = createGoodBoyValidator;
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
               /* var validationResult = await _updateProfileValidator.ValidateAsync(profileDto);
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
                }*/

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
                /*if (!scanDto.QRCodeData.StartsWith("OSH/LAG/"))
                {
                    return ResponseFactory.Fail<TraderQRValidationResponseDto>(
                        "Invalid trader QR code");
                }*/

                if(string.IsNullOrEmpty(scanDto?.TraderId))
                {
                    return ResponseFactory.Fail<TraderQRValidationResponseDto>(
                       "traderId is required");
                }

                //var traderId = scanDto.QRCodeData.Replace("OSH/LAG/", "");

                // Get the trader by ID
                var trader = await _repository.TraderRepository.GetTraderById(scanDto?.TraderId, trackChanges: false);

                if (trader == null)
                {
                    await CreateAuditLog(
                        "QR Code Validation Failed",
                        $"Invalid trader ID from QR Code: {scanDto?.TraderId}",
                        "Payment Processing"
                    );
                    return ResponseFactory.Fail<TraderQRValidationResponseDto>(
                        new NotFoundException("Trader not found"),
                        "Invalid trader QR code");
                }

                // Check if scanning user is authorized (must be a GoodBoy)
                var goodBoy = await _repository.GoodBoyRepository.GetGoodBoyByUserId(scanDto.ScannedByUserId);
                if (goodBoy == null)
                {
                    return ResponseFactory.Fail<TraderQRValidationResponseDto>(
                        new UnauthorizedException("Unauthorized scan attempt"),
                        "Unauthorized to scan trader QR codes");
                }

                // Get payment frequency and amount from most recent levy payment for this market and trader occupancy
                var levySetups = await _repository.LevyPaymentRepository.GetByMarketAndOccupancyAsync(
                    trader.MarketId,
                    trader.TraderOccupancy);

                string paymentFrequency = "Not configured";
                if (levySetups != null && levySetups.Any())
                {
                    // Get the most recent levy payment setup for this trader occupancy
                    var latestSetup = levySetups
                        .OrderByDescending(lp => lp.CreatedAt)
                        .FirstOrDefault();

                    if (latestSetup != null)
                    {
                        // Format payment frequency string based on period and amount
                        paymentFrequency = $"{GetPeriodDays(latestSetup.Period)} days - N{latestSetup.Amount}";
                    }
                }

                // Get the most recent payment for this trader
                var latestPayment = trader.LevyPayments
                    .OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefault();

                // Create response with dynamic data from the trader entity
                var validationResponse = new TraderQRValidationResponseDto
                {
                    TraderId = trader.Id,
                    TraderName = $"{trader.User.FirstName} {trader.User.LastName}",
                    TraderOccupancy = trader.TraderOccupancy.ToString(),
                    TraderIdentityNumber =   trader.TIN, //$"OSH/LAG/{trader.Id}",
                    PaymentFrequency = paymentFrequency,
                    LastPaymentDate = latestPayment?.PaymentDate,
                    UpdatePaymentUrl = $"/payments/updatetraderpayment/{trader.Id}"
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

        // Helper method to convert PaymentPeriodEnum to number of days
        private int GetPeriodDays(PaymentPeriodEnum period)
        {
            return period switch
            {
                PaymentPeriodEnum.Daily => 1,
                PaymentPeriodEnum.Weekly => 7,
                PaymentPeriodEnum.BiWeekly => 14,
                PaymentPeriodEnum.Monthly => 30,
                PaymentPeriodEnum.Quarterly => 90,
                PaymentPeriodEnum.HalfYearly => 180,
                PaymentPeriodEnum.Yearly => 365,
                _ => 0
            };
        }

        /* public async Task<BaseResponse<TraderQRValidationResponseDto>> ValidateTraderQRCode(ScanTraderQRCodeDto scanDto)
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
                         .FirstOrDefault()?.PaymentDate,
                     UpdatePaymentUrl = ""
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
 */
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

        public async Task<BaseResponse<IEnumerable<GoodBoyLevyPaymentResponseDto>>> GetTodayLeviesForGoodBoy(string goodBoyId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Today's Levies Query",
                    $"CorrelationId: {correlationId} - Retrieving today's levies for GoodBoy ID: {goodBoyId}",
                    "Levy Management"
                );

                var today = DateTime.Now.Date;
                var tomorrow = today.AddDays(1);

                var todayLevies = await _repository.LevyPaymentRepository.GetLevyPaymentsByDateRangeAsync(
                    goodBoyId,
                    today,
                    tomorrow
                );

                var levyPaymentDtos = _mapper.Map<IEnumerable<GoodBoyLevyPaymentResponseDto>>(todayLevies);

                await CreateAuditLog(
                    "Today's Levies Retrieved",
                    $"CorrelationId: {correlationId} - Retrieved {levyPaymentDtos.Count()} levies for GoodBoy ID: {goodBoyId}",
                    "Levy Management"
                );

                return ResponseFactory.Success(levyPaymentDtos, "Today's levies retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Today's Levies Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Levy Management"
                );
                return ResponseFactory.Fail<IEnumerable<GoodBoyLevyPaymentResponseDto>>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<GoodBoyDashboardStatsDto>> GetDashboardStats(string goodBoyId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Dashboard Stats Query",
                    $"CorrelationId: {correlationId} - Retrieving dashboard stats for GoodBoy ID: {goodBoyId}",
                    "Levy Management"
                );

                // Set default date range if not provided (current month)
                if (!fromDate.HasValue || !toDate.HasValue)
                {
                    fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    toDate = fromDate.Value.AddMonths(1).AddDays(-1);
                }

                // Get trader count managed by the good boy
                var traderCount = await _repository.TraderRepository.GetTraderCountByGoodBoyIdAsync(goodBoyId);

                // Get total levy amount collected by the good boy in the date range
                var totalLevies = await _repository.LevyPaymentRepository.GetTotalLevyAmountByGoodBoyIdAsync(
                    goodBoyId,
                    fromDate.Value,
                    toDate.Value
                );

                var result = new GoodBoyDashboardStatsDto
                {
                    TraderCount = traderCount,
                    TotalLevies = totalLevies
                };

                await CreateAuditLog(
                    "Dashboard Stats Retrieved",
                    $"CorrelationId: {correlationId} - Dashboard stats retrieved for GoodBoy ID: {goodBoyId}",
                    "Levy Management"
                );

                return ResponseFactory.Success(result, "Dashboard statistics retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Dashboard Stats Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Levy Management"
                );
                return ResponseFactory.Fail<GoodBoyDashboardStatsDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<GoodBoyLevyPaymentResponseDto>> CollectLevyPayment(LevyPaymentCreateDto levyPaymentDto)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Levy Payment Collection",
                    $"CorrelationId: {correlationId} - Collecting levy payment from Trader ID: {levyPaymentDto.TraderId}",
                    "Levy Management"
                );

                // Validate trader exists
                var trader = await _repository.TraderRepository.GetTraderById(levyPaymentDto.TraderId, false);
                if (trader == null)
                {
                    return ResponseFactory.Fail<GoodBoyLevyPaymentResponseDto>("Trader not found");
                }

                // Create the levy payment entity
                var levyPayment = new LevyPayment
                {
                    TraderId = levyPaymentDto.TraderId,
                    GoodBoyId = levyPaymentDto.GoodBoyId,
                    MarketId = trader.MarketId,
                    Amount = levyPaymentDto.Amount,
                    Period = levyPaymentDto.Period,
                    PaymentMethod = levyPaymentDto.PaymentMethod,
                    PaymentStatus = PaymentStatusEnum.Paid,
                    TransactionReference = Guid.NewGuid().ToString(),
                    HasIncentive = levyPaymentDto.HasIncentive,
                    IncentiveAmount = levyPaymentDto.IncentiveAmount,
                    PaymentDate = DateTime.Now,
                    CollectionDate = DateTime.Now,
                    Notes = levyPaymentDto.Notes,
                    QRCodeScanned = levyPaymentDto.QRCodeScanned
                };

                 _repository.LevyPaymentRepository.AddPayment(levyPayment);
                await _repository.SaveChangesAsync();

                var levyPaymentResponse = _mapper.Map<GoodBoyLevyPaymentResponseDto>(levyPayment);

                await CreateAuditLog(
                    "Levy Payment Collected",
                    $"CorrelationId: {correlationId} - Levy payment collected from Trader ID: {levyPaymentDto.TraderId}",
                    "Levy Management"
                );

                return ResponseFactory.Success(levyPaymentResponse, "Levy payment collected successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Levy Payment Collection Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Levy Management"
                );
                return ResponseFactory.Fail<GoodBoyLevyPaymentResponseDto>(ex, "An unexpected error occurred");
            }
        }
    }
}

