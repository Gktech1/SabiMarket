
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
using SabiMarket.Domain.Enum;
using SabiMarket.Infrastructure.Utilities;
using SabiMarket.Domain.Entities;
using Microsoft.AspNetCore.Http;
using ValidationException = FluentValidation.ValidationException;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Domain.DTOs;
using System.Linq.Expressions;
using SabiMarket.Application.Interfaces;
using CloudinaryDotNet.Actions;

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
        private readonly IValidator<CreateMarketRequestDto> _createMarketValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

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
            ICurrentUserService currentUser,
            IHttpContextAccessor httpContextAccessor,
            IValidator<CreateMarketRequestDto> createMarketValidator,
            IValidator<CreateAssistantOfficerRequestDto> createAssistOfficerValidator,
            ApplicationDbContext context,
            ICloudinaryService cloudinaryService = null)
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
            _httpContextAccessor = httpContextAccessor;
            _createMarketValidator = createMarketValidator;
            _createAssistOfficerValidator = createAssistOfficerValidator;
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        private string GetCurrentIpAddress()
        {
            return _httpContextAccessor.GetRemoteIPAddress();
        }
        private async Task CreateAuditLog(string activity, string details, string module = "Chairman Management")
        {
            var userId = _currentUser.GetUserId();
            if (userId == null)
            {
                return;
            }
            var auditLog = new AuditLog
            {
                UserId = userId ?? "",
                Activity = activity,
                Module = module,
                Details = details,
                IpAddress = GetCurrentIpAddress()
            };
            auditLog.SetDateTime(DateTime.UtcNow);

            _repository.AuditLogRepository.Create(auditLog);
            await _repository.SaveChangesAsync();
        }


        public async Task<BaseResponse<LocalGovernmentWithUsersResponseDto>> GetLocalGovernmentWithUsersByUserId(string userId)
        {
            try
            {
                // First find the user
                //var user = await _userManager.FindByIdAsync(userId);
                var user =  await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    await CreateAuditLog(
                        "User Lookup Failed",
                        $"Failed to find user with ID: {userId}",
                        "User Query"
                    );
                    return ResponseFactory.Fail<LocalGovernmentWithUsersResponseDto>(
                        new NotFoundException("User not found"),
                        "User not found");
                }

                // Check if user has an associated local government
                if (string.IsNullOrEmpty(user.LocalGovernmentId))
                {
                    await CreateAuditLog(
                        "LocalGovernment Lookup Failed",
                        $"User with ID: {userId} has no associated LocalGovernment",
                        "LocalGovernment Query"
                    );
                    return ResponseFactory.Fail<LocalGovernmentWithUsersResponseDto>(
                        new NotFoundException("User has no associated LocalGovernment"),
                        "User has no associated LocalGovernment");
                }

                // Get the local government with users
                var localGovernment = await _repository.LocalGovernmentRepository.GetLocalGovernmentWithUsers(user.LocalGovernmentId, trackChanges: false);
                if (localGovernment == null)
                {
                    await CreateAuditLog(
                        "LocalGovernment Lookup Failed",
                        $"Failed to find local government with ID: {user.LocalGovernmentId}",
                        "LocalGovernment Query"
                    );
                    return ResponseFactory.Fail<LocalGovernmentWithUsersResponseDto>(
                        new NotFoundException("LocalGovernment not found"),
                        "LocalGovernment not found");
                }

                //var localGovernmentDto = _mapper.Map<LocalGovernmentWithUsersResponseDto>(localGovernment);
                var localGovernmentDto = _mapper.Map<LocalGovernmentWithUsersResponseDto>((user, localGovernment));
                await CreateAuditLog(
                    "LocalGovernment Lookup",
                    $"Retrieved local government with users for user ID: {userId}",
                    "LocalGovernment Query"
                );
                return ResponseFactory.Success(localGovernmentDto, "LocalGovernment with users retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving local government with users by user ID");
                return ResponseFactory.Fail<LocalGovernmentWithUsersResponseDto>(ex, "An unexpected error occurred");
            }
        }

            public async Task<BaseResponse<PaginatorDto<IEnumerable<LGResponseDto>>>> GetLocalGovernmentAreas(
       string searchTerm,
       PaginationFilter paginationFilter)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "LGA List Query",
                    $"CorrelationId: {correlationId} - Retrieving LGAs with search term: {searchTerm}",
                    "LGA Management"
                );

                var result = await _repository.LocalGovernmentRepository.GetLocalGovernmentArea(
                    searchTerm,
                    paginationFilter);

                // The mapping can stay the same since the repository is returning the same structure
                var lgaDtos = result.PageItems.Select(lga => new LGResponseDto
                {
                    Id = lga.Id,
                    LocalGovernmentArea = lga.Name,
                    LGAChairman = lga.LGA // Now we can directly use LGA since it contains the chairman name
                });

                var paginatedResult = new PaginatorDto<IEnumerable<LGResponseDto>>
                {
                    PageItems = lgaDtos,
                    PageSize = result.PageSize,
                    CurrentPage = result.CurrentPage,
                    NumberOfPages = result.NumberOfPages
                };

                await CreateAuditLog(
                    "LGA List Retrieved",
                    $"CorrelationId: {correlationId} - Successfully retrieved {lgaDtos.Count()} LGAs",
                    "LGA Management"
                );

                return ResponseFactory.Success(paginatedResult, "Local Government Areas retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "LGA List Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "LGA Management"
                );
                _logger.LogError(ex, "Error retrieving LGAs: {ErrorMessage}", ex.Message);
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<LGResponseDto>>>(ex,
                    "An unexpected error occurred while retrieving Local Government Areas");
            }
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>> GetLocalGovernments(
      LGAFilterRequestDto filterDto,
      PaginationFilter paginationFilter)
        {
            try
            {
                // Get the filtered query with AsNoTracking
                var query = _repository.LocalGovernmentRepository
                    .GetFilteredLGAsQuery(filterDto)
                    .AsNoTracking();  // Ensure we're not tracking entities

                // Execute pagination in a single database round trip
                var paginatedLGAs = await query.Paginate(paginationFilter);

                // Map the results after they're materialized
                var lgaDtos = paginatedLGAs.PageItems.Select(lga => _mapper.Map<LGAResponseDto>(lga));

                var result = new PaginatorDto<IEnumerable<LGAResponseDto>>
                {
                    PageItems = lgaDtos,
                    PageSize = paginatedLGAs.PageSize,
                    CurrentPage = paginatedLGAs.CurrentPage,
                    NumberOfPages = paginatedLGAs.NumberOfPages
                };

                await CreateAuditLog(
                    "LGA List Query",
                    $"Retrieved LGA list - Page {paginationFilter.PageNumber}, " +
                    $"Size {paginationFilter.PageSize}, Filters: {JsonSerializer.Serialize(filterDto)}",
                    "LGA Query"
                );

                return ResponseFactory.Success(result, "LGAs retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving LGAs: {ErrorMessage}", ex.Message);
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<LGAResponseDto>>>(
                    ex, "An unexpected error occurred while retrieving LGAs");
            }
        }

        public async Task<BaseResponse<LGAResponseDto>> GetLocalGovernmentById(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return ResponseFactory.Fail<LGAResponseDto>(
                        new BadRequestException("Local Government ID is required"),
                        "Invalid ID provided");
                }

                var localGovernment = await _repository.LocalGovernmentRepository
                    .GetLocalGovernmentById(id, trackChanges: false);

                if (localGovernment == null)
                {
                    return ResponseFactory.Fail<LGAResponseDto>(
                        new NotFoundException($"Local Government with ID {id} was not found"),
                        "Local Government not found");
                }

                var lgaDto = _mapper.Map<LGAResponseDto>(localGovernment);

                await CreateAuditLog(
                    "LGA Details Query",
                    $"Retrieved LGA details for ID: {id}",
                    "LGA Query"
                );

                return ResponseFactory.Success(lgaDto, "Local Government retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Local Government with ID {Id}: {ErrorMessage}",
                    id, ex.Message);

                return ResponseFactory.Fail<LGAResponseDto>(
                    ex, "An unexpected error occurred while retrieving the Local Government");
            }
        }
        public async Task<BaseResponse<ChairmanResponseDto>> GetChairmanById(string chairmanId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Chairman Details Query",
                    $"CorrelationId: {correlationId} - Fetching chairman details for ID: {chairmanId}",
                    "Chairman Management"
                );

                var result = await _repository.ChairmanRepository.GetChairmanById(chairmanId, trackChanges: false);

                if (result is null)
                {
                    await CreateAuditLog(
                    "Chairman Details Query",
                    $"CorrelationId: {correlationId} - Cahirman not found for ID: {chairmanId}",
                    "Chairman Management"
                );
                    return ResponseFactory.Success(_mapper.Map<ChairmanResponseDto>(result), "Chairman not found");
                }

                await CreateAuditLog(
                    "Chairman Details Retrieved",
                    $"CorrelationId: {correlationId} - Chairman details retrieved successfully",
                    "Chairman Management"
                );

                return ResponseFactory.Success(_mapper.Map<ChairmanResponseDto>(result), "Chairman retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Chairman Details Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Chairman Management"
                );
                return ResponseFactory.Fail<ChairmanResponseDto>(ex, "Error retrieving chairman");
            }
        }

        public async Task<BaseResponse<AdminDashboardResponse>> GetChairmen(
    string? searchTerm, PaginationFilter paginationFilter)
        {
            var correlationId = Guid.NewGuid().ToString();
            var currentUserId = _currentUser.GetUserId();
            try
            {
                await CreateAuditLog(
                    "Chairmen List Query",
                    $"CorrelationId: {correlationId} - Retrieving chairmen list - Page: {paginationFilter.PageNumber}, Size: {paginationFilter.PageSize}",
                    "Chairman Management"
                );

                // Fetch the paginated chairmen
                var chairmenPage = await _repository.ChairmanRepository
                    .GetChairmenWithPaginationAsync(paginationFilter, trackChanges: false, searchTerm);

                // Log the number of chairmen retrieved
                await CreateAuditLog(
                    "Chairmen List Retrieved",
                    $"CorrelationId: {correlationId} - Retrieved {chairmenPage.PageItems.Count()} chairmen",
                    "Chairman Management"
                );

                // Map the chairmen entities to response DTOs
                var chairmanDtos = chairmenPage.PageItems.Select(c => new ChairmanResponseDto
                {
                    Id = c.UserId,
                    FullName = string.IsNullOrEmpty(c.FullName)
                        ? (c.User != null ? $"{c.User.FirstName} {c.User.LastName}" : string.Empty)
                        : c.FullName,
                    Email = c.User != null ? c.User.Email ?? string.Empty : string.Empty,
                    PhoneNumber = c.User != null ? c.User.PhoneNumber ?? string.Empty : string.Empty,
                    MarketName = c.Market != null ? c.Market.MarketName : string.Empty,
                    LocalGovernmentName = c.LocalGovernment != null ? c.LocalGovernment.Name : string.Empty,
                    IsActive = c.User != null && c.User.IsActive,
                    MarketId = c.MarketId,
                    LocalGovernmentId = c.LocalGovernmentId,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    DefaultPassword = null  // Explicitly setting it to null if not needed
                }).ToList();

                // Get current admin to fetch dashboard stats
                var admin = await _repository.AdminRepository.GetAdminByUserIdAsync(currentUserId, trackChanges: false);

                if (admin != null)
                {
                    // Update the last dashboard access timestamp
                    admin.LastDashboardAccess = DateTime.UtcNow;
                    _repository.AdminRepository.UpdateAdmin(admin);

                    // Also refresh the stats if they're more than an hour old
                    if (admin.StatsLastUpdatedAt < DateTime.UtcNow.AddHours(-1))
                    {
                        // Get fresh stats
                        int registeredLGAs = await _repository.LocalGovernmentRepository.CountAsync();
                        int activeChairmen = await _repository.ChairmanRepository.CountAsync(c => c.User != null && c.User.IsActive);
                        decimal totalRevenue = await _repository.LevyPaymentRepository.GetTotalRevenueAsync();

                        // Update admin stats
                        await _repository.AdminRepository.UpdateAdminStatsAsync(
                            admin.UserId,
                            registeredLGAs,
                            activeChairmen,
                            totalRevenue
                        );
                    }

                    await _repository.SaveChangesAsync();
                }

                // Create the dashboard metrics from the admin entity
                var metrics = new DashboardMetrics
                {
                    RegisteredLGAs = admin?.RegisteredLGAs ?? 0,
                    ActiveChairmen = admin?.ActiveChairmen ?? 0,
                    TotalRevenue = admin?.TotalRevenue ?? 0
                };

                // Create and return the comprehensive response
                var paginatedChairmen = new PaginatorDto<IEnumerable<ChairmanResponseDto>>
                {
                    PageItems = chairmanDtos,
                    CurrentPage = chairmenPage.CurrentPage,
                    PageSize = chairmenPage.PageSize,
                    NumberOfPages = chairmenPage.NumberOfPages
                };

                var dashboardResponse = new AdminDashboardResponse
                {
                    Chairmen = paginatedChairmen,
                    Metrics = metrics
                };

                return ResponseFactory.Success(dashboardResponse, "Chairmen and dashboard metrics retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Chairmen List Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Chairman Management"
                );
                return ResponseFactory.Fail<AdminDashboardResponse>(ex, "Error retrieving chairmen and metrics");
            }
        }

        /*    public async Task<BaseResponse<PaginatorDto<IEnumerable<ChairmanResponseDto>>>> GetChairmen(
        string? searchTerm, PaginationFilter paginationFilter)
            {
                var correlationId = Guid.NewGuid().ToString();
                try
                {
                    await CreateAuditLog(
                        "Chairmen List Query",
                        $"CorrelationId: {correlationId} - Retrieving chairmen list - Page: {paginationFilter.PageNumber}, Size: {paginationFilter.PageSize}",
                        "Chairman Management"
                    );

                    // Fetch the paginated chairmen
                    var chairmenPage = await _repository.ChairmanRepository
                        .GetChairmenWithPaginationAsync(paginationFilter, trackChanges: false, searchTerm);

                    // Log the number of chairmen retrieved
                    await CreateAuditLog(
                        "Chairmen List Retrieved",
                        $"CorrelationId: {correlationId} - Retrieved {chairmenPage.PageItems.Count()} chairmen",
                        "Chairman Management"
                    );

                    // Map the chairmen entities to response DTOs
                    //var chairmanDtos = _mapper.Map<IEnumerable<ChairmanResponseDto>>(chairmenPage.PageItems);

                    var chairmanDtos = chairmenPage.PageItems.Select(c => new ChairmanResponseDto
                    {
                        Id = c.Id,
                        FullName = string.IsNullOrEmpty(c.FullName)
                        ? (c.User != null ? $"{c.User.FirstName} {c.User.LastName}" : string.Empty)
                        : c.FullName,
                                    Email = c.User != null ? c.User.Email ?? string.Empty : string.Empty,
                        PhoneNumber = c.User != null ? c.User.PhoneNumber ?? string.Empty : string.Empty,
                        MarketName = c.Market != null ? c.Market.MarketName : string.Empty,
                        IsActive = c.User != null && c.User.IsActive,
                        MarketId = c.MarketId,
                        LocalGovernmentId = c.LocalGovernmentId,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        DefaultPassword = null  // Explicitly setting it to null if not needed
                    }).ToList();


                    // Create and return the paginated response
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
                    await CreateAuditLog(
                        "Chairmen List Query Failed",
                        $"CorrelationId: {correlationId} - Error: {ex.Message}",
                        "Chairman Management"
                    );
                    return ResponseFactory.Fail<PaginatorDto<IEnumerable<ChairmanResponseDto>>>(ex, "Error retrieving chairmen");
                }
            }
    */

        /*public async Task<BaseResponse<PaginatorDto<IEnumerable<ChairmanResponseDto>>>> GetChairmen(string? searchTerm, PaginationFilter paginationFilter)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Chairmen List Query",
                    $"CorrelationId: {correlationId} - Retrieving chairmen list - Page: {paginationFilter.PageNumber}, Size: {paginationFilter.PageSize}",
                    "Chairman Management"
                );

                var chairmenPage = await _repository.ChairmanRepository.GetChairmenWithPaginationAsync(paginationFilter, false, searchTerm);

                await CreateAuditLog(
                    "Chairmen List Retrieved",
                    $"CorrelationId: {correlationId} - Retrieved {chairmenPage.PageItems.Count()} chairmen",
                    "Chairman Management"
                );

                return ResponseFactory.Success(new PaginatorDto<IEnumerable<ChairmanResponseDto>>
                {
                    PageItems = _mapper.Map<IEnumerable<ChairmanResponseDto>>(chairmenPage.PageItems),
                    CurrentPage = chairmenPage.CurrentPage,
                    PageSize = chairmenPage.PageSize,
                    NumberOfPages = chairmenPage.NumberOfPages
                }, "Chairmen retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Chairmen List Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Chairman Management"
                );
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<ChairmanResponseDto>>>(ex, "Error retrieving chairmen");
            }
        }*/

        public async Task<BaseResponse<bool>> AssignCaretakerToChairman(string chairmanId, string caretakerId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Caretaker Assignment",
                    $"CorrelationId: {correlationId} - Assigning caretaker {caretakerId} to chairman {chairmanId}",
                    "Chairman Management"
                );

                var chairman = await _repository.ChairmanRepository.GetChairmanByIdAsync(chairmanId, true);
                var caretaker = await _repository.CaretakerRepository.GetCaretakerById(caretakerId, true);

                caretaker.ChairmanId = chairmanId;
                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Caretaker Assigned",
                    $"CorrelationId: {correlationId} - Successfully assigned caretaker to chairman",
                    "Chairman Management"
                );

                return ResponseFactory.Success(true, "Caretaker assigned successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Caretaker Assignment Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Chairman Management"
                );
                return ResponseFactory.Fail<bool>(ex, "Error assigning caretaker");
            }
        }

        public async Task<BaseResponse<DashboardMetricsResponseDto>> GetDashboardMetrics()
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Dashboard Metrics Query",
                    $"CorrelationId: {correlationId} - Fetching dashboard metrics",
                    "Dashboard Analytics"
                );

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

                await CreateAuditLog(
                   "Dashboard Metrics Retrieved",
                   $"CorrelationId: {correlationId} - Successfully retrieved dashboard metrics",
                   "Dashboard Analytics"
               );

                return ResponseFactory.Success(response, "Dashboard metrics retrieved successfully");

            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Dashboard Metrics Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Dashboard Analytics"
                );
                return ResponseFactory.Fail<DashboardMetricsResponseDto>(ex, "Error retrieving dashboard metrics");
            }
        }

        public async Task<BaseResponse<ChairmanDashboardStatsDto>> GetChairmanDashboardStats(string chairmanId)
        {
            try
            {
                // First get the chairman to check existence and update access time
                var chairman = await _repository.ChairmanRepository.GetChairmanByChairmanIdId(chairmanId, trackChanges: true);
                if (chairman == null)
                {
                    await CreateAuditLog(
                        "Dashboard Access Failed",
                        $"Chairman not found for ID: {chairmanId}",
                        "Dashboard Access"
                    );
                    return ResponseFactory.Fail<ChairmanDashboardStatsDto>(
                        new NotFoundException("Chairman not found"),
                        "Chairman not found");
                }

                // Update last dashboard access
                chairman.UpdatedAt = DateTime.UtcNow;
                _repository.ChairmanRepository.UpdateChairman(chairman);

                // Get dashboard statistics - now correctly returning ChairmanDashboardStatsDto
                var statsDto = await _repository.ChairmanRepository.GetChairmanDashboardStatsAsync(chairmanId);

                await CreateAuditLog(
                    "Dashboard Access",
                    $"Retrieved dashboard stats for chairman ID: {chairmanId}",
                    "Dashboard Access"
                );

                await _repository.SaveChangesAsync();
                return ResponseFactory.Success(statsDto, "Dashboard stats retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chairman dashboard stats");
                return ResponseFactory.Fail<ChairmanDashboardStatsDto>(ex, "An unexpected error occurred");
            }
        }

        /// <summary>
        /// Searches for levy payments in a chairman's market
        /// </summary>
        /// <param name="chairmanId">The ID of the chairman</param>
        /// <param name="searchQuery">The search query string</param>
        /// <param name="paginationFilter">Pagination parameters</param>
        /// <returns>A paginated list of levy payments matching the search criteria</returns>

        public async Task<BaseResponse<PaginatorDto<IEnumerable<LevyPaymentDetailDto>>>> SearchLevyPayments(
    string chairmanId,
    string searchQuery,
    PaginationFilter paginationFilter)
        {
            try
            {
                // First get the chairman to check existence
                var chairman = await _repository.ChairmanRepository.GetChairmanByChairmanIdId(chairmanId, trackChanges: false);
                if (chairman == null)
                {
                    await CreateAuditLog(
                        "Levy Search Failed",
                        $"Chairman not found for ID: {chairmanId}",
                        "Levy Management"
                    );
                    return ResponseFactory.Fail<PaginatorDto<IEnumerable<LevyPaymentDetailDto>>>(
                        new NotFoundException("Chairman not found"),
                        "Chairman not found");
                }

                var marketId = chairman.MarketId;
                if (string.IsNullOrEmpty(marketId))
                {
                    return ResponseFactory.Fail<PaginatorDto<IEnumerable<LevyPaymentDetailDto>>>(
                        new BadRequestException("Chairman is not associated with any market"),
                        "Chairman is not associated with any market");
                }

                // Use repository method to search
                var searchResult = await _repository.LevyPaymentRepository.SearchLevyPaymentsInMarket(
                    marketId,
                    searchQuery,
                    paginationFilter,
                    trackChanges: false
                );

                // Create DTO list from search results
                var dtoList = searchResult.PageItems.Select(l => new LevyPaymentDetailDto
                {
                    PaymentId = l.Id,
                    AmountPaid = l.Amount,
                    PaidBy = l.Trader != null ? l.Trader.User.FirstName + " " + l.Trader.User.LastName :
                            l.GoodBoy != null ? l.GoodBoy.User.FirstName + " " + l.GoodBoy.User.LastName : "Unknown",
                    PaymentDate = l.PaymentDate,
                    PaymentMethod = l.PaymentMethod.ToString(),
                    PaymentStatus = l.PaymentStatus
                }).ToList();

                // Create new PaginatorDto with the properties that exist in your class
                var resultDto = new PaginatorDto<IEnumerable<LevyPaymentDetailDto>>
                {
                    PageItems = dtoList,
                    PageSize = searchResult.PageSize,
                    CurrentPage = searchResult.CurrentPage,
                    NumberOfPages = searchResult.NumberOfPages
                };

                await CreateAuditLog(
                    "Levy Search",
                    $"Searched levy payments for chairman ID: {chairmanId}, Query: {searchQuery}",
                    "Levy Management"
                );

                return ResponseFactory.Success(resultDto, "Levy payments retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching levy payments: {Message}", ex.Message);
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<LevyPaymentDetailDto>>>(ex, "An unexpected error occurred");
            }
        }

        /*   public async Task<BaseResponse<ChairmanDashboardStatsDto>> GetChairmanDashboardStats(string chairmanId)
           {
               try
               {
                   var chairman = await _repository.ChairmanRepository.GetChairmanByIdAsync(chairmanId, trackChanges: true);
                   if (chairman == null)
                   {
                       await CreateAuditLog(
                           "Dashboard Access Failed",
                           $"Chairman not found for ID: {chairmanId}",
                           "Dashboard Access"
                       );
                       return ResponseFactory.Fail<ChairmanDashboardStatsDto>(
                           new NotFoundException("Chairman not found"),
                           "Chairman not found");
                   }

                   // Update last dashboard access
                   chairman.UpdatedAt = DateTime.UtcNow;
                   _repository.ChairmanRepository.UpdateChairman(chairman);

                   // Get dashboard statistics
                   var stats = await _repository.ChairmanRepository.GetChairmanDashboardStatsAsync(chairmanId);
                   var statsDto = _mapper.Map<ChairmanDashboardStatsDto>(stats);

                   await CreateAuditLog(
                       "Dashboard Access",
                       $"Retrieved dashboard stats for chairman ID: {chairmanId}",
                       "Dashboard Access"
                   );

                   await _repository.SaveChangesAsync();
                   return ResponseFactory.Success(statsDto, "Dashboard stats retrieved successfully");
               }
               catch (Exception ex)
               {
                   _logger.LogError(ex, "Error retrieving chairman dashboard stats");
                   return ResponseFactory.Fail<ChairmanDashboardStatsDto>(ex, "An unexpected error occurred");
               }
           }
   */

        public async Task<BaseResponse<MarketResponseDto>> CreateMarket(CreateMarketRequestDto request)
        {
            var correlationId = Guid.NewGuid().ToString();
            var userId = _currentUser.GetUserId();

            try
            {
                // Validate request
                var validationResult = await _createMarketValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Fail<MarketResponseDto>(
                        new ValidationException(validationResult.Errors),
                        "Invalid market data"
                    );
                }

                // Map request to Market entity
                var market = _mapper.Map<Market>(request);

                // Verify Caretaker exists if provided
                if (!string.IsNullOrWhiteSpace(request.CaretakerId) && request.CaretakerId != "string")
                {
                    var caretaker = await _repository.CaretakerRepository.GetCaretakerById(request.CaretakerId, false);
                    if (caretaker == null)
                    {
                        return ResponseFactory.Fail<MarketResponseDto>(
                            new NotFoundException("Caretaker not found"),
                            "Invalid caretaker"
                        );
                    }
                    market.CaretakerId = caretaker.Id;
                }
                else
                {
                    market.CaretakerId = null;
                }

                // Get Chairman details
                var chairman = await _repository.ChairmanRepository.GetChairmanById(userId, false);
                if (chairman == null)
                {
                    return ResponseFactory.Fail<MarketResponseDto>(
                        new NotFoundException("Chairman not found"),
                        "Invalid chairman"
                    );
                }

                // Ensure Chairman has a Local Government
                var localGovernment = await _repository.LocalGovernmentRepository.GetLocalGovernmentById(chairman.LocalGovernmentId, false);
                if (localGovernment == null)
                {
                    return ResponseFactory.Fail<MarketResponseDto>(
                        new NotFoundException("Local Government not found"),
                        "Invalid Local Government"
                    );
                }

                // Ensure chairman has a Caretaker 
                var caretakerbyLGAid = await _repository.CaretakerRepository.GetCaretakerByLocalGovernmentId(chairman.LocalGovernmentId, false);
                /*  if (caretakerbyLGAid == null)
                  {
                      caretakerbyLGAid.Id = null;
                  }
  */


                // Log market creation attempt
                await CreateAuditLog(
                    "Market Creation",
                    $"CorrelationId: {correlationId} - Creating new market: {request.MarketName}",
                    "Market Management"
                );

                // Set Market properties
                market.Id = Guid.NewGuid().ToString();
                market.IsActive = true;
                market.MarketName = request.MarketName;
                market.Location = request.MarketName;
                market.LocalGovernmentId = chairman.LocalGovernmentId;
                market.LocalGovernmentName = localGovernment.Name; // Ensure this is correctly assigned
                market.StartDate = DateTime.UtcNow;
                market.MarketCapacity = 0;
                market.ChairmanId = chairman.Id;
                market.CaretakerId = caretakerbyLGAid?.Id! ?? null;



                // Save Market
                _repository.MarketRepository.AddMarket(market);
                await _repository.SaveChangesAsync();

                // Log success
                await CreateAuditLog(
                    "Market Created",
                    $"CorrelationId: {correlationId} - Market created successfully with ID: {market.Id}",
                    "Market Management"
                );

                return ResponseFactory.Success(_mapper.Map<MarketResponseDto>(market), "Market created successfully");
            }
            catch (Exception ex)
            {
                // Log failure
                await CreateAuditLog(
                    "Market Creation Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Market Management"
                );
                return ResponseFactory.Fail<MarketResponseDto>(ex, "Error creating market");
            }
        }

        public async Task<BaseResponse<bool>> UpdateMarket(string marketId, UpdateMarketRequestDto request)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                // Validate request
                /*  var validationResult = await _updateValidator.ValidateAsync(request);
                  if (!validationResult.IsValid)
                  {
                      return ResponseFactory.Fail<bool>(
                          new ValidationException(validationResult.Errors),
                          "Invalid market data"
                      );
                  }*/

                // Verify market exists
                var market = await _repository.MarketRepository.GetMarketByIdAsync(marketId, true);
                if (market == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Market not found"),
                        "Invalid market ID"
                    );
                }

                // Verify caretaker exists
                var caretaker = await _repository.CaretakerRepository.GetCaretakerById(request.CaretakerId, false);
                if (caretaker == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Caretaker not found"),
                        "Invalid caretaker"
                    );
                }

                await CreateAuditLog(
                    "Market Update",
                    $"CorrelationId: {correlationId} - Updating market {marketId}",
                    "Market Management"
                );

                // Update only the fields shown in UI
                market.MarketName = request.MarketName;
                market.MarketType = request.MarketType.ToString();
                market.CaretakerId = request.CaretakerId;
                market.UpdatedAt = DateTime.UtcNow;

                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Market Updated",
                    $"CorrelationId: {correlationId} - Market updated successfully",
                    "Market Management"
                );

                return ResponseFactory.Success(true, "Market updated successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Market Update Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Market Management"
                );
                return ResponseFactory.Fail<bool>(ex, "Error updating market");
            }
        }
        /*   public async Task<BaseResponse<MarketDetailsDto>> GetMarketDetails(string marketId)
           {
               var correlationId = Guid.NewGuid().ToString();
               try
               {
                   await CreateAuditLog(
                       "Market Details Query",
                       $"CorrelationId: {correlationId} - Fetching market details for ID: {marketId}",
                       "Market Management"
                   );
                   var market = await _repository.MarketRepository.GetMarketByIdAsync(marketId, trackChanges: false);
                   await CreateAuditLog(
                       "Market Details Retrieved",
                       $"CorrelationId: {correlationId} - Market details retrieved successfully",
                       "Market Management"
                   );

                   var dto = _mapper.Map<MarketDetailsDto>(market);

                   // Manually fix the trader names after mapping
                   if (dto.Traders != null && market.Traders != null)
                   {
                       var traderMap = market.Traders.ToDictionary(t => t.Id);

                       foreach (var traderDto in dto.Traders)
                       {
                           if (traderMap.TryGetValue(traderDto.Id, out var trader) && trader.User != null)
                           {
                               traderDto.FullName = $"{trader.User.FirstName} {trader.User.LastName}".Trim();
                               traderDto.Email = trader.User.Email;
                               traderDto.PhoneNumber = trader.User.PhoneNumber;
                               traderDto.Gender = trader.User.Gender;
                           }
                       }
                   }

                   return ResponseFactory.Success(dto, "Market details retrieved successfully");
                  // return ResponseFactory.Success(_mapper.Map<MarketDetailsDto>(market), "Market details retrieved successfully");
               }
               catch (Exception ex)
               {
                   await CreateAuditLog(
                       "Market Details Query Failed",
                       $"CorrelationId: {correlationId} - Error: {ex.Message}",
                       "Market Management"
                   );
                   return ResponseFactory.Fail<MarketDetailsDto>(ex, "Error retrieving market details");
               }
           }*/

        public async Task<BaseResponse<MarketDetailsDto>> GetMarketDetails(string marketId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Market Details Query",
                    $"CorrelationId: {correlationId} - Fetching market details for ID: {marketId}",
                    "Market Management"
                );
                var market = await _repository.MarketRepository.GetMarketByIdAsync(marketId, trackChanges: false);
                await CreateAuditLog(
                    "Market Details Retrieved",
                    $"CorrelationId: {correlationId} - Market details retrieved successfully",
                    "Market Management"
                );
                var dto = _mapper.Map<MarketDetailsDto>(market);

                // Manually fix the trader names after mapping
                if (dto.Traders != null && market.Traders != null)
                {
                    var traderMap = market.Traders.ToDictionary(t => t.Id);
                    foreach (var traderDto in dto.Traders)
                    {
                        if (traderMap.TryGetValue(traderDto.Id, out var trader) && trader.User != null)
                        {
                            traderDto.FullName = $"{trader.User.FirstName} {trader.User.LastName}".Trim();
                            traderDto.Email = trader.User.Email;
                            traderDto.PhoneNumber = trader.User.PhoneNumber;
                            traderDto.Gender = trader.User.Gender;
                        }
                    }
                }

                // Manually fix the caretaker names after mapping
                if (dto.Caretakers != null && market.AdditionalCaretakers != null)
                {
                    var caretakerMap = market.AdditionalCaretakers.ToDictionary(c => c.Id);
                    foreach (var caretakerDto in dto.Caretakers)
                    {
                        if (caretakerMap.TryGetValue(caretakerDto.Id, out var caretaker) && caretaker.User != null)
                        {
                            caretakerDto.FirstName = caretaker.User.FirstName ?? "Default";
                            caretakerDto.LastName = caretaker.User.LastName ?? "User";
                            caretakerDto.Email = caretaker.User.Email;
                            caretakerDto.PhoneNumber = caretaker.User.PhoneNumber;
                            caretakerDto.ProfileImageUrl = caretaker.User.ProfileImageUrl ?? "";
                            caretakerDto.IsActive = caretaker.User.IsActive;
                        }
                    }
                }

                return ResponseFactory.Success(dto, "Market details retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Market Details Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Market Management"
                );
                return ResponseFactory.Fail<MarketDetailsDto>(ex, "Error retrieving market details");
            }
        }
        public async Task<BaseResponse<bool>> DeleteLevy(string levyId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Levy Deletion",
                    $"CorrelationId: {correlationId} - Attempting to delete levy: {levyId}",
                    "Levy Management"
                );

                var levy = await _repository.LevyPaymentRepository.GetPaymentById(levyId, true);
                _repository.LevyPaymentRepository.DeleteLevyPayment(levy);
                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Levy Deleted",
                    $"CorrelationId: {correlationId} - Levy deleted successfully",
                    "Levy Management"
                );

                return ResponseFactory.Success(true, "Levy deleted successfully.");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Levy Deletion Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Levy Management"
                );
                return ResponseFactory.Fail<bool>(ex, "Error deleting levy");
            }
        }

        public async Task<BaseResponse<PaginatorDto<List<AssistOfficerListDto>>>> GetAssistOfficers(
              PaginationFilter pagination,
              string searchTerm = "",
              string status = "Active")
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Assist Officers Query",
                    $"CorrelationId: {correlationId} - Fetching assist officers with filters: Page {pagination.PageNumber}, Size {pagination.PageSize}, Search '{searchTerm}', Status '{status}'",
                    "Officer Management"
                );

                // Build the filter expression for status
                Expression<Func<AssistCenterOfficer, bool>> statusExpression;
                if (status == "All")
                {
                    statusExpression = o => true; // Include all records
                }
                else
                {
                    bool isActive = status == "Active";
                    statusExpression = o => o.IsBlocked != isActive;
                }

                // Use repository method for searching or regular query
                PaginatorDto<IEnumerable<AssistCenterOfficer>> officersPaginated;
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    // Use the search method from repository
                    officersPaginated = await _repository.AssistCenterOfficerRepository.SearchAssistOfficersAsync(
                        statusExpression,
                        searchTerm,
                        pagination,
                        trackChanges: false
                    );
                }
                else
                {
                    // Use the regular query method
                    officersPaginated = await _repository.AssistCenterOfficerRepository.GetAssistOfficersAsync(
                        statusExpression,
                        pagination,
                        trackChanges: false
                    );
                }

                // Map entities to DTOs
                var officerDtos = _mapper.Map<List<AssistOfficerListDto>>(officersPaginated.PageItems);

                // Manually fix the officer names and additional info after mapping
                int index = 0;
                foreach (var officer in officersPaginated.PageItems)
                {
                    if (officer.User != null)
                    {
                        officerDtos[index].FullName = $"{officer.User?.FirstName} {officer.User?.LastName}".Trim();
                        officerDtos[index].Email = officer.User?.Email;
                        officerDtos[index].PhoneNumber = officer.User?.PhoneNumber;

                        // Set market name if available
                        if (officer.Market != null)
                        {
                            officerDtos[index].MarketName = officer.Market?.MarketName;
                        }

                        // Set local government name if available
                        if (officer.LocalGovernment != null)
                        {
                            officerDtos[index].LocalGovernmentName = officer.LocalGovernment?.Name;
                        }
                    }
                    index++;
                }

                // Create final paginated result with DTOs
                var paginatedResult = new PaginatorDto<List<AssistOfficerListDto>>
                {
                    PageItems = officerDtos,
                    CurrentPage = officersPaginated.CurrentPage,
                    PageSize = officersPaginated.PageSize,
                    NumberOfPages = officersPaginated.NumberOfPages
                };

                await CreateAuditLog(
                    "Assist Officers Retrieved",
                    $"CorrelationId: {correlationId} - {officerDtos.Count} assist officers retrieved successfully",
                    "Officer Management"
                );

                return ResponseFactory.Success(paginatedResult, "Assist officers retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Assist Officers Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Officer Management"
                );
                return ResponseFactory.Fail<PaginatorDto<List<AssistOfficerListDto>>>(ex, "Error retrieving assist officers");
            }
        }/*
        public async Task<BaseResponse<bool>> BlockAssistantOfficer(string officerId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Assistant Officer Block",
                    $"CorrelationId: {correlationId} - Attempting to block officer: {officerId}",
                    "Officer Management"
                );

                var officer = await _repository.AssistCenterOfficerRepository.GetByIdAsync(officerId, true);
                officer.IsBlocked = true;
                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Assistant Officer Blocked",
                    $"CorrelationId: {correlationId} - Officer blocked successfully",
                    "Officer Management"
                );

                return ResponseFactory.Success(true, "Assistant Officer blocked successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Assistant Officer Block Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Officer Management"
                );
                return ResponseFactory.Fail<bool>(ex, "Error blocking Assistant Officer");
            }
        }*/

        public async Task<BaseResponse<QRCodeResponseDto>> GenerateTraderQRCode(string traderId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "QR Code Generation",
                    $"CorrelationId: {correlationId} - Generating QR code for trader: {traderId}",
                    "Trader Management"
                );

                var trader = await _repository.TraderRepository.GetTraderById(traderId, false);
                var qrData = GenerateTraderQRContent(trader);
                var qrCodeImage = QRCodeHelper.GenerateQRCode(qrData, 300, 300);

                var response = new QRCodeResponseDto
                {
                    QRCodeImage = qrCodeImage,
                    QRCodeData = qrData,
                    TraderId = trader.Id,
                    TraderName = trader.BusinessName,
                    GeneratedAt = DateTime.UtcNow
                };

                await CreateAuditLog(
                    "QR Code Generated",
                    $"CorrelationId: {correlationId} - QR code generated successfully",
                    "Trader Management"
                );

                return ResponseFactory.Success(response, "QR code generated successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "QR Code Generation Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Trader Management"
                );
                return ResponseFactory.Fail<QRCodeResponseDto>(ex, "Error generating QR code");
            }
        }

        public async Task<BaseResponse<IEnumerable<ReportResponseDto>>> GetChairmanReports(string chairmanId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Chairman Reports Query",
                    $"CorrelationId: {correlationId} - Fetching reports for chairman: {chairmanId}",
                    "Report Management"
                );

                var reports = await _repository.ChairmanRepository.GetReportsByChairmanIdAsync(chairmanId);
                var reportDtos = _mapper.Map<IEnumerable<ReportResponseDto>>(reports);

                await CreateAuditLog(
                    "Chairman Reports Retrieved",
                    $"CorrelationId: {correlationId} - Retrieved {reportDtos.Count()} reports",
                    "Report Management"
                );

                return ResponseFactory.Success(reportDtos, "Reports retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Chairman Reports Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Report Management"
                );
                return ResponseFactory.Fail<IEnumerable<ReportResponseDto>>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>> GetTraders(string marketId, PaginationFilter filter)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Traders List Query",
                    $"CorrelationId: {correlationId} - Fetching traders for market: {marketId}. Page: {filter.PageNumber}, Size: {filter.PageSize}",
                    "Trader Management"
                );

                var tradersPage = await _repository.TraderRepository.GetTradersByMarketAsync(marketId, filter, false);
                var traderDtos = _mapper.Map<IEnumerable<TraderResponseDto>>(tradersPage.PageItems);

                await CreateAuditLog(
                    "Traders List Retrieved",
                    $"CorrelationId: {correlationId} - Retrieved {traderDtos.Count()} traders",
                    "Trader Management"
                );

                return ResponseFactory.Success(new PaginatorDto<IEnumerable<TraderResponseDto>>
                {
                    PageItems = traderDtos,
                    CurrentPage = filter.PageNumber,
                    PageSize = filter.PageSize,
                    NumberOfPages = tradersPage.NumberOfPages
                }, "Traders retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Traders List Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Trader Management"
                );
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<TraderResponseDto>>>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<MarketComplianceDto>> GetMarketComplianceRates(string marketId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Market Compliance Query",
                    $"CorrelationId: {correlationId} - Fetching compliance rates for market: {marketId}",
                    "Market Analytics"
                );

                var compliance = await _repository.MarketRepository.GetComplianceRatesAsync(marketId);
                var complianceDto = _mapper.Map<MarketComplianceDto>(compliance);

                await CreateAuditLog(
                    "Market Compliance Retrieved",
                    $"CorrelationId: {correlationId} - Compliance rates retrieved successfully",
                    "Market Analytics"
                );

                return ResponseFactory.Success(complianceDto, "Market compliance rates retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Market Compliance Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Market Analytics"
                );
                return ResponseFactory.Fail<MarketComplianceDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<byte[]>> ExportReport(ReportExportRequestDto request)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Report Export",
                    $"CorrelationId: {correlationId} - Exporting report for date range: {request.StartDate} to {request.EndDate}",
                    "Report Management"
                );

                var report = await _repository.ReportRepository.ExportReport(request.StartDate, request.EndDate);
                var reportData = _mapper.Map<ReportExportDto>(report);
                var excelBytes = await ExcelExportHelper.GenerateMarketReport(reportData);

                await CreateAuditLog(
                    "Report Exported",
                    $"CorrelationId: {correlationId} - Report exported successfully",
                    "Report Management"
                );

                return ResponseFactory.Success(excelBytes, "Report exported successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Report Export Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Report Management"
                );
                return ResponseFactory.Fail<byte[]>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> ConfigureLevySetup(LevySetupRequestDto request)
        {
            var correlationId = Guid.NewGuid().ToString();
            var userId = _currentUser.GetUserId();
            try
            {
                await CreateAuditLog(
                    "Levy Setup Configuration",
                    $"CorrelationId: {correlationId} - Configuring new levy setup for {request.MarketId} ({request.MarketType})",
                    "Levy Management"
                );

                var existingLevy = await _repository.LevyPaymentRepository.GetByMarketAndOccupancyAsync(request.MarketId, request.TraderOccupancy);

                if (existingLevy != null || existingLevy.Any())
                {
                    return ResponseFactory.Fail<bool>("Levy setup already exists for this market and trader occupancy.");
                }
                var chairman = await _repository.ChairmanRepository.GetChairmanById(userId, false);
                if (chairman == null)
                {
                    return ResponseFactory.Fail<bool>("Chairman not found");
                }

                var levySetup = _mapper.Map<LevyPayment>(request);

                levySetup.ChairmanId = chairman.Id;
                levySetup.TraderId = "";
                levySetup.GoodBoyId = "";
                levySetup.Notes = "Initial Levy Setup by the Chairman";
                levySetup.TransactionReference = "";
                levySetup.QRCodeScanned = "";

                _repository.LevyPaymentRepository.AddPayment(levySetup);
                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Levy Setup Configured",
                    $"CorrelationId: {correlationId} - Levy setup configured successfully for {request.MarketId}",
                    "Levy Management"
                );

                return ResponseFactory.Success(true, "Levy setup configured successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Levy Setup Configuration Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}\nStackTrace: {ex.StackTrace}",
                    "Levy Management"
                );
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<IEnumerable<MarketResponseDto>>> SearchMarkets(string searchTerm)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Market Search",
                    $"CorrelationId: {correlationId} - Searching markets with term: {searchTerm}",
                    "Market Management"
                );

                var paginationFilter = new PaginationFilter { PageNumber = 1, PageSize = 100 };
                var searchResults = await _repository.MarketRepository.SearchMarket(searchTerm, paginationFilter);
                var marketDtos = _mapper.Map<IEnumerable<MarketResponseDto>>(searchResults.PageItems);

                await CreateAuditLog(
                    "Market Search Completed",
                    $"CorrelationId: {correlationId} - Found {marketDtos.Count()} matching markets",
                    "Market Management"
                );

                return ResponseFactory.Success(marketDtos, "Markets search completed successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Market Search Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Market Management"
                );
                return ResponseFactory.Fail<IEnumerable<MarketResponseDto>>(ex, "An unexpected error occurred");
            }
        }
