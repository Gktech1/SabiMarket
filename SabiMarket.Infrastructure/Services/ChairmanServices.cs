﻿
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.IRepositories;
using SabiMarket.Application.IServices;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Infrastructure.Helpers;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Infrastructure.Services
{
    public class ChairmanService : IChairmanService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILogger<ChairmanService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUser;
        private readonly IValidator<CreateChairmanRequestDto> _createChairmanValidator;
        private readonly IValidator<UpdateProfileDto> _updateProfileValidator;
        private readonly IValidator<CreateAssistantOfficerRequestDto> _createAssistOfficerValidator;

        // Add these properties to your ChairmanService class
        private readonly IValidator<CreateLevyRequestDto> _createLevyValidator;
        private readonly IValidator<UpdateLevyRequestDto> _updateLevyValidator;

        // Update the constructor to include new validators
        public ChairmanService(
            IRepositoryManager repository,
            ILogger<ChairmanService> logger,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IValidator<CreateChairmanRequestDto> createChairmanValidator,
            IValidator<UpdateProfileDto> updateProfileValidator,
            IValidator<CreateLevyRequestDto> createLevyValidator,
            IValidator<UpdateLevyRequestDto> updateLevyValidator,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _createChairmanValidator = createChairmanValidator;
            _updateProfileValidator = updateProfileValidator;
            _createLevyValidator = createLevyValidator;
            _updateLevyValidator = updateLevyValidator;
            _currentUser = currentUser;
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<LevyInfoResponseDto>>>> GetMarketLevies(
            string marketId, PaginationFilter paginationFilter)
        {
            try
            {
                var chairmanId = _currentUser.GetUserId();
                var market = await _repository.MarketRepository.GetMarketByIdAsync(marketId, false);
                if (market == null || market.ChairmanId != chairmanId)
                {
                    return ResponseFactory.Fail<PaginatorDto<IEnumerable<LevyInfoResponseDto>>>(
                        new UnauthorizedException("Unauthorized to view this market's levies"),
                        "Unauthorized access");
                }

                var query = _repository.LevyPaymentRepository.GetMarketLevySetups(marketId);
                var paginatedLevies = await query.Paginate(paginationFilter);

                var levyDtos = _mapper.Map<IEnumerable<LevyInfoResponseDto>>(paginatedLevies.PageItems);
                var result = new PaginatorDto<IEnumerable<LevyInfoResponseDto>>
                {
                    PageItems = levyDtos,
                    PageSize = paginatedLevies.PageSize,
                    CurrentPage = paginatedLevies.CurrentPage,
                    NumberOfPages = paginatedLevies.NumberOfPages
                };

                return ResponseFactory.Success(result, "Market levy configurations retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving market levy configurations");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<LevyInfoResponseDto>>>(
                    ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<ChairmanResponseDto>> GetChairmanById(string chairmanId)
        {
            try
            {
                var chairman = await _repository.ChairmanRepository.GetChairmanByIdAsync(chairmanId, trackChanges: false);
                if (chairman == null)
                {
                    return ResponseFactory.Fail<ChairmanResponseDto>(
                        new NotFoundException("Chairman not found"),
                        "Chairman not found");
                }

                var chairmanDto = _mapper.Map<ChairmanResponseDto>(chairman);
                return ResponseFactory.Success(chairmanDto, "Chairman retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chairman");
                return ResponseFactory.Fail<ChairmanResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<ChairmanResponseDto>> CreateChairman(CreateChairmanRequestDto chairmanDto)
        {
            try
            {
                var userId = _currentUser.GetUserId();
                var validationResult = await _createChairmanValidator.ValidateAsync(chairmanDto);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Fail<ChairmanResponseDto>(
                        new FluentValidation.ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                var existingChairman = await _repository.ChairmanRepository.ChairmanExistsAsync(userId, chairmanDto.MarketId);
                if (existingChairman)
                {
                    return ResponseFactory.Fail<ChairmanResponseDto>(
                        new BadRequestException("Chairman already exists"),
                        "Chairman already exists");
                }

                var chairman = _mapper.Map<Chairman>(chairmanDto);
                _repository.ChairmanRepository.CreateChairman(chairman);
                await _repository.SaveChangesAsync();

                var createdChairman = _mapper.Map<ChairmanResponseDto>(chairman);
                return ResponseFactory.Success(createdChairman, "Chairman created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chairman");
                return ResponseFactory.Fail<ChairmanResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> UpdateChairmanProfile(string chairmanId, UpdateProfileDto profileDto)
        {
            try
            {
                var validationResult = await _updateProfileValidator.ValidateAsync(profileDto);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Fail<bool>(
                        new FluentValidation.ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                var chairman = await _repository.ChairmanRepository.GetChairmanByIdAsync(chairmanId, trackChanges: true);
                if (chairman == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Chairman not found"),
                        "Chairman not found");
                }

                _mapper.Map(profileDto, chairman);
                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "Chairman profile updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating chairman profile");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<ChairmanResponseDto>>>> GetChairmen(
            PaginationFilter paginationFilter)
        {
            try
            {
                var chairmenPage = await _repository.ChairmanRepository.GetChairmenWithPaginationAsync(paginationFilter, trackChanges: false);

                var chairmanDtos = _mapper.Map<IEnumerable<ChairmanResponseDto>>(chairmenPage.PageItems);
                var response = new PaginatorDto<IEnumerable<ChairmanResponseDto>>
                {
                    PageItems = chairmanDtos,
                    CurrentPage = chairmenPage.CurrentPage,
                    PageSize = chairmenPage.PageSize,
                    NumberOfPages = chairmenPage.NumberOfPages
                };

                return ResponseFactory.Success(response, "Chairmen retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chairmen");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<ChairmanResponseDto>>>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> AssignCaretakerToChairman(string chairmanId, string caretakerId)
        {
            try
            {
                var chairman = await _repository.ChairmanRepository
                    .GetChairmanByIdAsync(chairmanId, trackChanges: true);

                if (chairman == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Chairman not found"),
                        "Chairman not found");
                }

                var caretaker = await _repository.CaretakerRepository.GetCaretakerById(caretakerId, trackChanges: true);
                if (caretaker == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Caretaker not found"),
                        "Caretaker not found");
                }

                caretaker.ChairmanId = chairmanId;
                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "Caretaker assigned successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning caretaker to chairman");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<DashboardMetricsResponseDto>> GetDashboardMetrics()
        {
            try
            {
                string preset = DateRangePresets.Today;
                var dateRange = DateRangePresets.GetPresetRange(preset);
                var previousDateRange = GetPreviousDateRange(dateRange);

                // Get current period metrics
                var currentTraders = await _repository.TraderRepository.GetTraderCountAsync(dateRange.StartDate, dateRange.EndDate);
                var currentCaretakers = await _repository.CaretakerRepository.GetCaretakerCountAsync(dateRange.StartDate, dateRange.EndDate);
                var currentLevies = await _repository.LevyPaymentRepository.GetTotalLeviesAsync(dateRange.StartDate, dateRange.EndDate);

                // Get previous period metrics
                var previousTraders = await _repository.TraderRepository.GetTraderCountAsync(previousDateRange.StartDate, previousDateRange.EndDate);
                var previousCaretakers = await _repository.CaretakerRepository.GetCaretakerCountAsync(previousDateRange.StartDate, previousDateRange.EndDate);
                var previousLevies = await _repository.LevyPaymentRepository.GetTotalLeviesAsync(previousDateRange.StartDate, previousDateRange.EndDate);

                var response = new DashboardMetricsResponseDto
                {
                    Traders = CalculateMetricChange(currentTraders, previousTraders),
                    Caretakers = CalculateMetricChange(currentCaretakers, previousCaretakers),
                    Levies = CalculateMetricChange((int)currentLevies, (int)previousLevies),
                    TimePeriod = dateRange.DateRangeType
                };

                return ResponseFactory.Success(response, "Dashboard metrics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard metrics");
                return ResponseFactory.Fail<DashboardMetricsResponseDto>(ex, "An unexpected error occurred");
            }
        }

        private DateRangeDto GetPreviousDateRange(DateRangeDto currentRange)
        {
            var daysDifference = (currentRange.EndDate - currentRange.StartDate).Days + 1;
            return new DateRangeDto
            {
                StartDate = currentRange.StartDate.AddDays(-daysDifference),
                EndDate = currentRange.StartDate.AddDays(-1),
                DateRangeType = currentRange.DateRangeType
            };
        }

        private MetricChangeDto CalculateMetricChange(int currentValue, int previousValue)
        {
            decimal percentageChange = previousValue == 0 ?
                100 :
                ((decimal)(currentValue - previousValue) / previousValue) * 100;

            return new MetricChangeDto
            {
                CurrentValue = currentValue,
                PreviousValue = previousValue,
                PercentageChange = Math.Round(Math.Abs(percentageChange), 1),
                ChangeDirection = percentageChange >= 0 ? "Up" : "Down"
            };
        }

        public async Task<BaseResponse<IEnumerable<MarketResponseDto>>> GetAllMarkets()
        {
            try
            {
                var markets = await _repository.MarketRepository.GetAllMarketForExport(trackChanges: false);
                var marketDtos = _mapper.Map<IEnumerable<MarketResponseDto>>(markets);

                return ResponseFactory.Success(marketDtos, "Markets retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving markets");
                return ResponseFactory.Fail<IEnumerable<MarketResponseDto>>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> AssignCaretakerToMarket(string marketId, string caretakerId)
        {
            try
            {
                // Fetch the market by ID
                var market = await _repository.MarketRepository.GetMarketByIdAsync(marketId, trackChanges: true);
                if (market == null)
                {
                    return ResponseFactory.Fail<bool>(new NotFoundException("Market not found"), "Market not found");
                }

                // Fetch the caretaker by ID
                var caretaker = await _repository.CaretakerRepository.GetCaretakerById(caretakerId, trackChanges: true);
                if (caretaker == null)
                {
                    return ResponseFactory.Fail<bool>(new NotFoundException("Caretaker not found"), "Caretaker not found");
                }

                // Check if caretaker is already assigned to the market
                if (market.Caretakers.Any(c => c.Id == caretakerId))
                {
                    return ResponseFactory.Fail<bool>(new InvalidOperationException("Caretaker is already assigned to this market"), "Caretaker already assigned");
                }

                // Assign the caretaker to the market
                market.Caretakers.Add(caretaker);

                // Save changes
                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "Caretaker assigned to market successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning caretaker to market");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<IEnumerable<CaretakerResponseDto>>> GetAllCaretakers()
        {
            try
            {
                var caretakers = await _repository.CaretakerRepository.GetAllCaretakers(trackChanges: false);
                var caretakerDtos = _mapper.Map<IEnumerable<CaretakerResponseDto>>(caretakers);

                return ResponseFactory.Success(caretakerDtos, "Caretakers retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving caretakers");
                return ResponseFactory.Fail<IEnumerable<CaretakerResponseDto>>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<AssistantOfficerResponseDto>> GetAssistantOfficerById(string officerId)
        {

            try
            {
                var officer = await _repository.AssistCenterOfficerRepository.GetByIdAsync(officerId, trackChanges: false);
                if (officer == null)
                {
                    return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                        new NotFoundException("Assistant Officer not found"),
                        "Assistant Officer not found");
                }

                var officerDto = _mapper.Map<AssistantOfficerResponseDto>(officer);
                return ResponseFactory.Success(officerDto, "Assistant Officer retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Assistant Officer");
                return ResponseFactory.Fail<AssistantOfficerResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<AssistantOfficerResponseDto>> CreateAssistantOfficer(CreateAssistantOfficerRequestDto officerDto)
        {
            try
            {
                var validationResult = await _createAssistOfficerValidator.ValidateAsync(officerDto);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                        new FluentValidation.ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                var officer = _mapper.Map<AssistCenterOfficer>(officerDto);
                _repository.AssistCenterOfficerRepository.AddAssistCenterOfficer(officer);
                await _repository.SaveChangesAsync();

                var createdOfficer = _mapper.Map<AssistantOfficerResponseDto>(officer);
                return ResponseFactory.Success(createdOfficer, "Assistant Officer created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Assistant Officer");
                return ResponseFactory.Fail<AssistantOfficerResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> UnblockAssistantOfficer(string officerId)
        {
            try
            {
                var officer = await _repository.AssistCenterOfficerRepository.GetByIdAsync(officerId, trackChanges: true);
                if (officer == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Assistant Officer not found"),
                        "Assistant Officer not found");
                }

                officer.IsBlocked = false;
                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "Assistant Officer unblocked successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unblocking Assistant Officer");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> BlockAssistantOfficer(string officerId)
        {
            try
            {
                var officer = await _repository.AssistCenterOfficerRepository.GetByIdAsync(officerId, trackChanges: true);
                if (officer == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Assistant Officer not found"),
                        "Assistant Officer not found");
                }

                officer.IsBlocked = true;
                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "Assistant Officer blocked successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking Assistant Officer");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<IEnumerable<ReportResponseDto>>> GetChairmanReports(string chairmanId)
        {
            try
            {
                var reports = await _repository.ChairmanRepository.GetReportsByChairmanIdAsync(chairmanId);
                if (reports == null || !reports.Any())
                {
                    return ResponseFactory.Fail<IEnumerable<ReportResponseDto>>(
                        new NotFoundException("No reports found"),
                        "No reports available");
                }

                var reportDtos = _mapper.Map<IEnumerable<ReportResponseDto>>(reports);
                return ResponseFactory.Success(reportDtos, "Reports retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chairman reports");
                return ResponseFactory.Fail<IEnumerable<ReportResponseDto>>(ex, "An unexpected error occurred");
            }


        }

        public async Task<BaseResponse<LevyResponseDto>> CreateLevy(CreateLevyRequestDto request)
        {
            try
            {
                var validationResult = await _createLevyValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Fail<LevyResponseDto>(
                        new FluentValidation.ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                // Get current chairman ID from current user service
                var chairmanId = _currentUser.GetUserId();

                // Validate market exists and belongs to chairman
                var market = await _repository.MarketRepository.GetMarketById(request.MarketId,false);
                if (market == null || market.ChairmanId != chairmanId)
                    return ResponseFactory.Fail<LevyResponseDto>(new NotFoundException("Market not found"), "Market not found or unauthorized.");

                // Validate trader exists and belongs to the market
                var trader = await _repository.TraderRepository.GetTraderById(request.TraderId, false);
                if (trader == null || trader.MarketId != request.MarketId)
                    return ResponseFactory.Fail<LevyResponseDto>(new NotFoundException("Trader not found"), "Trader not found or not in specified market.");

                // Validate good boy exists and is active
                var goodBoy = await _repository.GoodBoyRepository.GetGoodBoyById(request.GoodBoyId);
                if (goodBoy == null || goodBoy.Status == StatusEnum.Blocked)
                    return ResponseFactory.Fail<LevyResponseDto>(new NotFoundException("Good Boy not found"), "Good Boy not found or inactive.");

                var levy = _mapper.Map<LevyPayment>(request);
                levy.ChairmanId = chairmanId;
                levy.PaymentStatus = PaymentStatusEnum.Pending;
                levy.PaymentDate = DateTime.UtcNow;
                levy.TransactionReference = GenerateTransactionReference();

                _repository.LevyPaymentRepository.AddPayment(levy);
                await _repository.SaveChangesAsync();

                var responseDto = _mapper.Map<LevyResponseDto>(levy);
                return ResponseFactory.Success(responseDto, "Levy created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating levy");
                return ResponseFactory.Fail<LevyResponseDto>(ex, "Error creating levy");
            }
        }

        public async Task<BaseResponse<bool>> UpdateLevy(string levyId, UpdateLevyRequestDto request)
        {
            try
            {
                var validationResult = await _updateLevyValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    return ResponseFactory.Fail<bool>(
                        new FluentValidation.ValidationException(validationResult.Errors),
                        "Validation failed");
                // Get current chairman ID from current user service
                var chairmanId = _currentUser.GetUserId();

                var levy = await _repository.LevyPaymentRepository.GetPaymentById(levyId, trackChanges: true);
                if (levy == null || levy.ChairmanId != chairmanId)
                    return ResponseFactory.Fail<bool>(new NotFoundException("Levy not found"), "Levy not found or unauthorized.");

                // Validate market exists and belongs to chairman
                var market = await _repository.MarketRepository.GetMarketById(request.MarketId, false);
                if (market == null || market.ChairmanId != chairmanId)
                    return ResponseFactory.Fail<bool>(new NotFoundException("Market not found"), "Market not found or unauthorized.");

                // Validate trader exists and belongs to the market
                var trader = await _repository.TraderRepository.GetTraderById(request.TraderId, false);
                if (trader == null || trader.MarketId != request.MarketId)
                    return ResponseFactory.Fail<bool>(new NotFoundException("Trader not found"), "Trader not found or not in specified market.");

                if (!string.IsNullOrEmpty(request.GoodBoyId))
                {
                    var goodBoy = await _repository.GoodBoyRepository.GetGoodBoyById(request.GoodBoyId);
                    if (goodBoy == null || goodBoy.Status == StatusEnum.Blocked)
                        return ResponseFactory.Fail<bool>(new NotFoundException("Good Boy not found"), "Good Boy not found or inactive.");
                }

                _mapper.Map(request, levy);
                levy.UpdatedAt = DateTime.UtcNow;

                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "Levy updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating levy");
                return ResponseFactory.Fail<bool>(ex, "Error updating levy");
            }
        }

        private string GenerateTransactionReference()
        {
            return $"LVY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}".ToUpper();
        }

        public async Task<BaseResponse<bool>> DeleteLevy(string levyId)
        {
            try
            {
                var levy = await _repository.LevyPaymentRepository.GetPaymentById(levyId, trackChanges: true);
                if (levy == null)
                {
                    return ResponseFactory.Fail<bool>(new NotFoundException("Levy not found"), "Levy not found.");
                }

                _repository.LevyPaymentRepository.DeleteLevyPayment(levy);
                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "Levy deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting levy");
                return ResponseFactory.Fail<bool>(ex, "Error deleting levy");
            }
        }

        public async Task<BaseResponse<LevyResponseDto>> GetLevyById(string levyId)
        {
            try
            {
                // Fetch the levy payment from the repository
                var levy = await _repository.LevyPaymentRepository.GetPaymentById(levyId, trackChanges: false);

                // Check if levy exists
                if (levy == null)
                {
                    return ResponseFactory.Fail<LevyResponseDto>(new NotFoundException("Levy not found"), "Levy not found.");
                }

                // Map the levy payment entity to response DTO
                var levyDto = _mapper.Map<LevyResponseDto>(levy);

                // Return a successful response
                return ResponseFactory.Success(levyDto, "Levy retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving levy");
                return ResponseFactory.Fail<LevyResponseDto>(ex, "Error retrieving levy");
            }
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<LevyResponseDto>>>> GetAllLevies(string chairmanId, PaginationFilter filter)
        {
            try
            {
                // Fetch paginated levy payments from the repository
                var levyPayments = await _repository.LevyPaymentRepository.GetLevyPaymentsAsync(chairmanId, filter, trackChanges: false);

                // Map the levy payment entities to response DTOs
                var levyDtos = _mapper.Map<IEnumerable<LevyResponseDto>>(levyPayments.PageItems);

                // Calculate the number of pages
                var numberOfPages = levyPayments.NumberOfPages;

                // Create the paginated result
                var paginatedResult = new PaginatorDto<IEnumerable<LevyResponseDto>>
                {
                    PageItems = levyDtos,
                    PageSize = filter.PageSize,
                    CurrentPage = filter.PageNumber,
                    NumberOfPages = numberOfPages
                };



                // Return a successful response
                return ResponseFactory.Success(paginatedResult, "Levies retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving levies");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<LevyResponseDto>>>(ex, "Error retrieving levies");
            }
        }

        // Market Management Implementations
        public async Task<BaseResponse<MarketResponseDto>> CreateMarket(CreateMarketRequestDto request)
        {
            try
            {
                var market = _mapper.Map<Market>(request);
                _repository.MarketRepository.AddMarket(market);
                await _repository.SaveChangesAsync();

                var marketDto = _mapper.Map<MarketResponseDto>(market);
                return ResponseFactory.Success(marketDto, "Market created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating market");
                return ResponseFactory.Fail<MarketResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> UpdateMarket(string marketId, UpdateMarketRequestDto request)
        {
            try
            {
                var market = await _repository.MarketRepository.GetMarketByIdAsync(marketId, trackChanges: true);
                if (market == null)
                {
                    return ResponseFactory.Fail<bool>(new NotFoundException("Market not found"), "Market not found");
                }

                _mapper.Map(request, market);
                await _repository.SaveChangesAsync();
                return ResponseFactory.Success(true, "Market updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating market");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> DeleteMarket(string marketId)
        {
            try
            {
                var market = await _repository.MarketRepository.GetMarketByIdAsync(marketId, trackChanges: true);
                if (market == null)
                {
                    return ResponseFactory.Fail<bool>(new NotFoundException("Market not found"), "Market not found");
                }

                _repository.MarketRepository.DeleteMarket(market);
                await _repository.SaveChangesAsync();
                return ResponseFactory.Success(true, "Market deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting market");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<MarketRevenueDto>> GetMarketRevenue(string marketId)
        {
            try
            {
                string preset = DateRangePresets.ThisMonth;
                var dateRange = DateRangePresets.GetPresetRange(preset);
                var revenue = await _repository.MarketRepository.GetMarketRevenueAsync(
                    marketId,
                    dateRange.StartDate,
                    dateRange.EndDate
                );
                var revenueDto = _mapper.Map<MarketRevenueDto>(revenue);
                return ResponseFactory.Success(revenueDto, "Market revenue retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving market revenue");
                return ResponseFactory.Fail<MarketRevenueDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<ReportMetricsDto>> GetReportMetrics()
        {
            try
            {
                string preset = DateRangePresets.ThisMonth;
                var dateRange = DateRangePresets.GetPresetRange(preset);
                var metrics = await _repository.ReportRepository.GetMetricsAsync(dateRange.StartDate, dateRange.EndDate);
                var metricsDto = _mapper.Map<ReportMetricsDto>(metrics);
                metricsDto.Period = dateRange.DateRangeType;
                return ResponseFactory.Success(metricsDto, "Report metrics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report metrics");
                return ResponseFactory.Fail<ReportMetricsDto>(ex, "An unexpected error occurred");
            }
        }

        // Trader Management Implementations
        public async Task<BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>> GetTraders(
            string marketId, PaginationFilter filter)
        {
            try
            {
                var tradersPage = await _repository.TraderRepository.GetTradersByMarketAsync(marketId, filter, trackChanges: false);
                var traderDtos = _mapper.Map<IEnumerable<TraderResponseDto>>(tradersPage.PageItems);

                var paginatedResult = new PaginatorDto<IEnumerable<TraderResponseDto>>
                {
                    PageItems = traderDtos,
                    CurrentPage = filter.PageNumber,
                    PageSize = filter.PageSize,
                    NumberOfPages = tradersPage.NumberOfPages
                };

                return ResponseFactory.Success(paginatedResult, "Traders retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving traders");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<TraderResponseDto>>>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<QRCodeResponseDto>> GenerateTraderQRCode(string traderId)
        {
            try
            {
                // Get trader details
                var trader = await _repository.TraderRepository.GetTraderById(traderId, trackChanges: false);
                if (trader == null)
                {
                    return ResponseFactory.Fail<QRCodeResponseDto>(
                        new NotFoundException("Trader not found"),
                        "Trader not found");
                }

                // Generate QR code content with trader information
                var qrData = GenerateTraderQRContent(trader);

                // Generate QR code image
                var qrCodeImage = QRCodeHelper.GenerateQRCode(qrData, 300, 300);

                var response = new QRCodeResponseDto
                {
                    QRCodeImage = qrCodeImage,
                    QRCodeData = qrData,
                    TraderId = trader.Id,
                    TraderName = trader.BusinessName,
                    GeneratedAt = DateTime.UtcNow
                };

                return ResponseFactory.Success(response, "QR code generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for trader {TraderId}: {Error}", traderId, ex.Message);
                return ResponseFactory.Fail<QRCodeResponseDto>(ex, "An unexpected error occurred while generating QR code");
            }
        }

        private string GenerateTraderQRContent(Trader trader)
        {
            // Create a more structured QR content
            var qrContent = new
            {
                Id = trader.Id,
                BusinessName = trader.BusinessName,
                MarketId = trader.MarketId,
                Timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                Prefix = "SABIMARKET-TRADER"
            };

            // Serialize to JSON for more structured data
            return System.Text.Json.JsonSerializer.Serialize(qrContent);
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<AuditLogDto>>>> GetAuditLogs(PaginationFilter filter)
        {
            try
            {
                var auditLogs = await _repository.AuditLogRepository.GetPagedAuditLogs(filter);
                var auditLogDtos = _mapper.Map<IEnumerable<AuditLogDto>>(auditLogs.PageItems);

                var paginatedResult = new PaginatorDto<IEnumerable<AuditLogDto>>
                {
                    PageItems = auditLogDtos,
                    CurrentPage = filter.PageNumber,
                    PageSize = filter.PageSize,
                    NumberOfPages = auditLogs.NumberOfPages
                };

                return ResponseFactory.Success(paginatedResult, "Audit logs retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<AuditLogDto>>>(ex, "An unexpected error occurred");
            }
        }

        // Report Generation Implementations
        public async Task<BaseResponse<ReportMetricsDto>> GetReportMetrics(DateTime startDate, DateTime endDate)
        {
            try
            {
                var metrics = await _repository.ReportRepository.GetMetricsAsync(startDate, endDate);
                var metricsDto = _mapper.Map<ReportMetricsDto>(metrics);
                return ResponseFactory.Success(metricsDto, "Report metrics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report metrics");
                return ResponseFactory.Fail<ReportMetricsDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<DashboardMetricsResponseDto>> GetDailyMetricsChange()
        {
            try
            {
                string preset = DateRangePresets.Today;
                var dateRange = DateRangePresets.GetPresetRange(preset);
                var previousDateRange = GetPreviousDateRange(dateRange);

                // Get current day metrics
                var currentMetrics = await _repository.ReportRepository.GetMetricsAsync(dateRange.StartDate, dateRange.EndDate);

                // Get previous day metrics
                var previousMetrics = await _repository.ReportRepository.GetMetricsAsync(previousDateRange.StartDate, previousDateRange.EndDate);

                var response = new DashboardMetricsResponseDto
                {
                    Traders = CalculateMetricChange(currentMetrics.TotalTraders, previousMetrics.TotalTraders),
                    Caretakers = CalculateMetricChange(currentMetrics.TotalCaretakers, previousMetrics.TotalCaretakers),
                    Levies = CalculateMetricChange((int)currentMetrics.TotalRevenueGenerated, (int)previousMetrics.TotalRevenueGenerated),
                    TimePeriod = dateRange.DateRangeType,
                    ComplianceRate = new MetricChangeDto
                    {
                        CurrentValue = currentMetrics.ComplianceRate,
                        PreviousValue = previousMetrics.ComplianceRate,
                        PercentageChange = CalculatePercentageChange(previousMetrics.ComplianceRate, currentMetrics.ComplianceRate)
                    },
                    TransactionCount = new MetricChangeDto
                    {
                        CurrentValue = currentMetrics.PaymentTransactions,
                        PreviousValue = previousMetrics.PaymentTransactions,
                        PercentageChange = CalculatePercentageChange(previousMetrics.PaymentTransactions, currentMetrics.PaymentTransactions)
                    },
                    ActiveMarkets = new MetricChangeDto
                    {
                        CurrentValue = currentMetrics.ActiveMarkets,
                        PreviousValue = previousMetrics.ActiveMarkets,
                        PercentageChange = CalculatePercentageChange(previousMetrics.ActiveMarkets, currentMetrics.ActiveMarkets)
                    }
                };

                return ResponseFactory.Success(response, "Daily metrics changes retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating daily metrics changes");
                return ResponseFactory.Fail<DashboardMetricsResponseDto>(ex, "An unexpected error occurred");
            }
        }

        private decimal CalculatePercentageChange(decimal previous, decimal current)
        {
            if (previous == 0) return 100;
            return ((current - previous) / previous) * 100;
        }

        // Market Analytics Implementations
        public async Task<BaseResponse<MarketComplianceDto>> GetMarketComplianceRates(string marketId)
        {
            try
            {
                var compliance = await _repository.MarketRepository.GetComplianceRatesAsync(marketId);
                var complianceDto = _mapper.Map<MarketComplianceDto>(compliance);
                return ResponseFactory.Success(complianceDto, "Market compliance rates retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving market compliance rates");
                return ResponseFactory.Fail<MarketComplianceDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<byte[]>> ExportReport(ReportExportRequestDto request)
        {
            try
            {
                var report = await _repository.ReportRepository.ExportReport(request.StartDate, request.EndDate);
                var reportData = _mapper.Map<ReportExportDto>(report);
                var excelBytes = await ExcelExportHelper.GenerateMarketReport(reportData);
                return ResponseFactory.Success(excelBytes, "Report exported successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                return ResponseFactory.Fail<byte[]>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<MarketRevenueDto>> GetMarketRevenue(string marketId, DateRangeDto dateRange)
        {
            try
            {
                var revenue = await _repository.MarketRepository.GetMarketRevenueAsync(
                    marketId,
                    dateRange.StartDate,
                    dateRange.EndDate
                );

                var revenueDto = _mapper.Map<MarketRevenueDto>(revenue);
                return ResponseFactory.Success(revenueDto, "Market revenue retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving market revenue");
                return ResponseFactory.Fail<MarketRevenueDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> ConfigureLevySetup(LevySetupRequestDto request)
        {
            try
            {
                var levySetup = _mapper.Map<LevyPayment>(request);
                _repository.LevyPaymentRepository.AddPayment(levySetup);
                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "Levy setup configured successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring levy setup");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<IEnumerable<LevySetupResponseDto>>> GetLevySetups()
        {
            try
            {
                var levySetups = await _repository.LevyPaymentRepository.GetAllLevySetupsAsync(trackChanges: false);
                var levySetupDtos = _mapper.Map<IEnumerable<LevySetupResponseDto>>(levySetups);

                return ResponseFactory.Success(levySetupDtos, "Levy setups retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving levy setups");
                return ResponseFactory.Fail<IEnumerable<LevySetupResponseDto>>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<TraderDetailsDto>> GetTraderDetails(string traderId)
        {
            try
            {
                var trader = await _repository.TraderRepository.GetTraderById(traderId, trackChanges: false);
                if (trader == null)
                {
                    return ResponseFactory.Fail<TraderDetailsDto>(
                        new NotFoundException("Trader not found"),
                        "Trader not found");
                }

                var traderDto = _mapper.Map<TraderDetailsDto>(trader);
                return ResponseFactory.Success(traderDto, "Trader details retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trader details");
                return ResponseFactory.Fail<TraderDetailsDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<MarketDetailsDto>> GetMarketDetails(string marketId)
        {
            try
            {
                var market = await _repository.MarketRepository.GetMarketByIdAsync(marketId, trackChanges: false);
                if (market == null)
                {
                    return ResponseFactory.Fail<MarketDetailsDto>(
                        new NotFoundException("Market not found"),
                        "Market not found");
                }

                var marketDto = _mapper.Map<MarketDetailsDto>(market);
                return ResponseFactory.Success(marketDto, "Market details retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving market details");
                return ResponseFactory.Fail<MarketDetailsDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<IEnumerable<MarketResponseDto>>> SearchMarkets(string searchTerm)
        {
            try
            {
                var paginationFilter = new PaginationFilter { PageNumber = 1, PageSize = 100 }; // Default values
                var searchResults = await _repository.MarketRepository.SearchMarket(searchTerm, paginationFilter);
                var marketDtos = _mapper.Map<IEnumerable<MarketResponseDto>>(searchResults.PageItems);

                return ResponseFactory.Success(marketDtos, "Markets search completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching markets");
                return ResponseFactory.Fail<IEnumerable<MarketResponseDto>>(ex, "An unexpected error occurred");
            }
        }

    }

}




