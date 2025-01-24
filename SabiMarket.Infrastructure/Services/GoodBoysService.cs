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
    }
}