/*
        public async Task<BaseResponse<bool>> UnblockAssistantOfficer(string officerId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Officer Unblock Attempt",
                    $"CorrelationId: {correlationId} - Unblocking officer: {officerId}",
                    "Officer Management"
                );

                var officer = await _repository.AssistCenterOfficerRepository.GetByIdAsync(officerId, true);
                if (officer == null)
                {
                    await CreateAuditLog(
                        "Officer Unblock Failed",
                        $"CorrelationId: {correlationId} - Officer not found",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<bool>(new NotFoundException("Assistant Officer not found"), "Not found");
                }

                officer.IsBlocked = false;
                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Officer Unblocked",
                    $"CorrelationId: {correlationId} - Officer successfully unblocked",
                    "Officer Management"
                );

                return ResponseFactory.Success(true, "Assistant Officer unblocked successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Officer Unblock Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Officer Management"
                );
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }
*/
        public async Task<BaseResponse<PaginatorDto<IEnumerable<LevyPaymentWithTraderDto>>>> GetLevyPayments(
    PaymentPeriodEnum? period,
    string searchQuery,
    PaginationFilter paginationFilter)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Levy Payments Query",
                    $"CorrelationId: {correlationId} - Retrieving levy payments with period: {period}, search: {searchQuery}",
                    "Levy Management"
                );

                // Get paginated data
                var pagedPayments = await _repository.LevyPaymentRepository.GetPagedPaymentWithDetails(
                    period,
                    searchQuery,
                    paginationFilter,
                    false);

                // Add debug logging to see what's loaded
                foreach (var payment in pagedPayments.PageItems)
                {
                    _logger.LogInformation($"Payment {payment.Id}, TraderId: {payment.TraderId}, " +
                        $"Trader is null: {payment.Trader == null}, " +
                        $"Trader.User is null: {payment.Trader?.User == null}");
                }

                // Map to DTOs with safer null handling and extra logging
                var levyPaymentDtos = pagedPayments.PageItems.Select(payment =>
                {
                    string traderName = "Unknown";

                    try
                    {
                        // Try to get trader name
                        if (payment.Trader?.User != null)
                        {
                            traderName = $"{payment.Trader.User.FirstName} {payment.Trader.User.LastName}";
                            _logger.LogInformation($"Found trader name: {traderName} for payment {payment.Id}");
                        }
                        // If trader not found, try GoodBoy
                        else if (payment.GoodBoy?.User != null)
                        {
                            traderName = $"{payment.GoodBoy.User.FirstName} {payment.GoodBoy.User.LastName}";
                            _logger.LogInformation($"Found goodboy name: {traderName} for payment {payment.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"No trader or goodboy user found for payment {payment.Id}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error getting trader name for payment {payment.Id}");
                    }

                    return new LevyPaymentWithTraderDto
                    {
                        Id = payment.Id,
                        Amount = payment.Amount,
                        TraderName = traderName,
                        PaymentDate = payment.PaymentDate,
                        PaymentStatus = payment.PaymentStatus
                    };
                }).ToList();

                // Create paginator result
                var result = new PaginatorDto<IEnumerable<LevyPaymentWithTraderDto>>
                {
                    PageItems = levyPaymentDtos,
                    CurrentPage = pagedPayments.CurrentPage,
                    PageSize = pagedPayments.PageSize,
                    NumberOfPages = pagedPayments.NumberOfPages
                };

                await CreateAuditLog(
                    "Levy Payments Retrieved",
                    $"CorrelationId: {correlationId} - Retrieved {levyPaymentDtos.Count()} payments",
                    "Levy Management"
                );

                return ResponseFactory.Success(result, "Levy payments retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Levy Payments Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Levy Management"
                );
                _logger.LogError(ex, "Error retrieving levy payments: {Message}", ex.Message);
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<LevyPaymentWithTraderDto>>>(ex, "An unexpected error occurred");
            }
        }


        /*  public async Task<BaseResponse<PaginatorDto<IEnumerable<LevyPaymentListDto>>>> GetLevyPayments(
        PaymentPeriodEnum? period,
        string searchQuery,
        PaginationFilter paginationFilter)
          {
              var correlationId = Guid.NewGuid().ToString();
              try
              {
                  await CreateAuditLog(
                      "Levy Payments Query",
                      $"CorrelationId: {correlationId} - Retrieving levy payments with period: {period}, search: {searchQuery}",
                      "Levy Management"
                  );

                  // Get paginated data
                  var pagedPayments = await _repository.LevyPaymentRepository.GetPagedPaymentWithDetails(
                      period,
                      searchQuery,
                      paginationFilter,
                      false);

                  // Map to DTOs with sequential SN
                  var levyPaymentDtos = pagedPayments.PageItems.Select((payment, index) => new LevyPaymentListDto
                  {
                      Id = payment.Id,
                      Amount = payment.Amount,
                      // Handle trader name based on payment source
                      TraderName = payment.Trader != null
                          ? $"{payment.Trader.User.FirstName} {payment.Trader.User.LastName}"
                          : payment.GoodBoy != null
                              ? $"{payment.GoodBoy.User.FirstName} {payment.GoodBoy.User.LastName}"
                              : "Unknown",
                      PaymentDate = payment.PaymentDate,
                      PaymentStatus = payment.PaymentStatus
                  }).ToList();

                  // Create paginator result
                  var result = new PaginatorDto<IEnumerable<LevyPaymentListDto>>
                  {
                      PageItems = levyPaymentDtos,
                      CurrentPage = pagedPayments.CurrentPage,
                      PageSize = pagedPayments.PageSize,
                      NumberOfPages = pagedPayments.NumberOfPages
                  };

                  await CreateAuditLog(
                      "Levy Payments Retrieved",
                      $"CorrelationId: {correlationId} - Retrieved {levyPaymentDtos.Count()} payments",
                      "Levy Management"
                  );

                  return ResponseFactory.Success(result, "Levy payments retrieved successfully");
              }
              catch (Exception ex)
              {
                  await CreateAuditLog(
                      "Levy Payments Query Failed",
                      $"CorrelationId: {correlationId} - Error: {ex.Message}",
                      "Levy Management"
                  );
                  _logger.LogError(ex, "Error retrieving levy payments: {Message}", ex.Message);
                  return ResponseFactory.Fail<PaginatorDto<IEnumerable<LevyPaymentListDto>>>(ex, "An unexpected error occurred");
              }
          }
  */


        public async Task<BaseResponse<IEnumerable<LevySetupResponseDto>>> GetLevySetups()
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Levy Setups Query",
                    $"CorrelationId: {correlationId} - Retrieving all levy setups",
                    "Levy Management"
                );

                var levySetups = await _repository.LevyPaymentRepository.GetAllLevySetupsAsync(false);
                var levySetupDtos = _mapper.Map<IEnumerable<LevySetupResponseDto>>(levySetups);

                await CreateAuditLog(
                    "Levy Setups Retrieved",
                    $"CorrelationId: {correlationId} - Retrieved {levySetupDtos.Count()} setups",
                    "Levy Management"
                );

                return ResponseFactory.Success(levySetupDtos, "Levy setups retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Levy Setups Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Levy Management"
                );
                return ResponseFactory.Fail<IEnumerable<LevySetupResponseDto>>(ex, "An unexpected error occurred");
            }
        }

        // Service Method with Search Query
        public async Task<BaseResponse<IEnumerable<MarketResponseDto>>> GetAllMarkets(string localgovermentId = null, string searchQuery = null)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Markets List Query",
                    $"CorrelationId: {correlationId} - Retrieving markets" +
                    (string.IsNullOrEmpty(localgovermentId) ? "" : $" for LocalGovernment: {localgovermentId}") +
                    (string.IsNullOrEmpty(searchQuery) ? "" : $" with search term: {searchQuery}"),
                    "Market Management"
                );

                // Pass the search query directly to the repository
                var markets = await _repository.MarketRepository.GetAllMarketForExport(false, searchQuery);

                // Filter by LocalGovernment ID if provided
                if (!string.IsNullOrEmpty(localgovermentId))
                {
                    markets = markets.Where(m => m.LocalGovernmentId == localgovermentId).ToList();
                }

                var marketDtos = _mapper.Map<IEnumerable<MarketResponseDto>>(markets);

                await CreateAuditLog(
                    "Markets Retrieved",
                    $"CorrelationId: {correlationId} - Retrieved {marketDtos.Count()} markets" +
                    (string.IsNullOrEmpty(localgovermentId) ? "" : $" for LocalGovernment: {localgovermentId}") +
                    (string.IsNullOrEmpty(searchQuery) ? "" : $" with search term: {searchQuery}"),
                    "Market Management"
                );

                return ResponseFactory.Success(marketDtos, "Markets retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Markets List Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Market Management"
                );
                return ResponseFactory.Fail<IEnumerable<MarketResponseDto>>(ex, "An unexpected error occurred");
            }
        }
        /*     public async Task<BaseResponse<IEnumerable<MarketResponseDto>>> GetAllMarkets(string localgovermentId = null)
             {
                 var correlationId = Guid.NewGuid().ToString();
                 try
                 {
                     await CreateAuditLog(
                         "Markets List Query",
                         $"CorrelationId: {correlationId} - Retrieving markets" +
                         (string.IsNullOrEmpty(localgovermentId) ? "" : $" for LocalGovernment: {localgovermentId}"),
                         "Market Management"
                     );

                     var markets = await _repository.MarketRepository.GetAllMarketForExport(false);

                     // Filter by LocalGovernment ID if provided
                     if (!string.IsNullOrEmpty(localgovermentId))
                     {
                         markets = markets.Where(m => m.LocalGovernmentId == localgovermentId).ToList();
                     }

                     var marketDtos = _mapper.Map<IEnumerable<MarketResponseDto>>(markets);

                     await CreateAuditLog(
                         "Markets Retrieved",
                         $"CorrelationId: {correlationId} - Retrieved {marketDtos.Count()} markets" +
                         (string.IsNullOrEmpty(localgovermentId) ? "" : $" for LocalGovernment: {localgovermentId}"),
                         "Market Management"
                     );

                     return ResponseFactory.Success(marketDtos, "Markets retrieved successfully");
                 }
                 catch (Exception ex)
                 {
                     await CreateAuditLog(
                         "Markets List Query Failed",
                         $"CorrelationId: {correlationId} - Error: {ex.Message}",
                         "Market Management"
                     );
                     return ResponseFactory.Fail<IEnumerable<MarketResponseDto>>(ex, "An unexpected error occurred");
                 }
             }
     */
        public async Task<BaseResponse<TraderDetailsDto>> GetTraderDetails(string traderId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Trader Details Query",
                    $"CorrelationId: {correlationId} - Fetching trader: {traderId}",
                    "Trader Management"
                );

                var trader = await _repository.TraderRepository.GetTraderById(traderId, false);
                if (trader == null)
                {
                    await CreateAuditLog(
                        "Trader Details Query Failed",
                        $"CorrelationId: {correlationId} - Trader not found",
                        "Trader Management"
                    );
                    return ResponseFactory.Fail<TraderDetailsDto>(new NotFoundException("Trader not found"), "Not found");
                }

                var traderDto = _mapper.Map<TraderDetailsDto>(trader);

                await CreateAuditLog(
                    "Trader Details Retrieved",
                    $"CorrelationId: {correlationId} - Trader details retrieved successfully",
                    "Trader Management"
                );

                return ResponseFactory.Success(traderDto, "Trader details retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Trader Details Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Trader Management"
                );
                return ResponseFactory.Fail<TraderDetailsDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<MarketRevenueDto>> GetMarketRevenue(string marketId, DateRangeDto dateRange)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Market Revenue Query",
                    $"CorrelationId: {correlationId} - Fetching revenue for market {marketId} from {dateRange.StartDate} to {dateRange.EndDate}",
                    "Market Analytics"
                );

                var revenue = await _repository.MarketRepository.GetMarketRevenueAsync(marketId, dateRange.StartDate, dateRange.EndDate);
                var revenueDto = _mapper.Map<MarketRevenueDto>(revenue);

                await CreateAuditLog(
                    "Market Revenue Retrieved",
                    $"CorrelationId: {correlationId} - Revenue data retrieved successfully",
                    "Market Analytics"
                );

                return ResponseFactory.Success(revenueDto, "Market revenue retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Market Revenue Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Market Analytics"
                );
                return ResponseFactory.Fail<MarketRevenueDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<ReportMetricsDto>> GetReportMetrics()
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Report Metrics Query",
                    $"CorrelationId: {correlationId} - Fetching report metrics",
                    "Report Management"
                );

                string preset = DateRangePresets.ThisMonth;
                var dateRange = DateRangePresets.GetPresetRange(preset);
                var metrics = await _repository.ReportRepository.GetMetricsAsync(dateRange.StartDate, dateRange.EndDate);
                var metricsDto = _mapper.Map<ReportMetricsDto>(metrics);
                metricsDto.Period = dateRange.DateRangeType;

                await CreateAuditLog(
                    "Report Metrics Retrieved",
                    $"CorrelationId: {correlationId} - Metrics retrieved successfully",
                    "Report Management"
                );

                return ResponseFactory.Success(metricsDto, "Report metrics retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Report Metrics Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Report Management"
                );
                return ResponseFactory.Fail<ReportMetricsDto>(ex, "An unexpected error occurred");
            }
        }

        /*    public async Task<BaseResponse<bool>> DeleteMarket(string marketId)
            {
                var correlationId = Guid.NewGuid().ToString();
                try
                {
                    await CreateAuditLog(
                        "Market Deletion",
                        $"CorrelationId: {correlationId} - Attempting to delete market: {marketId}",
                        "Market Management"
                    );

                    var market = await _repository.MarketRepository.GetMarketByIdAsync(marketId, true);
                    _repository.MarketRepository.DeleteMarket(market);
                    await _repository.SaveChangesAsync();

                    await CreateAuditLog(
                        "Market Deleted",
                        $"CorrelationId: {correlationId} - Market deleted successfully",
                        "Market Management"
                    );

                    return ResponseFactory.Success(true, "Market deleted successfully");
                }
                catch (Exception ex)
                {
                    await CreateAuditLog(
                        "Market Deletion Failed",
                        $"CorrelationId: {correlationId} - Error: {ex.Message}",
                        "Market Management"
                    );
                    return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
                }
            }
    */
        public async Task<BaseResponse<bool>> DeleteMarket(string marketId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Market Deletion With Dependencies",
                    $"CorrelationId: {correlationId} - Attempting to delete market and all dependencies: {marketId}",
                    "Market Management"
                );

                // Get market
                var market = await _repository.MarketRepository.GetMarketByIdAsync(marketId, true);
                if (market == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Market not found"),
                        "Market not found");
                }

                // 1. Delete LevyPayments first since that's causing the constraint violation
                var levyPayments = await _context.LevyPayments
                    .Where(lp => lp.MarketId == marketId)
                    .ToListAsync();

                foreach (var payment in levyPayments)
                {
                    _context.LevyPayments.Remove(payment);
                }
                await _repository.SaveChangesAsync();

                // 2. Handle sections and traders
                if (market.MarketSections != null && market.MarketSections.Any())
                {
                    foreach (var section in market.MarketSections.ToList())
                    {
                        _context.MarketSections.Remove(section);
                    }
                    await _repository.SaveChangesAsync();
                }

                // 3. Handle the chairman relationship
                if (market.Chairman != null)
                {
                    market.Chairman.MarketId = null;
                    await _repository.SaveChangesAsync();
                }

                // 4. Handle traders
                var traders = await _context.Traders
                    .Where(t => t.MarketId == marketId)
                    .ToListAsync();

                foreach (var trader in traders)
                {
                    _context.Traders.Remove(trader);
                }
                await _repository.SaveChangesAsync();

                // 5. Handle caretakers - IMPORTANT: Delete caretakers instead of setting MarketId to null
                // Since MarketId is non-nullable in Caretakers table
                if (market.Caretaker != null)
                {
                    // Delete primary caretaker
                    _context.Caretakers.Remove(market.Caretaker);
                    await _repository.SaveChangesAsync();
                }

                // Delete additional caretakers
                var additionalCaretakers = await _context.Caretakers
                    .Where(c => c.MarketId == marketId)
                    .ToListAsync();

                foreach (var caretaker in additionalCaretakers)
                {
                    // Delete caretaker instead of setting MarketId to null
                    _context.Caretakers.Remove(caretaker);
                }
                await _repository.SaveChangesAsync();

                // 6. Finally delete the market
                _repository.MarketRepository.DeleteMarket(market);
                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Market Deleted With Dependencies",
                    $"CorrelationId: {correlationId} - Market and all its dependencies deleted successfully",
                    "Market Management"
                );
                return ResponseFactory.Success(true, "Market and all its dependencies deleted successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Market Deletion Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Market Management"
                );
                _logger.LogError(ex, "Error deleting market with dependencies: {MarketId}, Error: {ErrorMessage}", marketId, ex.Message);
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<IEnumerable<CaretakerResponseDto>>> GetAllCaretakers()
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Caretakers List Query",
                    $"CorrelationId: {correlationId} - Retrieving all caretakers",
                    "Caretaker Management"
                );

                var caretakers = await _repository.CaretakerRepository.GetAllCaretakers(false);
                var caretakerDtos = _mapper.Map<IEnumerable<CaretakerResponseDto>>(caretakers);

                await CreateAuditLog(
                    "Caretakers Retrieved",
                    $"CorrelationId: {correlationId} - Retrieved {caretakerDtos.Count()} caretakers",
                    "Caretaker Management"
                );

                return ResponseFactory.Success(caretakerDtos, "Caretakers retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Caretakers List Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Caretaker Management"
                );
                return ResponseFactory.Fail<IEnumerable<CaretakerResponseDto>>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<AssistantOfficerResponseDto>> CreateAssistantOfficer(CreateAssistantOfficerRequestDto request)
        {
            var correlationId = Guid.NewGuid().ToString();
            var userId = _currentUser.GetUserId();

            try
            {
                await CreateAuditLog(
                    "Assistant Officer Creation",
                    $"CorrelationId: {correlationId} - Creating new assistant officer: {request.FullName}",
                    "Officer Management"
                );

                // Validate request
                var validationResult = await _createAssistOfficerValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - Validation failed",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                        new ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - Email already registered",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                        new BadRequestException("Email address is already registered"),
                        "Email already exists");
                }

                // Get Chairman details
                var chairman = await _repository.ChairmanRepository.GetChairmanById(userId, false);
                if (chairman == null)
                {
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - Chairman not found",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                        new BadRequestException("Chairman not found"),
                        "Chairman not found");
                }

                // Create ApplicationUser
                var nameParts = request.FullName.Split(' ');
                var firstName = nameParts[0];
                var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

                var defaultPassword = GenerateDefaultPassword(request.FullName);
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = request.Email,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Gender = request.Gender,
                    ProfileImageUrl = request.ProfileImage,
                    LocalGovernmentId = chairman.LocalGovernmentId
                };

                var createUserResult = await _userManager.CreateAsync(user, defaultPassword);
                if (!createUserResult.Succeeded)
                {
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - Failed to create user account",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                        new Exception(string.Join(", ", createUserResult.Errors.Select(e => e.Description))),
                        "Failed to create user account");
                }

                // Handle profile image update if provided
                /* if (request.ProfileImage != null)
                 {
                     // If there's an existing image, you might want to delete it first
                     if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                     {
                         await _cloudinaryService.DeletePhotoAsync(user.ProfileImageUrl);
                     }

                     var uploadResult = await _cloudinaryService.UploadImage(request.ProfileImage, "assistant-officers");
                     if (uploadResult.IsSuccessful && uploadResult.Data.ContainsKey("Url"))
                     {
                         user.ProfileImageUrl = uploadResult.Data["Url"];
                          await _userManager.UpdateAsync(user);
                     }
                 }
                 */

                await _userManager.UpdateAsync(user);
                // Assign role
                var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.AssistOfficer);
                if (!roleResult.Succeeded)
                {
                    // Rollback user creation if role assignment fails
                    await _userManager.DeleteAsync(user);
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - Failed to assign assistant officer role",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                        new Exception("Failed to assign assistant officer role"),
                        "Role assignment failed");
                }

                // Create AssistCenterOfficer entity
                var officer = new AssistCenterOfficer
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    ChairmanId = chairman.Id,
                    LocalGovernmentId = chairman.LocalGovernmentId,
                    // For backward compatibility, store the first market ID in MarketId if provided
                    MarketId = request.MarketIds != null && request.MarketIds.Count > 0 ? request.MarketIds[0] : null,
                    IsBlocked = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _repository.AssistCenterOfficerRepository.AddAssistCenterOfficer(officer);

                // Create market assignments if provided
                if (request.MarketIds != null && request.MarketIds.Count > 0)
                {
                    foreach (var marketId in request.MarketIds.Distinct())
                    {
                        // Validate that market exists and belongs to chairman's LG
                        var market = await _repository.MarketRepository.GetMarketByIdAsync(marketId, false);
                        if (market != null && market.LocalGovernmentId == chairman.LocalGovernmentId)
                        {
                            var assignment = new OfficerMarketAssignment
                            {
                                Id = Guid.NewGuid().ToString(),
                                AssistCenterOfficerId = officer.Id,
                                MarketId = marketId,
                                CreatedAt = DateTime.UtcNow
                            };

                            _repository.OfficerMarketAssignmentRepository.AddAssignment(assignment);
                        }
                    }
                }

                await _repository.SaveChangesAsync();

                // Get full officer details including markets for response
                var createdOfficer = await _repository.AssistCenterOfficerRepository.GetAssistantOfficerByIdAsync(officer.Id, false);

                // Map response
                var response = _mapper.Map<AssistantOfficerResponseDto>(createdOfficer);
                response.DefaultPassword = defaultPassword;
                response.FullName = request.FullName;
                response.Email = user.Email;
                response.PhoneNumber = user.PhoneNumber;
                response.Gender = user.Gender;
                response.ProfileImageUrl = user.ProfileImageUrl;

                await CreateAuditLog(
                    "Assistant Officer Created",
                    $"CorrelationId: {correlationId} - Officer created successfully with ID: {officer.Id}",
                    "Officer Management"
                );

                return ResponseFactory.Success(response,
                    "Assistant Officer created successfully. Please note down the default password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assistant officer");
                await CreateAuditLog(
                    "Creation Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Officer Management"
                );
                return ResponseFactory.Fail<AssistantOfficerResponseDto>(ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<AssistantOfficerResponseDto>> UpdateAssistantOfficer(string officerId, UpdateAssistantOfficerRequestDto request)
        {
            var correlationId = Guid.NewGuid().ToString();
            var userId = _currentUser.GetUserId();

            try
            {
                await CreateAuditLog(
                    "Assistant Officer Update",
                    $"CorrelationId: {correlationId} - Updating assistant officer with ID: {officerId}",
                    "Officer Management"
                );

                // Get Chairman details (to verify authority)
                var chairman = await _repository.ChairmanRepository.GetChairmanById(userId, false);
                if (chairman == null)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Chairman not found",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                        new BadRequestException("Chairman not found"),
                        "Chairman not found");
                }

                // Get existing officer with market assignments
                var officer = await _repository.AssistCenterOfficerRepository.GetAssistantOfficerByIdAsync(officerId, false);
                if (officer == null)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Assistant officer not found",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                        new NotFoundException($"Assistant officer with ID {officerId} not found"),
                        "Assistant officer not found");
                }

                // Verify this chairman has authority over this officer
                if (officer.ChairmanId != chairman.Id)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Unauthorized access",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                        new UnauthorizedAccessException("You are not authorized to update this assistant officer"),
                        "Unauthorized access");
                }

                // Get the user associated with the officer - USING AsNoTracking() to avoid tracking conflicts
                var userToUpdate = await _userManager.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == officer.UserId);

                if (userToUpdate == null)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Associated user not found",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                        new NotFoundException("Associated user account not found"),
                        "User not found");
                }

                // Apply updates to the user entity
                if (!string.IsNullOrEmpty(request?.FullName))
                {
                    var nameParts = request.FullName.Split(' ');
                    userToUpdate.FirstName = nameParts.Length > 0 ? nameParts[0] : userToUpdate.FirstName;
                    userToUpdate.LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : userToUpdate.LastName;
                }

                if (!string.IsNullOrEmpty(request?.Email))
                {
                    userToUpdate.Email = request.Email;
                    userToUpdate.UserName = request.Email; // Update username to match email
                    userToUpdate.NormalizedEmail = request.Email.ToUpper();
                    userToUpdate.NormalizedUserName = request.Email.ToUpper();
                }

                if (!string.IsNullOrEmpty(request?.PhoneNumber))
                {
                    userToUpdate.PhoneNumber = request.PhoneNumber;
                }

                if (!string.IsNullOrEmpty(request?.Gender))
                {
                    userToUpdate.Gender = request.Gender;
                }

                // Handle profile image update if provided
              /*  if (request?.ProfileImage != null)
                {
                    // If there's an existing image, you might want to delete it first
                    if (!string.IsNullOrEmpty(userToUpdate.ProfileImageUrl))
                    {
                        await _cloudinaryService.DeletePhotoAsync(userToUpdate.ProfileImageUrl);
                    }

                    var uploadResult = await _cloudinaryService.UploadImage(request.ProfileImage, "assistant-officers");
                    if (uploadResult.IsSuccessful && uploadResult.Data.ContainsKey("Url"))
                    {
                        userToUpdate.ProfileImageUrl = uploadResult.Data["Url"];
                    }
                }*/
                userToUpdate.ProfileImageUrl = request.ProfileImage;
                // Update user with the detached entity
                var updateUserResult = await _userManager.UpdateAsync(userToUpdate);
                if (!updateUserResult.Succeeded)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Failed to update user account",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                        new Exception(string.Join(", ", updateUserResult.Errors.Select(e => e.Description))),
                        "Failed to update user account");
                }

                // Update Officer details
                officer.UpdatedAt = DateTime.UtcNow;

                // For backward compatibility, update the MarketId field if markets changed
                if (request?.MarketIds != null && request.MarketIds.Count > 0)
                {
                    officer.MarketId = request.MarketIds[0]; // First market
                }
                else
                {
                    officer.MarketId = null;
                }

                // Handle market assignments
                if (request?.MarketIds != null)
                {
                    // Get current assignments
                    var existingAssignments = officer.MarketAssignments.ToList();

                    // Markets to remove (in existing but not in request)
                    var marketsToRemove = existingAssignments
                        .Where(a => !request.MarketIds.Contains(a.MarketId))
                        .ToList();

                    // Markets to add (in request but not in existing)
                    var existingMarketIds = existingAssignments.Select(a => a.MarketId).ToList();
                    var marketsToAdd = request.MarketIds
                        .Where(id => !existingMarketIds.Contains(id))
                        .Distinct()
                        .ToList();

                    // Remove markets no longer assigned
                    foreach (var assignment in marketsToRemove)
                    {
                        _repository.OfficerMarketAssignmentRepository.RemoveAssignment(assignment);
                    }

                    // Add new market assignments
                    foreach (var marketId in marketsToAdd)
                    {
                        // Validate that market exists and belongs to chairman's LG
                        var market = await _repository.MarketRepository.GetMarketByIdAsync(marketId, false);
                        if (market != null && market.LocalGovernmentId == chairman.LocalGovernmentId)
                        {
                            var assignment = new OfficerMarketAssignment
                            {
                                Id = Guid.NewGuid().ToString(),
                                AssistCenterOfficerId = officer.Id,
                                MarketId = marketId,
                                CreatedAt = DateTime.UtcNow
                            };

                            _repository.OfficerMarketAssignmentRepository.AddAssignment(assignment);
                        }
                    }
                }

                _repository.AssistCenterOfficerRepository.UpdateAssistCenterOfficer(officer);
                await _repository.SaveChangesAsync();

                // Get updated officer with market details for response
                var updatedOfficer = await _repository.AssistCenterOfficerRepository.GetAssistantOfficerByIdAsync(officerId, false);

                // Get fresh user data after all updates
                var updatedUser = await _userManager.FindByIdAsync(officer.UserId);

                // Map response
                var response = _mapper.Map<AssistantOfficerResponseDto>(updatedOfficer);
                response.FullName = $"{updatedUser.FirstName} {updatedUser.LastName}".Trim();
                response.Email = updatedUser.Email;
                response.PhoneNumber = updatedUser.PhoneNumber;
                response.Gender = updatedUser.Gender;
                response.ProfileImageUrl = updatedUser.ProfileImageUrl;

                await CreateAuditLog(
                    "Assistant Officer Updated",
                    $"CorrelationId: {correlationId} - Officer updated successfully with ID: {officer.Id}",
                    "Officer Management"
                );

                return ResponseFactory.Success(response, "Assistant Officer updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assistant officer");
                await CreateAuditLog(
                    "Update Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Officer Management"
                );
                return ResponseFactory.Fail<AssistantOfficerResponseDto>(ex, "An unexpected error occurred");
            }
        }

        /*    public async Task<BaseResponse<AssistantOfficerResponseDto>> UpdateAssistantOfficer(string officerId, UpdateAssistantOfficerRequestDto request)
            {
                var correlationId = Guid.NewGuid().ToString();
                var userId = _currentUser.GetUserId();

                try
                {
                    await CreateAuditLog(
                        "Assistant Officer Update",
                        $"CorrelationId: {correlationId} - Updating assistant officer with ID: {officerId}",
                        "Officer Management"
                    );

                    // Validate request
                  *//*  var validationResult = await _updateAssistOfficerValidator.ValidateAsync(request);
                    if (!validationResult.IsValid)
                    {
                        await CreateAuditLog(
                            "Update Failed",
                            $"CorrelationId: {correlationId} - Validation failed",
                            "Officer Management"
                        );
                        return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                            new ValidationException(validationResult.Errors),
                            "Validation failed");
                    }*//*

                    // Get Chairman details (to verify authority)
                    var chairman = await _repository.ChairmanRepository.GetChairmanById(userId, false);
                    if (chairman == null)
                    {
                        await CreateAuditLog(
                            "Update Failed",
                            $"CorrelationId: {correlationId} - Chairman not found",
                            "Officer Management"
                        );
                        return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                            new BadRequestException("Chairman not found"),
                            "Chairman not found");
                    }

                    // Get existing officer with market assignments
                    var officer = await _repository.AssistCenterOfficerRepository.GetAssistantOfficerByIdAsync(officerId, false);
                    if (officer == null)
                    {
                        await CreateAuditLog(
                            "Update Failed",
                            $"CorrelationId: {correlationId} - Assistant officer not found",
                            "Officer Management"
                        );
                        return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                            new NotFoundException($"Assistant officer with ID {officerId} not found"),
                            "Assistant officer not found");
                    }

                    // Verify this chairman has authority over this officer
                    if (officer.ChairmanId != chairman.Id)
                    {
                        await CreateAuditLog(
                            "Update Failed",
                            $"CorrelationId: {correlationId} - Unauthorized access",
                            "Officer Management"
                        );
                        return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                            new UnauthorizedAccessException("You are not authorized to update this assistant officer"),
                            "Unauthorized access");
                    }

                    // Get the user associated with the officer
                    var user = await _userManager.FindByIdAsync(officer.UserId);
                    if (user == null)
                    {
                        await CreateAuditLog(
                            "Update Failed",
                            $"CorrelationId: {correlationId} - Associated user not found",
                            "Officer Management"
                        );
                        return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                            new NotFoundException("Associated user account not found"),
                            "User not found");
                    }

                    // Check if email changed and if it's already in use
                    *//* if (request.Email != user.Email)
                     {
                         var existingUser = await _userManager.FindByEmailAsync(request.Email);
                         if (existingUser != null && existingUser.Id != user.Id)
                         {
                             await CreateAuditLog(
                                 "Update Failed",
                                 $"CorrelationId: {correlationId} - Email already registered",
                                 "Officer Management"
                             );
                             return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                                 new BadRequestException("Email address is already registered"),
                                 "Email already exists");
                         }*//*
                    // }
                    string[] nameParts = Array.Empty<string>();

                    // Check if FullName is NOT null or empty before splitting
                    if (!string.IsNullOrEmpty(request?.FullName))
                    {
                        nameParts = request.FullName.Split(' ');
                        user.FirstName = nameParts.Length > 0 ? nameParts[0] : user.FirstName;
                        user.LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : user.LastName; // Keep original LastName if no new value
                    }
                    if (!string.IsNullOrEmpty(request?.Email))
                    {
                        user.Email = request?.Email;
                    }
                    if (!string.IsNullOrEmpty(request?.PhoneNumber))
                    {
                        user.PhoneNumber = request?.PhoneNumber;
                    }
                    if (!string.IsNullOrEmpty(request?.Gender))
                    {
                        user.Gender = request?.Gender;
                    }

                    // Handle profile image update if provided
                    if (request.ProfileImage != null)
                    {
                        // If there's an existing image, you might want to delete it first
                        if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                        {
                            await _cloudinaryService.DeletePhotoAsync(user.ProfileImageUrl);
                        }

                        var uploadResult = await _cloudinaryService.UploadImage(request.ProfileImage, "assistant-officers");
                        if (uploadResult.IsSuccessful && uploadResult.Data.ContainsKey("Url"))
                        {
                            user.ProfileImageUrl = uploadResult.Data["Url"];
                        }
                    }

                    var updateUserResult = await _userManager.UpdateAsync(user);
                    if (!updateUserResult.Succeeded)
                    {
                        await CreateAuditLog(
                            "Update Failed",
                            $"CorrelationId: {correlationId} - Failed to update user account",
                            "Officer Management"
                        );
                        return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                            new Exception(string.Join(", ", updateUserResult.Errors.Select(e => e.Description))),
                            "Failed to update user account");
                    }

                    // Update Officer details
                    officer.UpdatedAt = DateTime.UtcNow;

                    // For backward compatibility, update the MarketId field if markets changed
                    if (request.MarketIds != null && request.MarketIds.Count > 0)
                    {
                        officer.MarketId = request.MarketIds[0]; // First market
                    }
                    else
                    {
                        officer.MarketId = null;
                    }

                    // Handle market assignments
                    if (request.MarketIds != null)
                    {
                        // Get current assignments
                        var existingAssignments = officer.MarketAssignments.ToList();

                        // Markets to remove (in existing but not in request)
                        var marketsToRemove = existingAssignments
                            .Where(a => !request.MarketIds.Contains(a.MarketId))
                            .ToList();

                        // Markets to add (in request but not in existing)
                        var existingMarketIds = existingAssignments.Select(a => a.MarketId).ToList();
                        var marketsToAdd = request.MarketIds
                            .Where(id => !existingMarketIds.Contains(id))
                            .Distinct()
                            .ToList();

                        // Remove markets no longer assigned
                        foreach (var assignment in marketsToRemove)
                        {
                            _repository.OfficerMarketAssignmentRepository.RemoveAssignment(assignment);
                        }

                        // Add new market assignments
                        foreach (var marketId in marketsToAdd)
                        {
                            // Validate that market exists and belongs to chairman's LG
                            var market = await _repository.MarketRepository.GetMarketByIdAsync(marketId, false  );
                            if (market != null && market.LocalGovernmentId == chairman.LocalGovernmentId)
                            {
                                var assignment = new OfficerMarketAssignment
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    AssistCenterOfficerId = officer.Id,
                                    MarketId = marketId,
                                    CreatedAt = DateTime.UtcNow
                                };

                                _repository.OfficerMarketAssignmentRepository.AddAssignment(assignment);
                            }
                        }
                    }

                    _repository.AssistCenterOfficerRepository.UpdateAssistCenterOfficer(officer);
                    await _repository.SaveChangesAsync();

                    // Get updated officer with market details for response
                    var updatedOfficer = await _repository.AssistCenterOfficerRepository.GetAssistantOfficerByIdAsync(officerId, false);

                    // Map response
                    var response = _mapper.Map<AssistantOfficerResponseDto>(updatedOfficer);
                    response.FullName = request.FullName;
                    response.Email = user.Email;
                    response.PhoneNumber = user.PhoneNumber;
                    response.Gender = user.Gender;
                    response.ProfileImageUrl = user.ProfileImageUrl;

                    await CreateAuditLog(
                        "Assistant Officer Updated",
                        $"CorrelationId: {correlationId} - Officer updated successfully with ID: {officer.Id}",
                        "Officer Management"
                    );

                    return ResponseFactory.Success(response, "Assistant Officer updated successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating assistant officer");
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Error: {ex.Message}",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<AssistantOfficerResponseDto>(ex, "An unexpected error occurred");
                }
            }
    */
        public async Task<BaseResponse<AssistantOfficerResponseDto>> GetAssistantOfficerById(string officerId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Assistant Officer Query",
                    $"CorrelationId: {correlationId} - Fetching officer: {officerId}",
                    "Officer Management"
                );

                var officer = await _repository.AssistCenterOfficerRepository.GetAssistantOfficerByIdAsync(officerId, false);
                if (officer == null)
                {
                    await CreateAuditLog(
                        "Assistant Officer Query Failed",
                        $"CorrelationId: {correlationId} - Officer not found",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                        new NotFoundException("Assistant Officer not found"),
                        "Not found");
                }

                var response = _mapper.Map<AssistantOfficerResponseDto>(officer);

                await CreateAuditLog(
                    "Assistant Officer Retrieved",
                    $"CorrelationId: {correlationId} - Officer details retrieved successfully",
                    "Officer Management"
                );

                return ResponseFactory.Success(response, "Assistant Officer retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Assistant Officer Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Officer Management"
                );
                return ResponseFactory.Fail<AssistantOfficerResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> BlockAssistantOfficer(string officerId)
        {
            var correlationId = Guid.NewGuid().ToString();
            var userId = _currentUser.GetUserId();

            try
            {
                await CreateAuditLog(
                    "Assistant Officer Block",
                    $"CorrelationId: {correlationId} - Attempting to block officer: {officerId}",
                    "Officer Management"
                );

                // Get Chairman details to verify authority
                var chairman = await _repository.ChairmanRepository.GetChairmanById(userId, false);
                if (chairman == null)
                {
                    await CreateAuditLog(
                        "Block Failed",
                        $"CorrelationId: {correlationId} - Chairman not found",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("Chairman not found"),
                        "Chairman not found");
                }

                var officer = await _repository.AssistCenterOfficerRepository.GetByIdAsync(officerId, true);
                if (officer == null)
                {
                    await CreateAuditLog(
                        "Block Failed",
                        $"CorrelationId: {correlationId} - Officer not found",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Assistant Officer not found"),
                        "Not found");
                }

                // Verify chairman has authority over this officer
                if (officer.ChairmanId != chairman.Id)
                {
                    await CreateAuditLog(
                        "Block Failed",
                        $"CorrelationId: {correlationId} - Unauthorized access",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new UnauthorizedAccessException("You are not authorized to block this assistant officer"),
                        "Unauthorized access");
                }

                // Check if already blocked
                if (officer.IsBlocked)
                {
                    return ResponseFactory.Success(true, "Assistant Officer is already blocked");
                }

                // Update officer status
                officer.IsBlocked = true;
                officer.UpdatedAt = DateTime.UtcNow;

                // Also update user account status
                var user = await _userManager.FindByIdAsync(officer.UserId);
                if (user != null)
                {
                    user.IsActive = false;
                    await _userManager.UpdateAsync(user);
                }

                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Assistant Officer Blocked",
                    $"CorrelationId: {correlationId} - Officer blocked successfully",
                    "Officer Management"
                );

                return ResponseFactory.Success(true, "Assistant Officer blocked successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Assistant Officer Block Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Officer Management"
                );
                return ResponseFactory.Fail<bool>(ex, "Error blocking Assistant Officer");
            }
        }

        public async Task<BaseResponse<bool>> UnblockAssistantOfficer(string officerId)
        {
            var correlationId = Guid.NewGuid().ToString();
            var userId = _currentUser.GetUserId();

            try
            {
                await CreateAuditLog(
                    "Officer Unblock Attempt",
                    $"CorrelationId: {correlationId} - Unblocking officer: {officerId}",
                    "Officer Management"
                );

                // Get Chairman details to verify authority
                var chairman = await _repository.ChairmanRepository.GetChairmanById(userId, false);
                if (chairman == null)
                {
                    await CreateAuditLog(
                        "Unblock Failed",
                        $"CorrelationId: {correlationId} - Chairman not found",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("Chairman not found"),
                        "Chairman not found");
                }

                var officer = await _repository.AssistCenterOfficerRepository.GetByIdAsync(officerId, true);
                if (officer == null)
                {
                    await CreateAuditLog(
                        "Officer Unblock Failed",
                        $"CorrelationId: {correlationId} - Officer not found",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Assistant Officer not found"),
                        "Not found");
                }

                // Verify chairman has authority over this officer
                if (officer.ChairmanId != chairman.Id)
                {
                    await CreateAuditLog(
                        "Unblock Failed",
                        $"CorrelationId: {correlationId} - Unauthorized access",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new UnauthorizedAccessException("You are not authorized to unblock this assistant officer"),
                        "Unauthorized access");
                }

                // Check if already unblocked
                if (!officer.IsBlocked)
                {
                    return ResponseFactory.Success(true, "Assistant Officer is already active");
                }

                // Update officer status
                officer.IsBlocked = false;
                officer.UpdatedAt = DateTime.UtcNow;

                // Also update user account status
                var user = await _userManager.FindByIdAsync(officer.UserId);
                if (user != null)
                {
                    user.IsActive = true;
                    await _userManager.UpdateAsync(user);
                }

                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Officer Unblocked",
                    $"CorrelationId: {correlationId} - Officer successfully unblocked",
                    "Officer Management"
                );

                return ResponseFactory.Success(true, "Assistant Officer unblocked successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Officer Unblock Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Officer Management"
                );
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        /* public async Task<BaseResponse<AssistantOfficerResponseDto>> CreateAssistantOfficer(CreateAssistantOfficerRequestDto request)
         {
             var correlationId = Guid.NewGuid().ToString();
             var userId = _currentUser.GetUserId();

             try
             {
                 await CreateAuditLog(
                     "Assistant Officer Creation",
                     $"CorrelationId: {correlationId} - Creating new assistant officer: {request.FullName}",
                     "Officer Management"
                 );

                 // Validate request
                 var validationResult = await _createAssistOfficerValidator.ValidateAsync(request);
                 if (!validationResult.IsValid)
                 {
                     await CreateAuditLog(
                         "Creation Failed",
                         $"CorrelationId: {correlationId} - Validation failed",
                         "Officer Management"
                     );
                     return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                         new ValidationException(validationResult.Errors),
                         "Validation failed");
                 }

                 // Check if email already exists
                 var existingUser = await _userManager.FindByEmailAsync(request.Email);
                 if (existingUser != null)
                 {
                     await CreateAuditLog(
                         "Creation Failed",
                         $"CorrelationId: {correlationId} - Email already registered",
                         "Officer Management"
                     );
                     return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                         new BadRequestException("Email address is already registered"),
                         "Email already exists");
                 }

                 // Get Chairman details
                 var chairman = await _repository.ChairmanRepository.GetChairmanById(userId, false);
                 if (chairman == null)
                 {
                     await CreateAuditLog(
                         "Creation Failed",
                         $"CorrelationId: {correlationId} - Chairman not found",
                         "Officer Management"
                     );
                     return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                         new BadRequestException("Chairman not found"),
                         "Chairman not found");
                 }

                 // Create ApplicationUser
                 var nameParts = request.FullName.Split(' ');
                 var firstName = nameParts[0];
                 var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

                 var defaultPassword = GenerateDefaultPassword(request.FullName);
                 var user = new ApplicationUser
                 {
                     Id = Guid.NewGuid().ToString(),
                     UserName = request.Email,
                     Email = request.Email,
                     PhoneNumber = request.PhoneNumber,
                     FirstName = firstName,
                     LastName = lastName,
                     EmailConfirmed = true,
                     IsActive = true,
                     CreatedAt = DateTime.UtcNow,
                     Gender = request.Gender,
                     ProfileImageUrl = "",
                     LocalGovernmentId = chairman.LocalGovernmentId
                 };

                 var createUserResult = await _userManager.CreateAsync(user, defaultPassword);
                 if (!createUserResult.Succeeded)
                 {
                     await CreateAuditLog(
                         "Creation Failed",
                         $"CorrelationId: {correlationId} - Failed to create user account",
                         "Officer Management"
                     );
                     return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                         new Exception(string.Join(", ", createUserResult.Errors.Select(e => e.Description))),
                         "Failed to create user account");
                 }

                 // Assign role
                 var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.AssistOfficer);
                 if (!roleResult.Succeeded)
                 {
                     // Rollback user creation if role assignment fails
                     await _userManager.DeleteAsync(user);
                     await CreateAuditLog(
                         "Creation Failed",
                         $"CorrelationId: {correlationId} - Failed to assign assistant officer role",
                         "Officer Management"
                     );
                     return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                         new Exception("Failed to assign assistant officer role"),
                         "Role assignment failed");
                 }

                 // Create AssistCenterOfficer entity
                 var officer = new AssistCenterOfficer
                 {
                     UserId = user.Id,
                     ChairmanId = chairman.Id,
                     LocalGovernmentId = chairman.LocalGovernmentId,
                     MarketId = !string.IsNullOrEmpty(request.MarketId) ? request.MarketId : null,
                     IsBlocked = false,
                     IsActive = true,
                     CreatedAt = DateTime.UtcNow
                 };

                 _repository.AssistCenterOfficerRepository.AddAssistCenterOfficer(officer);
                 await _repository.SaveChangesAsync();

                 // Map response
                 var response = _mapper.Map<AssistantOfficerResponseDto>(officer);
                 response.DefaultPassword = defaultPassword;
                 response.FullName = request.FullName;
                 response.Email = user.Email;
                 response.PhoneNumber = user.PhoneNumber;
                 response.Gender = user.Gender;
                 response.MarketId = officer.MarketId;

                 await CreateAuditLog(
                     "Assistant Officer Created",
                     $"CorrelationId: {correlationId} - Officer created successfully with ID: {officer.Id}",
                     "Officer Management"
                 );

                 return ResponseFactory.Success(response,
                     "Assistant Officer created successfully. Please note down the default password.");
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error creating assistant officer");
                 await CreateAuditLog(
                     "Creation Failed",
                     $"CorrelationId: {correlationId} - Error: {ex.Message}",
                     "Officer Management"
                 );
                 return ResponseFactory.Fail<AssistantOfficerResponseDto>(ex, "An unexpected error occurred");
             }
         }
         public async Task<BaseResponse<AssistantOfficerResponseDto>> UpdateAssistantOfficer(string officerId, UpdateAssistantOfficerRequestDto request)
         {
             var correlationId = Guid.NewGuid().ToString();
             var userId = _currentUser.GetUserId();

             try
             {
                 await CreateAuditLog(
                     "Assistant Officer Update",
                     $"CorrelationId: {correlationId} - Updating assistant officer with ID: {officerId}",
                     "Officer Management"
                 );

                 // Get Chairman details (to verify authority)
                 var chairman = await _repository.ChairmanRepository.GetChairmanById(userId, false);
                 if (chairman == null)
                 {
                     await CreateAuditLog(
                         "Update Failed",
                         $"CorrelationId: {correlationId} - Chairman not found",
                         "Officer Management"
                     );
                     return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                         new BadRequestException("Chairman not found"),
                         "Chairman not found");
                 }

                 // Get existing officer
                 var officer = await _repository.AssistCenterOfficerRepository.GetAssistantOfficerByIdAsync(officerId, true);
                 if (officer == null)
                 {
                     await CreateAuditLog(
                         "Update Failed",
                         $"CorrelationId: {correlationId} - Assistant officer not found",
                         "Officer Management"
                     );
                     return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                         new NotFoundException($"Assistant officer with ID {officerId} not found"),
                         "Assistant officer not found");
                 }

                 // Verify this chairman has authority over this officer
                 if (officer.ChairmanId != chairman.Id)
                 {
                     await CreateAuditLog(
                         "Update Failed",
                         $"CorrelationId: {correlationId} - Unauthorized access",
                         "Officer Management"
                     );
                     return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                         new UnauthorizedAccessException("You are not authorized to update this assistant officer"),
                         "Unauthorized access");
                 }

                 // Get the user associated with the officer
                 var user = await _userManager.FindByIdAsync(officer.UserId);
                 if (user == null)
                 {
                     await CreateAuditLog(
                         "Update Failed",
                         $"CorrelationId: {correlationId} - Associated user not found",
                         "Officer Management"
                     );
                     return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                         new NotFoundException("Associated user account not found"),
                         "User not found");
                 }

                 // Check if email changed and if it's already in use
                 if (request.Email != user.Email)
                 {
                     var existingUser = await _userManager.FindByEmailAsync(request.Email);
                     if (existingUser != null && existingUser.Id != user.Id)
                     {
                         await CreateAuditLog(
                             "Update Failed",
                             $"CorrelationId: {correlationId} - Email already registered",
                             "Officer Management"
                         );
                         return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                             new BadRequestException("Email address is already registered"),
                             "Email already exists");
                     }
                 }

                 // Update User details
                 var nameParts = request.FullName.Split(' ');
                 user.FirstName = nameParts[0];
                 user.LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";
                 user.Email = request.Email;
                 user.UserName = request.Email; // Update username to match email
                 user.PhoneNumber = request.PhoneNumber;
                 user.Gender = request.Gender;

                 var updateUserResult = await _userManager.UpdateAsync(user);
                 if (!updateUserResult.Succeeded)
                 {
                     await CreateAuditLog(
                         "Update Failed",
                         $"CorrelationId: {correlationId} - Failed to update user account",
                         "Officer Management"
                     );
                     return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                         new Exception(string.Join(", ", updateUserResult.Errors.Select(e => e.Description))),
                         "Failed to update user account");
                 }

                 // Update Officer details
                 officer.MarketId = !string.IsNullOrEmpty(request.MarketId) ? request.MarketId : null;
                 officer.UpdatedAt = DateTime.UtcNow;

                 _repository.AssistCenterOfficerRepository.UpdateAssistCenterOfficer(officer);
                 await _repository.SaveChangesAsync();

                 // Get updated officer with market details for response
                 var updatedOfficer = await _repository.AssistCenterOfficerRepository.GetAssistantOfficerByIdAsync(officerId, false);

                 // Map response
                 var response = _mapper.Map<AssistantOfficerResponseDto>(updatedOfficer);

                 await CreateAuditLog(
                     "Assistant Officer Updated",
                     $"CorrelationId: {correlationId} - Officer updated successfully with ID: {officer.Id}",
                     "Officer Management"
                 );

                 return ResponseFactory.Success(response, "Assistant Officer updated successfully");
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error updating assistant officer");
                 await CreateAuditLog(
                     "Update Failed",
                     $"CorrelationId: {correlationId} - Error: {ex.Message}",
                     "Officer Management"
                 );
                 return ResponseFactory.Fail<AssistantOfficerResponseDto>(ex, "An unexpected error occurred");
             }
         }
         public async Task<BaseResponse<AssistantOfficerResponseDto>> GetAssistantOfficerById(string officerId)
         {
             var correlationId = Guid.NewGuid().ToString();
             try
             {
                 await CreateAuditLog(
                     "Assistant Officer Query",
                     $"CorrelationId: {correlationId} - Fetching officer: {officerId}",
                     "Officer Management"
                 );

                 var officer = await _repository.AssistCenterOfficerRepository.GetByIdAsync(officerId, false);
                 if (officer == null)
                 {
                     await CreateAuditLog(
                         "Assistant Officer Query Failed",
                         $"CorrelationId: {correlationId} - Officer not found",
                         "Officer Management"
                     );
                     return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                         new NotFoundException("Assistant Officer not found"),
                         "Not found");
                 }

                 await CreateAuditLog(
                     "Assistant Officer Retrieved",
                     $"CorrelationId: {correlationId} - Officer details retrieved successfully",
                     "Officer Management"
                 );

                 return ResponseFactory.Success(_mapper.Map<AssistantOfficerResponseDto>(officer),
                     "Assistant Officer retrieved successfully");
             }
             catch (Exception ex)
             {
                 await CreateAuditLog(
                     "Assistant Officer Query Failed",
                     $"CorrelationId: {correlationId} - Error: {ex.Message}",
                     "Officer Management"
                 );
                 return ResponseFactory.Fail<AssistantOfficerResponseDto>(ex, "An unexpected error occurred");
             }
         }*/
        public async Task<BaseResponse<LevyResponseDto>> GetLevyById(string levyId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Levy Details Query",
                    $"CorrelationId: {correlationId} - Fetching levy: {levyId}",
                    "Levy Management"
                );

                var levy = await _repository.LevyPaymentRepository.GetPaymentById(levyId, false);
                if (levy == null)
                {
                    await CreateAuditLog(
                        "Levy Details Query Failed",
                        $"CorrelationId: {correlationId} - Levy not found",
                        "Levy Management"
                    );
                    return ResponseFactory.Fail<LevyResponseDto>(
                        new NotFoundException("Levy not found"),
                        "Not found");
                }

                await CreateAuditLog(
                    "Levy Details Retrieved",
                    $"CorrelationId: {correlationId} - Levy details retrieved successfully",
                    "Levy Management"
                );

                return ResponseFactory.Success(_mapper.Map<LevyResponseDto>(levy),
                    "Levy retrieved successfully.");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Levy Details Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Levy Management"
                );
                return ResponseFactory.Fail<LevyResponseDto>(ex, "Error retrieving levy");
            }
        }
        public async Task<BaseResponse<bool>> AssignCaretakerToMarket(string marketId, string caretakerId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Caretaker Market Assignment",
                    $"CorrelationId: {correlationId} - Assigning caretaker {caretakerId} to market {marketId}",
                    "Market Management"
                );

                var market = await _repository.MarketRepository.GetMarketByIdAsync(marketId, true);
                if (market == null)
                {
                    await CreateAuditLog(
                        "Assignment Failed",
                        $"CorrelationId: {correlationId} - Market not found",
                        "Market Management"
                    );
                    return ResponseFactory.Fail<bool>(new NotFoundException("Market not found"), "Market not found");
                }

                var caretaker = await _repository.CaretakerRepository.GetCaretakerById(caretakerId, true);
                if (caretaker == null)
                {
                    await CreateAuditLog(
                        "Assignment Failed",
                        $"CorrelationId: {correlationId} - Caretaker not found",
                        "Market Management"
                    );
                    return ResponseFactory.Fail<bool>(new NotFoundException("Caretaker not found"), "Caretaker not found");
                }

                // Check if caretaker is already assigned to this market
                if (market.CaretakerId == caretakerId)
                {
                    await CreateAuditLog(
                        "Assignment Failed",
                        $"CorrelationId: {correlationId} - Caretaker already assigned to market",
                        "Market Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new InvalidOperationException("Caretaker is already assigned"),
                        "Already assigned");
                }

                // Assign the caretaker to the market
                market.CaretakerId = caretakerId;
                market.Caretaker = caretaker;  // Set the navigation property

                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Assignment Successful",
                    $"CorrelationId: {correlationId} - Caretaker successfully assigned to market",
                    "Market Management"
                );

                return ResponseFactory.Success(true, "Caretaker assigned successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Assignment Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Market Management"
                );
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<ChairmanResponseDto>> CreateChairman(CreateChairmanRequestDto request)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Chairman Creation",
                    $"CorrelationId: {correlationId} - Creating new chairman: {request.FullName}",
                    "Chairman Management"
                );

                // Validate request
                var validationResult = await _createChairmanValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - Validation failed",
                        "Chairman Management"
                    );
                    return ResponseFactory.Fail<ChairmanResponseDto>(
                        new ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - Email already registered",
                        "Chairman Management"
                    );
                    return ResponseFactory.Fail<ChairmanResponseDto>(
                        new BadRequestException("Email address is already registered"),
                        "Email already exists");
                }

                // Check if LocalGovernment already has a chairman
                var existingChairman = await _repository.ChairmanRepository.GetChairmanByIdAsync(request.LocalGovernmentId, false);
                if (existingChairman != null)
                {
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - LocalGovernment already has a chairman",
                        "Chairman Management"
                    );
                    return ResponseFactory.Fail<ChairmanResponseDto>(
                        new BadRequestException("LocalGovernment already has an assigned chairman"),
                        "Chairman already exists for this LocalGovernment");
                }

                // Create ApplicationUser
                var defaultPassword = GenerateDefaultPassword(request.FullName);
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = request.Email,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    FirstName = request.FullName.Split(' ')[0],
                    LastName = request.FullName.Split(' ').Length > 1 ? string.Join(" ", request.FullName.Split(' ').Skip(1)) : "",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Gender = "",
                    ProfileImageUrl = "",
                    LocalGovernmentId = request.LocalGovernmentId
                };

                var createUserResult = await _userManager.CreateAsync(user, defaultPassword);
                if (!createUserResult.Succeeded)
                {
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - Failed to create user account",
                        "Chairman Management"
                    );
                    return ResponseFactory.Fail<ChairmanResponseDto>(
                        new Exception(string.Join(", ", createUserResult.Errors.Select(e => e.Description))),
                        "Failed to create user account");
                }

                // Assign role
                var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.Chairman);
                if (!roleResult.Succeeded)
                {
                    // Rollback user creation if role assignment fails
                    await _userManager.DeleteAsync(user);
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - Failed to assign chairman role",
                        "Chairman Management"
                    );
                    return ResponseFactory.Fail<ChairmanResponseDto>(
                        new Exception("Failed to assign chairman role"),
                        "Role assignment failed");
                }

                // Create Chairman entity
                var chairman = new Chairman
                {
                    UserId = user.Id,
                    Title = "Honorable",
                    Office = "Chairman",
                    LocalGovernmentId = request.LocalGovernmentId,
                    FullName = request.FullName,
                    Email = request.Email,
                    TermStart = DateTime.UtcNow,
                    TermEnd = DateTime.UtcNow.AddYears(8),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    User = user
                };

                _repository.ChairmanRepository.CreateChairman(chairman);
                await _repository.SaveChangesAsync();

                // Map response
                var response = _mapper.Map<ChairmanResponseDto>(chairman);
                response.DefaultPassword = defaultPassword;

                await CreateAuditLog(
                    "Chairman Created",
                    $"CorrelationId: {correlationId} - Chairman created successfully with ID: {chairman.Id}",
                    "Chairman Management"
                );

                return ResponseFactory.Success(response,
                    "Chairman created successfully. Please note down the default password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chairman");
                await CreateAuditLog(
                    "Creation Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Chairman Management"
                );
                return ResponseFactory.Fail<ChairmanResponseDto>(ex, "An unexpected error occurred");
            }
        }

        private string GenerateDefaultPassword(string fullName)
        {
            // Handle null or empty fullName
            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = "User";
            }

            var nameParts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.Length > 0 ? nameParts[0] : "User";
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            var random = new Random();
            var randomNumbers = random.Next(100, 999).ToString(); // Generate a 3-digit random number

            // Special characters pool
            var specialChars = "!@#$%^&*(),.?\":{}|<>";
            var specialChar = specialChars[random.Next(specialChars.Length)];

            // Generate random uppercase letter
            var uppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var uppercaseLetter = uppercaseLetters[random.Next(uppercaseLetters.Length)];

            // Generate random lowercase letter - ensure we have at least one
            var lowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
            var lowercaseLetter = lowercaseLetters[random.Next(lowercaseLetters.Length)];

            // Process name parts - ensure they're properly formatted
            string firstNameProcessed = "";
            if (firstName.Length > 0)
            {
                firstNameProcessed = char.ToUpper(firstName[0]) +
                    (firstName.Length > 1 ? firstName.Substring(1).ToLower() : "");
            }

            string lastNameProcessed = "";
            if (!string.IsNullOrEmpty(lastName) && lastName.Length > 0)
            {
                lastNameProcessed = char.ToUpper(lastName[0]) +
                    (lastName.Length > 1 ? lastName.Substring(1).ToLower() : "");
            }

            // Combine all elements ensuring we have all required character types
            var passwordParts = new List<string>
    {
        firstNameProcessed,
        lastNameProcessed,
        randomNumbers,          // Ensures numbers
        uppercaseLetter.ToString(), // Ensures uppercase
        lowercaseLetter.ToString(), // Ensures lowercase
        specialChar.ToString()  // Ensures special character
    };

            // Remove any empty parts
            passwordParts.RemoveAll(string.IsNullOrEmpty);

            // Shuffle the parts for better security
            ShuffleList(passwordParts, random);

            var password = string.Join("", passwordParts);

            // Ensure minimum length of 8 characters
            if (password.Length < 8)
            {
                var additionalChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
                while (password.Length < 8)
                {
                    password += additionalChars[random.Next(additionalChars.Length)];
                }
            }

            // Final verification to ensure the password meets all requirements
            if (!ContainsUppercase(password)) password += uppercaseLetters[random.Next(uppercaseLetters.Length)];
            if (!ContainsLowercase(password)) password += lowercaseLetters[random.Next(lowercaseLetters.Length)];
            if (!ContainsNumber(password)) password += random.Next(0, 9).ToString();
            if (!ContainsSpecialChar(password)) password += specialChars[random.Next(specialChars.Length)];

            return password;
        }

        // Helper method to shuffle a list
        private void ShuffleList<T>(List<T> list, Random random)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        // Helper methods to verify password requirements
        private bool ContainsUppercase(string password)
        {
            return password.Any(char.IsUpper);
        }

        private bool ContainsLowercase(string password)
        {
            return password.Any(char.IsLower);
        }

        private bool ContainsNumber(string password)
        {
            return password.Any(char.IsDigit);
        }

        private bool ContainsSpecialChar(string password)
        {
            return password.Any(c => "!@#$%^&*(),.?\":{}|<>".Contains(c));
        }

        /*private string GenerateDefaultPassword(string fullName)
        {
            var nameParts = fullName.Split(' '); // Split the full name into first name and last name
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : ""; // Handle cases where only one name is provided

            var random = new Random();
            var randomNumbers = random.Next(100, 999).ToString(); // Generate a 3-digit random number

            // Special characters pool
            var specialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";
            var specialChar1 = specialChars[random.Next(specialChars.Length)];
            var specialChar2 = specialChars[random.Next(specialChars.Length)];

            // Generate random uppercase letters
            var uppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var uppercaseLetter = uppercaseLetters[random.Next(uppercaseLetters.Length)];

            // Combine first name, last name, random number, and special characters
            // Make sure at least one character in the name parts is uppercase
            var firstNameProcessed = char.ToUpper(firstName[0]) + firstName.Substring(1).ToLower();
            var lastNameProcessed = !string.IsNullOrEmpty(lastName)
                ? char.ToUpper(lastName[0]) + lastName.Substring(1).ToLower()
                : "";

            // Combine all elements with special characters and uppercase
            var password = $"{firstNameProcessed}{specialChar1}{lastNameProcessed}{randomNumbers}{uppercaseLetter}{specialChar2}";

            // Ensure password has minimum complexity
            if (password.Length < 8)
            {
                // Add additional random characters for very short names
                var additionalChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
                while (password.Length < 8)
                {
                    password += additionalChars[random.Next(additionalChars.Length)];
                }
            }

            return password;
        }
*/
        /*  private string GenerateDefaultPassword(string fullName)
          {
              var firstName = fullName.Split(' ')[0];
              var random = new Random();
              var randomNumbers = random.Next(1000, 9999).ToString();
              return $"{firstName}@{randomNumbers}";
          }
  */
        public async Task<BaseResponse<bool>> UpdateChairmanProfile(string chairmanId, UpdateProfileDto profileDto)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                // Get the chairman with tracking
                var chairman = await _repository.ChairmanRepository.GetChairmanById(chairmanId, true);
                if (chairman == null)
                {
                    return ResponseFactory.Fail<bool>("Chairman not found");
                }

                // Validate existing LocalGovernmentId from chairman
                var localGovernmentExists = await _repository.LocalGovernmentRepository
                    .LocalGovernmentExist(chairman.LocalGovernmentId);

                if (!localGovernmentExists)
                {
                    return ResponseFactory.Fail<bool>("Invalid Local Government ID");
                }

                // Only update fields that are not null, empty, or "string" literal
                if (!string.IsNullOrEmpty(profileDto.FullName) && profileDto.FullName != "string")
                    chairman.FullName = profileDto.FullName;

                if (!string.IsNullOrEmpty(profileDto.EmailAddress) && profileDto.EmailAddress != "string")
                    chairman.Email = profileDto.EmailAddress;

                if (chairman.User != null)
                {
                    if (!string.IsNullOrEmpty(profileDto.PhoneNumber) && profileDto.PhoneNumber != "string")
                        chairman.User.PhoneNumber = profileDto.PhoneNumber;

                    if (!string.IsNullOrEmpty(profileDto.Address) && profileDto.Address != "string")
                        chairman.User.Address = profileDto.Address;

                    if (!string.IsNullOrEmpty(profileDto.ProfileImageUrl) && profileDto.ProfileImageUrl != "string")
                        chairman.User.ProfileImageUrl = profileDto.ProfileImageUrl;
                }

                await _repository.SaveChangesAsync();
                return ResponseFactory.Success(true, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating chairman profile: {ChairmanId}", chairmanId);
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<PaginatorDto<IEnumerable<AuditLogDto>>>> GetAuditLogs(PaginationFilter filter)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Audit Logs Query",
                    $"CorrelationId: {correlationId} - Retrieving audit logs - Page: {filter.PageNumber}, Size: {filter.PageSize}",
                    "Audit Management"
                );

                var auditLogs = await _repository.AuditLogRepository.GetPagedAuditLogs(filter);
                var auditLogDtos = _mapper.Map<IEnumerable<AuditLogDto>>(auditLogs.PageItems);

                var result = new PaginatorDto<IEnumerable<AuditLogDto>>
                {
                    PageItems = auditLogDtos,
                    CurrentPage = filter.PageNumber,
                    PageSize = filter.PageSize,
                    NumberOfPages = auditLogs.NumberOfPages
                };

                await CreateAuditLog(
                    "Audit Logs Retrieved",
                    $"CorrelationId: {correlationId} - Retrieved {auditLogDtos.Count()} logs",
                    "Audit Management"
                );

                return ResponseFactory.Success(result, "Audit logs retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Audit Logs Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Audit Management"
                );
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<AuditLogDto>>>(ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<ReportMetricsDto>> GetReportMetrics(DateTime startDate, DateTime endDate)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Report Metrics Query",
                    $"CorrelationId: {correlationId} - Fetching metrics from {startDate} to {endDate}",
                    "Report Management"
                );

                var metrics = await _repository.ReportRepository.GetMetricsAsync(startDate, endDate);
                var metricsDto = _mapper.Map<ReportMetricsDto>(metrics);

                await CreateAuditLog(
                    "Report Metrics Retrieved",
                    $"CorrelationId: {correlationId} - Metrics retrieved successfully",
                    "Report Management"
                );

                return ResponseFactory.Success(metricsDto, "Report metrics retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Report Metrics Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Report Management"
                );
                return ResponseFactory.Fail<ReportMetricsDto>(ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<DashboardMetricsResponseDto>> GetDailyMetricsChange()
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Daily Metrics Query",
                    $"CorrelationId: {correlationId} - Calculating daily metrics changes",
                    "Dashboard Analytics"
                );

                string preset = DateRangePresets.Today;
                var dateRange = DateRangePresets.GetPresetRange(preset);
                var previousDateRange = GetPreviousDateRange(dateRange);

                var currentMetrics = await _repository.ReportRepository.GetMetricsAsync(dateRange.StartDate, dateRange.EndDate);
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

                await CreateAuditLog(
                    "Daily Metrics Retrieved",
                    $"CorrelationId: {correlationId} - Daily metrics changes calculated successfully",
                    "Dashboard Analytics"
                );

                return ResponseFactory.Success(response, "Daily metrics changes retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Daily Metrics Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Dashboard Analytics"
                );
                return ResponseFactory.Fail<DashboardMetricsResponseDto>(ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<PaginatorDto<IEnumerable<LevyResponseDto>>>> GetAllLevies(string chairmanId, PaginationFilter filter)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "All Levies Query",
                    $"CorrelationId: {correlationId} - Fetching all levies for chairman: {chairmanId}",
                    "Levy Management"
                );

                var levyPayments = await _repository.LevyPaymentRepository.GetLevyPaymentsAsync(chairmanId, filter, false);
                var levyDtos = _mapper.Map<IEnumerable<LevyResponseDto>>(levyPayments.PageItems);

                var paginatedResult = new PaginatorDto<IEnumerable<LevyResponseDto>>
                {
                    PageItems = levyDtos,
                    PageSize = filter.PageSize,
                    CurrentPage = filter.PageNumber,
                    NumberOfPages = levyPayments.NumberOfPages
                };

                await CreateAuditLog(
                    "Levies Retrieved",
                    $"CorrelationId: {correlationId} - Retrieved {levyDtos.Count()} levies",
                    "Levy Management"
                );

                return ResponseFactory.Success(paginatedResult, "Levies retrieved successfully.");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Levies Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Levy Management"
                );
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<LevyResponseDto>>>(ex, "Error retrieving levies");
            }
        }
        public async Task<BaseResponse<PaginatorDto<IEnumerable<LevyInfoResponseDto>>>> GetMarketLevies(string marketId, PaginationFilter paginationFilter)
        {
            try
            {
                var chairmanId = _currentUser.GetUserId();
                var market = await _repository.MarketRepository.GetMarketByIdAsync(marketId, false);

                if (market == null || market.ChairmanId != chairmanId)
                    return ResponseFactory.Fail<PaginatorDto<IEnumerable<LevyInfoResponseDto>>>(
                        new UnauthorizedException("Unauthorized to view this market's levies"),
                        "Unauthorized access");

                var query = await _repository.LevyPaymentRepository.GetMarketLevySetups(marketId);
                var paginatedLevies = await query.Paginate(paginationFilter);

                var result = new PaginatorDto<IEnumerable<LevyInfoResponseDto>>
                {
                    PageItems = _mapper.Map<IEnumerable<LevyInfoResponseDto>>(paginatedLevies.PageItems),
                    PageSize = paginatedLevies.PageSize,
                    CurrentPage = paginatedLevies.CurrentPage,
                    NumberOfPages = paginatedLevies.NumberOfPages
                };

                return ResponseFactory.Success(result, "Market levy configurations retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving market levy configurations");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<LevyInfoResponseDto>>>(ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<MarketRevenueDto>> GetMarketRevenue(string marketId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Market Revenue Query",
                    $"CorrelationId: {correlationId} - Fetching revenue for market: {marketId}",
                    "Market Management"
                );

                string preset = DateRangePresets.ThisMonth;
                var dateRange = DateRangePresets.GetPresetRange(preset);
                var revenue = await _repository.MarketRepository.GetMarketRevenueAsync(
                    marketId,
                    dateRange.StartDate,
                    dateRange.EndDate
                );
                var revenueDto = _mapper.Map<MarketRevenueDto>(revenue);

                await CreateAuditLog(
                    "Market Revenue Retrieved",
                    $"CorrelationId: {correlationId} - Successfully retrieved revenue data",
                    "Market Management"
                );

                return ResponseFactory.Success(revenueDto, "Market revenue retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Market Revenue Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Market Management"
                );
                _logger.LogError(ex, "Error retrieving market revenue");
                return ResponseFactory.Fail<MarketRevenueDto>(ex, "An unexpected error occurred");
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
                var market = await _repository.MarketRepository.GetMarketById(request.MarketId, false);
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
        private string GenerateTransactionReference()
        {
            return $"LVY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}".ToUpper();
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
        private decimal CalculatePercentageChange(decimal previous, decimal current)
        {
            if (previous == 0) return 100;
            return ((current - previous) / previous) * 100;
        }

        public async Task<BaseResponse<bool>> DeleteChairmanByAdmin(string chairmanId)
        {
            var correlationId = Guid.NewGuid().ToString();
            var adminId = _currentUser.GetUserId();
            try
            {
                // First verify the admin exists and has proper permissions
                var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: false);
                if (admin == null)
                {
                    await CreateAuditLog(
                        "Chairman Deletion Failed",
                        $"CorrelationId: {correlationId} - Admin not found with ID: {adminId}",
                        "Chairman Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Admin not found"),
                        "Admin not found");
                }

                // Check if admin has role management permissions
                if (!admin.HasRoleManagementAccess)
                {
                    await CreateAuditLog(
                        "Chairman Deletion Failed",
                        $"CorrelationId: {correlationId} - Admin does not have permission to manage roles: {adminId}",
                        "Chairman Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new UnauthorizedException("Admin does not have permission to manage roles"),
                        "Admin does not have permission to manage roles");
                }

                // Now get the chairman to delete
                var chairman = await _repository.ChairmanRepository.GetChairmanById(chairmanId, trackChanges: true);
                if (chairman == null)
                {
                    await CreateAuditLog(
                        "Chairman Deletion Failed",
                        $"CorrelationId: {correlationId} - Chairman not found with ID: {chairmanId}",
                        "Chairman Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Chairman not found"),
                        "Chairman not found");
                }

                // Check if there are any dependencies (e.g., markets, caretakers) before deletion
                var hasActiveDependencies = await CheckChairmanDependencies(chairman);
                if (hasActiveDependencies)
                {
                    await CreateAuditLog(
                        "Chairman Deletion Failed",
                        $"CorrelationId: {correlationId} - Chairman has active dependencies",
                        "Chairman Management"
                    );
                    /*return ResponseFactory.Fail<bool>(
                        new InvalidOperationException("Chairman has active dependencies"),
                        "Cannot delete chairman with active dependencies");*/
                }

                // Get associated user
                var user = await _userManager.FindByIdAsync(chairman.UserId);
                if (user != null)
                {
                    // Remove chairman role from user
                    await _userManager.RemoveFromRoleAsync(user, UserRoles.Chairman);

                    // Update user status - you might want to uncomment this depending on your business logic
                    // user.IsActive = false;
                    await _userManager.UpdateAsync(user);
                }

                // Delete chairman
                _repository.ChairmanRepository.DeleteChairman(chairman);
                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Chairman Deleted",
                    $"CorrelationId: {correlationId} - Admin {adminId} successfully deleted chairman with ID: {chairmanId}",
                    "Chairman Management"
                );

                return ResponseFactory.Success(true, "Chairman deleted successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Chairman Deletion Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Chairman Management"
                );
                _logger.LogError(ex, "Error deleting chairman: {ChairmanId} by admin: {AdminId}", chairmanId, adminId);
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred while deleting the chairman");
            }
        }


        /*    public async Task<BaseResponse<bool>> DeleteChairmanById(string chairmanId)
            {
                var correlationId = Guid.NewGuid().ToString();
                try
                {
                    await CreateAuditLog(
                        "Chairman Deletion",
                        $"CorrelationId: {correlationId} - Attempting to delete chairman: {chairmanId}",
                        "Chairman Management"
                    );

                    // Get chairman with tracking for deletion
                    var chairman = await _repository.ChairmanRepository.GetChairmanById(chairmanId, true);
                    if (chairman == null)
                    {
                        await CreateAuditLog(
                            "Chairman Deletion Failed",
                            $"CorrelationId: {correlationId} - Chairman not found with ID: {chairmanId}",
                            "Chairman Management"
                        );
                        return ResponseFactory.Fail<bool>(
                            new NotFoundException("Chairman not found"),
                            "Chairman not found");
                    }

                    // Check if there are any dependencies (e.g., markets, caretakers) before deletion
                    var hasActiveDependencies = await CheckChairmanDependencies(chairman);
                    if (hasActiveDependencies)
                    {
                        await CreateAuditLog(
                            "Chairman Deletion Failed",
                            $"CorrelationId: {correlationId} - Chairman has active dependencies and cannot be deleted",
                            "Chairman Management"
                        );
                        return ResponseFactory.Fail<bool>(
                            new InvalidOperationException("Chairman has active dependencies"),
                            "Cannot delete chairman with active dependencies");
                    }

                    // Get associated user
                    var user = await _userManager.FindByIdAsync(chairman.UserId);
                    if (user != null)
                    {
                        // Remove chairman role from user
                        await _userManager.RemoveFromRoleAsync(user, UserRoles.Chairman);

                        // Update user status
                        //user.IsActive = false;
                        await _userManager.UpdateAsync(user);
                    }

                    // Delete chairman
                    _repository.ChairmanRepository.DeleteChairman(chairman);
                    await _repository.SaveChangesAsync();

                    await CreateAuditLog(
                        "Chairman Deleted",
                        $"CorrelationId: {correlationId} - Successfully deleted chairman with ID: {chairmanId}",
                        "Chairman Management"
                    );

                    return ResponseFactory.Success(true, "Chairman deleted successfully");
                }
                catch (Exception ex)
                {
                    await CreateAuditLog(
                        "Chairman Deletion Failed",
                        $"CorrelationId: {correlationId} - Error: {ex.Message}",
                        "Chairman Management"
                    );
                    _logger.LogError(ex, "Error deleting chairman: {ChairmanId}", chairmanId);
                    return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred while deleting the chairman");
                }
            }
    */
        private async Task<bool> CheckChairmanDependencies(Chairman chairman)
        {
            // Check for active markets
            var hasActiveMarkets = await _repository.MarketRepository
                .GetMarketsQuery()
                .AnyAsync(m => m.ChairmanId == chairman.Id && m.IsActive);

            if (hasActiveMarkets)
            {
                return true;
            }

            // Check for active caretakers
            var hasActiveCaretakers = await _repository.CaretakerRepository
                .GetCaretakersQuery()
                .AnyAsync(c => c.ChairmanId == chairman.Id && c.IsActive);

            if (hasActiveCaretakers)
            {
                return true;
            }

            // Check for pending levy payments
            var hasPendingLevies = await _repository.LevyPaymentRepository
                .GetPaymentsQuery()
                .AnyAsync(l => l.ChairmanId == chairman.Id && l.PaymentStatus == PaymentStatusEnum.Pending);

            return hasPendingLevies;
        }
    }
}



