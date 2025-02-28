
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
using SabiMarket.Infrastructure.Utilities;
using SabiMarket.Domain.Entities;
using Microsoft.AspNetCore.Http;
using ValidationException = FluentValidation.ValidationException;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using iText.Commons.Actions.Contexts;

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
            IValidator<CreateMarketRequestDto> createMarketValidator)
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
        }

        private string GetCurrentIpAddress()
        {
            return _httpContextAccessor.GetRemoteIPAddress();
        }
        private async Task CreateAuditLog(string activity, string details, string module = "Chairman Management")
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

        /*  public async Task<BaseResponse<PaginatorDto<IEnumerable<LGResponseDto>>>> GetLocalGovernmentAreas(
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

                  var lgaDtos = result.PageItems.Select(lga => new LGResponseDto
                  {
                      Id = lga.Id,
                      LocalGovernmentArea = lga.Name,
                      LGAChairman = lga.AssistCenterOfficers
                          .FirstOrDefault()?.User != null ?
                          $"{lga.AssistCenterOfficers.FirstOrDefault().User.FirstName} {lga.AssistCenterOfficers.FirstOrDefault().User.LastName}" :
                          "Not Assigned"
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
          }*/
        /*  public async Task<BaseResponse<PaginatorDto<IEnumerable<LGResponseDto>>>> GetLocalGovernmentAreas(
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

                  // Use existing repository method with minimal filtering
                  var result = await _repository.LocalGovernmentRepository.GetLocalGovernmentArea(
                      searchTerm, paginationFilter);
                  // Map to simplified DTO structure matching UI
                  var lgaDtos = result.PageItems.Select(lga => new LGResponseDto
                  {
                      Id = lga.Id,
                      LocalGovernmentArea = lga.Name,
                      LGAChairman = lga.AssistCenterOfficers
                          .FirstOrDefault()?.User != null ?
                          $"{lga.AssistCenterOfficers.FirstOrDefault().User.FirstName} {lga.AssistCenterOfficers.FirstOrDefault().User.LastName}" :
                          "Not Assigned"
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
          }*/
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

        public async Task<BaseResponse<PaginatorDto<IEnumerable<ChairmanResponseDto>>>> GetChairmen(
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
                var chairmanDtos = _mapper.Map<IEnumerable<ChairmanResponseDto>>(chairmenPage.PageItems);

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
                market.Location= request.MarketName; 
                market.LocalGovernmentId = chairman.LocalGovernmentId;
                market.LocalGovernmentName = localGovernment.Name; // Ensure this is correctly assigned
                market.StartDate = DateTime.UtcNow;
                market.MarketCapacity = 0;
                market.ChairmanId = chairman.Id;

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


        /* public async Task<BaseResponse<MarketResponseDto>> CreateMarket(CreateMarketRequestDto request)
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

                 var market = _mapper.Map<Market>(request);

                 // Verify caretaker exists only if CaretakerId is provided
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
                     market.CaretakerId = request.CaretakerId;
                 }
                 else
                 {
                     // Ensure CaretakerId is null if not provided
                     market.CaretakerId = null;
                 }

                 // Get Local Government and Chairman Details 
                 var chairman = await _repository.ChairmanRepository.GetChairmanById(userId, false);
                 if (chairman == null)
                 {
                     return ResponseFactory.Fail<MarketResponseDto>(
                         new NotFoundException("Chairman not found"),
                         "Invalid chairman"
                     );
                 }

                 market.ChairmanId = chairman.Id; // Assign only if chairman exists

                 await CreateAuditLog(
                     "Market Creation",
                     $"CorrelationId: {correlationId} - Creating new market: {request.MarketName}",
                     "Market Management"
                 );

                 // Set default values for required fields
                 market.Id = Guid.NewGuid().ToString();
                 market.IsActive = true;
                 market.Location = request.MarketName;
                 market.MarketName = request.MarketName;
                 market.LocalGovernmentName = chairman.LocalGovernment.Name;
                 market.StartDate = DateTime.UtcNow;
                 market.MarketCapacity = 0;
                 market.LocalGovernmentId = chairman.LocalGovernmentId;
                 market.Chairman = chairman; 
                 market.Chairman.UserId = userId; 

                 _repository.MarketRepository.AddMarket(market);
                 await _repository.SaveChangesAsync();

                 await CreateAuditLog(
                     "Market Created",
                     $"CorrelationId: {correlationId} - Market created successfully with ID: {market.Id}",
                     "Market Management"
                 );

                 return ResponseFactory.Success(_mapper.Map<MarketResponseDto>(market), "Market created successfully");
             }
             catch (Exception ex)
             {
                 await CreateAuditLog(
                     "Market Creation Failed",
                     $"CorrelationId: {correlationId} - Error: {ex.Message}",
                     "Market Management"
                 );
                 return ResponseFactory.Fail<MarketResponseDto>(ex, "Error creating market");
             }
         }*/

        /*    public async Task<BaseResponse<MarketResponseDto>> CreateMarket(CreateMarketRequestDto request)
            {
                var correlationId = Guid.NewGuid().ToString();
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

                    var market = _mapper.Map<Market>(request);

                    // Verify caretaker exists only if CaretakerId is provided
                    // Only set market.CaretakerId if request.CaretakerId has a value
                    if (!string.IsNullOrEmpty(request.CaretakerId) && request.CaretakerId != "string")
                    {
                        // Verify caretaker exists
                        var caretaker = await _repository.CaretakerRepository.GetCaretakerById(request.CaretakerId, false);

                        if (caretaker == null)
                        {
                            return ResponseFactory.Fail<MarketResponseDto>(
                                new NotFoundException("Caretaker not found"),
                                "Invalid caretaker"
                            );
                        }

                        market.CaretakerId = request.CaretakerId;
                    }
                    else
                    {
                        // Ensure CaretakerId is null, not empty string
                        market.CaretakerId = null;
                    }


                    await CreateAuditLog(
                        "Market Creation",
                        $"CorrelationId: {correlationId} - Creating new market: {request.MarketName}",
                        "Market Management"
                    );


                    // Set default values for required fields
                   // market.LocalGovernmentId = ""; // Assuming caretaker has this
                    market.Id = Guid.NewGuid().ToString();   
                    market.IsActive = true;
                    market.Location = "";
                    market.LocalGovernmentName = string.Empty;  
                    market.StartDate = DateTime.UtcNow;
                    market.MarketCapacity = 0; // Can be updated later

                    _repository.MarketRepository.AddMarket(market);
                    await _repository.SaveChangesAsync();

                    await CreateAuditLog(
                        "Market Created",
                        $"CorrelationId: {correlationId} - Market created successfully with ID: {market.Id}",
                        "Market Management"
                    );

                    return ResponseFactory.Success(_mapper.Map<MarketResponseDto>(market), "Market created successfully");
                }
                catch (Exception ex)
                {
                    await CreateAuditLog(
                        "Market Creation Failed",
                        $"CorrelationId: {correlationId} - Error: {ex.Message}",
                        "Market Management"
                    );
                    return ResponseFactory.Fail<MarketResponseDto>(ex, "Error creating market");
                }
            }
    */
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

        /*   public async Task<BaseResponse<MarketResponseDto>> CreateMarket(CreateMarketRequestDto request)
           {
               var correlationId = Guid.NewGuid().ToString();
               try
               {
                   await CreateAuditLog(
                       "Market Creation",
                       $"CorrelationId: {correlationId} - Creating new market: {request.}",
                       "Market Management"
                   );

                   var market = _mapper.Map<Market>(request);
                   _repository.MarketRepository.AddMarket(market);
                   await _repository.SaveChangesAsync();

                   await CreateAuditLog(
                       "Market Created",
                       $"CorrelationId: {correlationId} - Market created successfully with ID: {market.Id}",
                       "Market Management"
                   );

                   return ResponseFactory.Success(_mapper.Map<MarketResponseDto>(market), "Market created successfully");
               }
               catch (Exception ex)
               {
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
                   await CreateAuditLog(
                       "Market Update",
                       $"CorrelationId: {correlationId} - Updating market {marketId}",
                       "Market Management"
                   );

                   var market = await _repository.MarketRepository.GetMarketByIdAsync(marketId, true);
                   _mapper.Map(request, market);
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
   */
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
                return ResponseFactory.Success(_mapper.Map<MarketDetailsDto>(market), "Market details retrieved successfully");
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
        }

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


     /*   public async Task<BaseResponse<bool>> ConfigureLevySetup(LevySetupRequestDto request)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Levy Setup Configuration",
                    $"CorrelationId: {correlationId} - Configuring new levy setup",
                    "Levy Management"
                );

                var levySetup = _mapper.Map<LevyPayment>(request);
                _repository.LevyPaymentRepository.AddPayment(levySetup);
                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Levy Setup Configured",
                    $"CorrelationId: {correlationId} - Levy setup configured successfully",
                    "Levy Management"
                );

                return ResponseFactory.Success(true, "Levy setup configured successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Levy Setup Configuration Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Levy Management"
                );
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }
*/
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

        public async Task<BaseResponse<IEnumerable<MarketResponseDto>>> GetAllMarkets()
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Markets List Query",
                    $"CorrelationId: {correlationId} - Retrieving all markets",
                    "Market Management"
                );

                var markets = await _repository.MarketRepository.GetAllMarketForExport(false);
                var marketDtos = _mapper.Map<IEnumerable<MarketResponseDto>>(markets);

                await CreateAuditLog(
                    "Markets Retrieved",
                    $"CorrelationId: {correlationId} - Retrieved {marketDtos.Count()} markets",
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

        public async Task<BaseResponse<bool>> DeleteMarket(string marketId)
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

        public async Task<BaseResponse<AssistantOfficerResponseDto>> CreateAssistantOfficer(CreateAssistantOfficerRequestDto officerDto)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Assistant Officer Creation",
                    $"CorrelationId: {correlationId} - Creating new assistant officer",
                    "Officer Management"
                );

                var validationResult = await _createAssistOfficerValidator.ValidateAsync(officerDto);
                if (!validationResult.IsValid)
                {
                    await CreateAuditLog(
                        "Assistant Officer Creation Failed",
                        $"CorrelationId: {correlationId} - Validation failed",
                        "Officer Management"
                    );
                    return ResponseFactory.Fail<AssistantOfficerResponseDto>(
                        new FluentValidation.ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                var officer = _mapper.Map<AssistCenterOfficer>(officerDto);
                _repository.AssistCenterOfficerRepository.AddAssistCenterOfficer(officer);
                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Assistant Officer Created",
                    $"CorrelationId: {correlationId} - Officer created with ID: {officer.Id}",
                    "Officer Management"
                );

                return ResponseFactory.Success(_mapper.Map<AssistantOfficerResponseDto>(officer),
                    "Assistant Officer created successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Assistant Officer Creation Failed",
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
        }

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
        /* public async Task<BaseResponse<bool>> AssignCaretakerToMarket(string marketId, string caretakerId)
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

                 if (market.Caretaker.Any(c => c.Id == caretakerId))
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

                 market.Caretaker.Add(caretaker);
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
 */
        /*    public async Task<BaseResponse<ChairmanResponseDto>> CreateChairman(CreateChairmanRequestDto chairmanDto)
            {
                var correlationId = Guid.NewGuid().ToString();
                try
                {
                    await CreateAuditLog(
                        "Chairman Creation",
                        $"CorrelationId: {correlationId} - Creating new chairman",
                        "Chairman Management"
                    );

                    var userId = _currentUser.GetUserId();
                    var validationResult = await _createChairmanValidator.ValidateAsync(chairmanDto);
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

                    // Set default values
                    chairmanDto.UserId = userId;
                    chairmanDto.Title = "Honorable";
                    chairmanDto.Office = "Chairman";
                    chairmanDto.Password = GenerateDefaultPassword(chairmanDto.FullName);

                    var chairman = _mapper.Map<Chairman>(chairmanDto);
                    _repository.ChairmanRepository.CreateChairman(chairman);
                    await _repository.SaveChangesAsync();

                    var responseDto = _mapper.Map<ChairmanResponseDto>(chairman);
                    responseDto.DefaultPassword = chairmanDto.Password;

                    await CreateAuditLog(
                        "Chairman Created",
                        $"CorrelationId: {correlationId} - Chairman created successfully with ID: {chairman.Id}",
                        "Chairman Management"
                    );

                    return ResponseFactory.Success(responseDto,
                        "Chairman created successfully. Please note down the default password.");
                }
                catch (Exception ex)
                {
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - Error: {ex.Message}",
                        "Chairman Management"
                    );
                    return ResponseFactory.Fail<ChairmanResponseDto>(ex, "An unexpected error occurred");
                }
            }
    */

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
            var nameParts = fullName.Split(' '); // Split the full name into first name and last name
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : ""; // Handle cases where only one name is provided

            var random = new Random();
            var randomNumbers = random.Next(100, 999).ToString(); // Generate a 3-digit random number

            // Combine first name, last name, and random number
            var password = $"{firstName}{lastName}{randomNumbers}";

            // Ensure the password is exactly 10 characters long
            return password.Length == 10 ? password : password.Substring(0, 10); // Trim to 10 characters if necessary
        }

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

        public async Task<BaseResponse<bool>> DeleteChairmanById(string chairmanId)
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
                var chairman = await _repository.ChairmanRepository.GetChairmanByIdAsync(chairmanId, true);
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