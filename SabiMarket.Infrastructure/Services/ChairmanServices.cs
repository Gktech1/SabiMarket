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
using SabiMarket.Application.DTOs.Responses.SabiMarket.Application.DTOs.Responses;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.LevyManagement;

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


        public ChairmanService(
            IRepositoryManager repository,
            ILogger<ChairmanService> logger,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IValidator<CreateChairmanRequestDto> createChairmanValidator,
            IValidator<UpdateProfileDto> updateProfileValidator,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _createChairmanValidator = createChairmanValidator;
            _updateProfileValidator = updateProfileValidator;
            _currentUser = currentUser;
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
                var totalTraders = await _repository.TraderRepository.GetTraderCountAsync();
                var totalCaretakers = await _repository.CaretakerRepository.GetCaretakerCountAsync();
                var totalLevies = await _repository.LevyPaymentRepository.GetTotalLeviesAsync();

                var response = new DashboardMetricsResponseDto
                {
                    TotalTraders = totalTraders,
                    TotalCaretakers = totalCaretakers,
                    TotalLeviesCollected = totalLevies
                };

                return ResponseFactory.Success(response, "Dashboard metrics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard metrics");
                return ResponseFactory.Fail<DashboardMetricsResponseDto>(ex, "An unexpected error occurred");
            }
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
                var levy = _mapper.Map<LevyPayment>(request);
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
                var levy = await _repository.LevyPaymentRepository.GetPaymentById(levyId, trackChanges: true);
                if (levy == null)
                {
                    return ResponseFactory.Fail<bool>(new NotFoundException("Levy not found"), "Levy not found.");
                }

                _mapper.Map(request, levy);
                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "Levy updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating levy");
                return ResponseFactory.Fail<bool>(ex, "Error updating levy");
            }
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

    }

}