/*public async Task<BaseResponse<bool>> UpdateChairmanProfile(string chairmanId, UpdateProfileDto profileDto)
{
    var correlationId = Guid.NewGuid().ToString();
    try
    {
        await CreateAuditLog(
            "Profile Update",
            $"CorrelationId: {correlationId} - Updating chairman profile: {chairmanId}",
            "Chairman Management"
        );

       *//* var validationResult = await _updateProfileValidator.ValidateAsync(profileDto);
        if (!validationResult.IsValid)
        {
            await CreateAuditLog(
                "Update Failed",
                $"CorrelationId: {correlationId} - Validation failed",
                "Chairman Management"
            );
            return ResponseFactory.Fail<bool>(
                new ValidationException(validationResult.Errors),
                "Validation failed");
        }*//*
       if(string.IsNullOrEmpty(chairmanId))
       {
            await CreateAuditLog(
                $"{chairmanId}: ChairmanId is null",
                $"CorrelationId: {correlationId} - {chairmanId} : chairmanId is null",
                "Chairman Management"
            );

            return ResponseFactory.Fail<bool>("chairman is not found");
        }

        var chairman = await _repository.ChairmanRepository.GetChairmanById(chairmanId, true);
        _mapper.Map(profileDto, chairman);
        await _repository.SaveChangesAsync();

        await CreateAuditLog(
            "Profile Updated",
            $"CorrelationId: {correlationId} - Profile updated successfully",
            "Chairman Management"
        );

        return ResponseFactory.Success(true, "Profile updated successfully");
    }
    catch (Exception ex)
    {
        await CreateAuditLog(
            "Update Failed",
            $"CorrelationId: {correlationId} - Error: {ex.Message}",
            "Chairman Management"
        );
        return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
    }
}*/