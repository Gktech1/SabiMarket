using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Advertisement;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.IRepositories;
using SabiMarket.Application.Services.Interfaces;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.AdvertisementModule;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Enum;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Domain.Repositories;
using SabiMarket.Domain.Utilities;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Helpers;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Application.Services
{
    public class AdvertisementService : IAdvertisementService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILogger<AdvertisementService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUser;
        private readonly IValidator<CreateAdvertisementRequestDto> _createAdvertValidator;
        private readonly IValidator<UpdateAdvertisementRequestDto> _updateAdvertValidator;
        private readonly IValidator<UploadPaymentProofRequestDto> _uploadPaymentProofValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        public AdvertisementService(
            IRepositoryManager repository,
            ILogger<AdvertisementService> logger,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUser,
            IValidator<CreateAdvertisementRequestDto> createAdvertValidator,
            IValidator<UpdateAdvertisementRequestDto> updateAdvertValidator,
            IValidator<UploadPaymentProofRequestDto> uploadPaymentProofValidator,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _currentUser = currentUser;
            _createAdvertValidator = createAdvertValidator;
            _updateAdvertValidator = updateAdvertValidator;
            _uploadPaymentProofValidator = uploadPaymentProofValidator;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        private string GetCurrentIpAddress()
        {
            return _httpContextAccessor.GetRemoteIPAddress();
        }

        private async Task CreateAuditLog(string activity, string details, string module = "Advertisement Management")
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

        public async Task<BaseResponse<AdvertisementResponseDto>> CreateAdvertisement(CreateAdvertisementRequestDto request)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Advertisement Creation",
                    $"CorrelationId: {correlationId} - Creating new advertisement: {request.Title}",
                    "Advertisement Management"
                );

                // Validate request
                var validationResult = await _createAdvertValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - Validation failed",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                // Get current vendor's ID
                var vendorId = _currentUser.GetVendorId();
                if (string.IsNullOrEmpty(vendorId))
                {
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - User is not a vendor",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new BadRequestException("User is not a vendor"),
                        "User must be a vendor to create advertisements");
                }

                // Create Advertisement entity
                var advertisement = new Advertisement
                {
                    Id = Guid.NewGuid().ToString(),
                    VendorId = vendorId,
                    AdminId = "", // Will be set when admin approves
                    Title = request.Title,
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    TargetUrl = request.TargetUrl,
                    Status = AdvertStatusEnum.Pending,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Price = request.Price,
                    Language = request.Language,
                    Location = request.Location,
                    AdvertPlacement = request.AdvertPlacement,
                    PaymentStatus = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _repository.AdvertisementRepository.CreateAdvertisement(advertisement);
                await _repository.SaveChangesAsync();

                // Map response
                var response = _mapper.Map<AdvertisementResponseDto>(advertisement);

                await CreateAuditLog(
                    "Advertisement Created",
                    $"CorrelationId: {correlationId} - Advertisement created successfully with ID: {advertisement.Id}",
                    "Advertisement Management"
                );

                return ResponseFactory.Success(response, "Advertisement created successfully. Please proceed to payment.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating advertisement");
                await CreateAuditLog(
                    "Creation Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Advertisement Management"
                );
                return ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<AdvertisementResponseDto>> UpdateAdvertisement(UpdateAdvertisementRequestDto request)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Advertisement Update",
                    $"CorrelationId: {correlationId} - Updating advertisement with ID: {request.Id}",
                    "Advertisement Management"
                );

                // Validate request
                var validationResult = await _updateAdvertValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Validation failed",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                // Get advertisement
                var advertisement = await _repository.AdvertisementRepository.GetAdvertisementById(request.Id, trackChanges: true);
                if (advertisement == null)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Advertisement not found with ID: {request.Id}",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new NotFoundException($"Advertisement with ID {request.Id} not found"),
                        "Advertisement not found");
                }

                // Check if user is authorized
                var vendorId = _currentUser.GetVendorId();
                if (advertisement.VendorId != vendorId)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - User not authorized to update this advertisement",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new UnauthorizedException("You are not authorized to update this advertisement"),
                        "Unauthorized access");
                }

                // Check if advertisement can be updated
                if (advertisement.Status != AdvertStatusEnum.Pending && advertisement.Status != AdvertStatusEnum.Rejected)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Advertisement cannot be updated in current status: {advertisement.Status}",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new BadRequestException($"Advertisement cannot be updated in current status: {advertisement.Status}"),
                        "Advertisement cannot be updated in its current status");
                }

                // Update advertisement
                advertisement.Title = request.Title;
                advertisement.Description = request.Description;
                advertisement.ImageUrl = request.ImageUrl;
                advertisement.TargetUrl = request.TargetUrl;
                advertisement.StartDate = request.StartDate;
                advertisement.EndDate = request.EndDate;
                advertisement.Price = request.Price;
                advertisement.Language = request.Language;
                advertisement.Location = request.Location;
                advertisement.AdvertPlacement = request.AdvertPlacement;
                advertisement.UpdatedAt = DateTime.UtcNow;

                _repository.AdvertisementRepository.UpdateAdvertisement(advertisement);
                await _repository.SaveChangesAsync();

                // Map response
                var response = _mapper.Map<AdvertisementResponseDto>(advertisement);

                await CreateAuditLog(
                    "Advertisement Updated",
                    $"CorrelationId: {correlationId} - Advertisement updated successfully with ID: {advertisement.Id}",
                    "Advertisement Management"
                );

                return ResponseFactory.Success(response, "Advertisement updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating advertisement with ID {Id}: {Message}", request.Id, ex.Message);
                await CreateAuditLog(
                    "Update Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Advertisement Management"
                );
                return ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<AdvertisementDetailResponseDto>> GetAdvertisementById(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return ResponseFactory.Fail<AdvertisementDetailResponseDto>(
                        new BadRequestException("Advertisement ID is required"),
                        "Invalid ID provided");
                }

                var advertisement = await _repository.AdvertisementRepository.GetAdvertisementDetails(id);
                if (advertisement == null)
                {
                    return ResponseFactory.Fail<AdvertisementDetailResponseDto>(
                        new NotFoundException($"Advertisement with ID {id} not found"),
                        "Advertisement not found");
                }

                var advertisementDto = _mapper.Map<AdvertisementDetailResponseDto>(advertisement);

                await CreateAuditLog(
                    "Advertisement Details Query",
                    $"Retrieved advertisement details for ID: {id}",
                    "Advertisement Query"
                );

                return ResponseFactory.Success(advertisementDto, "Advertisement retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving advertisement with ID {Id}: {ErrorMessage}", id, ex.Message);
                return ResponseFactory.Fail<AdvertisementDetailResponseDto>(
                    ex, "An unexpected error occurred while retrieving the advertisement");
            }
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>> GetAdvertisements(
            AdvertisementFilterRequestDto filterDto, PaginationFilter paginationFilter)
        {
            try
            {
                // Get the vendor ID for filtering
                var vendorId = _currentUser.GetVendorId();

                // Base query
                var query = _repository.AdvertisementRepository.FindAll(trackChanges: false)
                    .Include(a => a.Vendor)
                    .Include(a => a.Views)
                    .AsQueryable();

                // Apply vendor filter if user is a vendor
                if (!string.IsNullOrEmpty(vendorId))
                {
                    query = query.Where(a => a.VendorId == vendorId);
                }

                // Apply search term filter
                if (!string.IsNullOrEmpty(filterDto.SearchTerm))
                {
                    var searchTerm = filterDto.SearchTerm.ToLower();
                    query = query.Where(a =>
                        a.Title.ToLower().Contains(searchTerm) ||
                        a.Description.ToLower().Contains(searchTerm));
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(filterDto.Status))
                {
                    if (Enum.TryParse<AdvertStatusEnum>(filterDto.Status, true, out var statusEnum))
                    {
                        query = query.Where(a => a.Status == statusEnum);
                    }
                }

                // Apply location filter
                if (!string.IsNullOrEmpty(filterDto.Location))
                {
                    query = query.Where(a => a.Location == filterDto.Location);
                }

                // Apply language filter
                if (!string.IsNullOrEmpty(filterDto.Language))
                {
                    query = query.Where(a => a.Language == filterDto.Language);
                }

                // Apply placement filter
                if (!string.IsNullOrEmpty(filterDto.AdvertPlacement))
                {
                    query = query.Where(a => a.AdvertPlacement == filterDto.AdvertPlacement);
                }

                // Apply date filters
                if (filterDto.StartDateFrom.HasValue)
                {
                    query = query.Where(a => a.StartDate >= filterDto.StartDateFrom.Value);
                }

                if (filterDto.StartDateTo.HasValue)
                {
                    query = query.Where(a => a.StartDate <= filterDto.StartDateTo.Value);
                }

                if (filterDto.EndDateFrom.HasValue)
                {
                    query = query.Where(a => a.EndDate >= filterDto.EndDateFrom.Value);
                }

                if (filterDto.EndDateTo.HasValue)
                {
                    query = query.Where(a => a.EndDate <= filterDto.EndDateTo.Value);
                }

                // Order by creation date, newest first
                query = query.OrderByDescending(a => a.CreatedAt);

                // Execute pagination
                var paginatedResult = await query.Paginate(paginationFilter);

                // Map to DTOs
                var advertisementDtos = paginatedResult.PageItems.Select(a =>
                {
                    var dto = _mapper.Map<AdvertisementResponseDto>(a);
                    dto.ViewCount = a.Views?.Count ?? 0;
                    return dto;
                });

                var result = new PaginatorDto<IEnumerable<AdvertisementResponseDto>>
                {
                    PageItems = advertisementDtos,
                    PageSize = paginatedResult.PageSize,
                    CurrentPage = paginatedResult.CurrentPage,
                    NumberOfPages = paginatedResult.NumberOfPages
                };

                await CreateAuditLog(
                    "Advertisement List Query",
                    $"Retrieved advertisement list - Page {paginationFilter.PageNumber}, " +
                    $"Size {paginationFilter.PageSize}, Filters: {JsonSerializer.Serialize(filterDto)}",
                    "Advertisement Query"
                );

                return ResponseFactory.Success(result, "Advertisements retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving advertisements: {ErrorMessage}", ex.Message);
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>(
                    ex, "An unexpected error occurred while retrieving advertisements");
            }
        }

        public async Task<BaseResponse<AdvertisementResponseDto>> UpdateAdvertisementStatus(string id, string status)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Advertisement Status Update",
                    $"CorrelationId: {correlationId} - Updating status of advertisement with ID: {id} to {status}",
                    "Advertisement Management"
                );

                // Get advertisement
                var advertisement = await _repository.AdvertisementRepository.GetAdvertisementById(id, trackChanges: true);
                if (advertisement == null)
                {
                    await CreateAuditLog(
                        "Status Update Failed",
                        $"CorrelationId: {correlationId} - Advertisement not found with ID: {id}",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new NotFoundException($"Advertisement with ID {id} not found"),
                        "Advertisement not found");
                }

                // Check if user is authorized (admin)
                var isAdmin = await _currentUser.IsInRoleAsync("Admin");
                if (!isAdmin)
                {
                    await CreateAuditLog(
                        "Status Update Failed",
                        $"CorrelationId: {correlationId} - User not authorized to update advertisement status",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new UnauthorizedException("You are not authorized to update advertisement status"),
                        "Unauthorized access");
                }

                // Update status
                if (Enum.TryParse<AdvertStatusEnum>(status, true, out var statusEnum))
                {
                    advertisement.Status = statusEnum;

                    // Set admin ID if approving
                    if (statusEnum == AdvertStatusEnum.Approved)
                    {
                        advertisement.AdminId = _currentUser.GetUserId();
                    }

                    advertisement.UpdatedAt = DateTime.UtcNow;

                    _repository.AdvertisementRepository.UpdateAdvertisement(advertisement);
                    await _repository.SaveChangesAsync();

                    // Map response
                    var response = _mapper.Map<AdvertisementResponseDto>(advertisement);

                    await CreateAuditLog(
                        "Advertisement Status Updated",
                        $"CorrelationId: {correlationId} - Advertisement status updated successfully with ID: {advertisement.Id}",
                        "Advertisement Management"
                    );

                    return ResponseFactory.Success(response, $"Advertisement status updated to {status} successfully");
                }
                else
                {
                    await CreateAuditLog(
                        "Status Update Failed",
                        $"CorrelationId: {correlationId} - Invalid status value: {status}",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new BadRequestException($"Invalid status value: {status}"),
                        "Invalid status value");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating advertisement status for ID {Id}: {Message}", id, ex.Message);
                await CreateAuditLog(
                    "Status Update Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Advertisement Management"
                );
                return ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<AdvertisementResponseDto>> DeleteAdvertisement(string id)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Advertisement Deletion",
                    $"CorrelationId: {correlationId} - Deleting advertisement with ID: {id}",
                    "Advertisement Management"
                );

                // Get advertisement
                var advertisement = await _repository.AdvertisementRepository.GetAdvertisementById(id, trackChanges: true);
                if (advertisement == null)
                {
                    await CreateAuditLog(
                        "Deletion Failed",
                        $"CorrelationId: {correlationId} - Advertisement not found with ID: {id}",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new NotFoundException($"Advertisement with ID {id} not found"),
                        "Advertisement not found");
                }

                // Check if user is authorized
                var vendorId = _currentUser.GetVendorId();
                var isAdmin = await _currentUser.IsInRoleAsync("Admin");

                if (advertisement.VendorId != vendorId && !isAdmin)
                {
                    await CreateAuditLog(
                        "Deletion Failed",
                        $"CorrelationId: {correlationId} - User not authorized to delete this advertisement",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new UnauthorizedException("You are not authorized to delete this advertisement"),
                        "Unauthorized access");
                }

                // Store for response
                var responseDto = _mapper.Map<AdvertisementResponseDto>(advertisement);

                // Delete advertisement
                _repository.AdvertisementRepository.DeleteAdvertisement(advertisement);
                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Advertisement Deleted",
                    $"CorrelationId: {correlationId} - Advertisement deleted successfully with ID: {id}",
                    "Advertisement Management"
                );

                return ResponseFactory.Success(responseDto, "Advertisement deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting advertisement with ID {Id}: {Message}", id, ex.Message);
                await CreateAuditLog(
                    "Deletion Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Advertisement Management"
                );
                return ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<AdvertisementResponseDto>> UploadPaymentProof(UploadPaymentProofRequestDto request)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Payment Proof Upload",
                    $"CorrelationId: {correlationId} - Uploading payment proof for advertisement with ID: {request.AdvertisementId}",
                    "Advertisement Payment"
                );

                // Validate request
                var validationResult = await _uploadPaymentProofValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    await CreateAuditLog(
                        "Upload Failed",
                        $"CorrelationId: {correlationId} - Validation failed",
                        "Advertisement Payment"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                // Get advertisement
                var advertisement = await _repository.AdvertisementRepository.GetAdvertisementById(request.AdvertisementId, trackChanges: true);
                if (advertisement == null)
                {
                    await CreateAuditLog(
                        "Upload Failed",
                        $"CorrelationId: {correlationId} - Advertisement not found with ID: {request.AdvertisementId}",
                        "Advertisement Payment"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new NotFoundException($"Advertisement with ID {request.AdvertisementId} not found"),
                        "Advertisement not found");
                }

                // Check if user is authorized
                var vendorId = "";
            }


    }   }
}
