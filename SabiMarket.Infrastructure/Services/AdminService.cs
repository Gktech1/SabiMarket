using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Application.IServices;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Infrastructure.Helpers;
using SabiMarket.Infrastructure.Utilities;
using ValidationException = FluentValidation.ValidationException;
using System.Text.Json;
using SabiMarket.Application.Validators;
using AutoMapper.QueryableExtensions;
using FluentValidation.Results;
using Azure.Core;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Enum;
using SabiMarket.Infrastructure.Repositories;

public class AdminService : IAdminService
{
    private readonly IRepositoryManager _repository;
    private readonly ILogger<AdminService> _logger;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IValidator<CreateAdminRequestDto> _createAdminValidator;
    private readonly IValidator<UpdateAdminProfileDto> _updateProfileValidator;
    private readonly IValidator<CreateRoleRequestDto> _createRoleValidator;
    private readonly IValidator<UpdateRoleRequestDto> _updateRoleValidator;

    public AdminService(
        IRepositoryManager repository,
        ILogger<AdminService> logger,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUser,
        IHttpContextAccessor httpContextAccessor,
        IValidator<CreateAdminRequestDto> createAdminValidator,
        IValidator<UpdateAdminProfileDto> updateProfileValidator,
        IValidator<CreateRoleRequestDto> createRoleValidator,
        IValidator<UpdateRoleRequestDto> updateRoleValidator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _createAdminValidator = createAdminValidator ?? throw new ArgumentNullException(nameof(createAdminValidator));
        _updateProfileValidator = updateProfileValidator ?? throw new ArgumentNullException(nameof(updateProfileValidator));
        _createRoleValidator = createRoleValidator ?? throw new ArgumentNullException(nameof(createRoleValidator));
        _updateRoleValidator = updateRoleValidator ?? throw new ArgumentNullException(nameof(updateRoleValidator));
    }

    private string GetCurrentIpAddress()
    {
        return _httpContextAccessor.GetRemoteIPAddress();
    }

    private async Task CreateAuditLog(string activity, string details, string module = "Admin Management")
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

    public async Task<BaseResponse<AdminResponseDto>> GetAdminById(string adminId)
    {
        try
        {
            var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: false);
            if (admin == null)
            {
                await CreateAuditLog(
                    "Admin Lookup Failed",
                    $"Failed to find admin with ID: {adminId}",
                    "Admin Query"
                );
                return ResponseFactory.Fail<AdminResponseDto>(
                    new NotFoundException("Admin not found"),
                    "Admin not found");
            }

            var adminDto = _mapper.Map<AdminResponseDto>(admin);

            await CreateAuditLog(
                "Admin Lookup",
                $"Retrieved admin details for ID: {adminId}",
                "Admin Query"
            );

            return ResponseFactory.Success(adminDto, "Admin retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin");
            return ResponseFactory.Fail<AdminResponseDto>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<AdminResponseDto>> CreateAdmin(CreateAdminRequestDto adminDto)
    {
        try
        {
            var validationResult = await _createAdminValidator.ValidateAsync(adminDto);
            if (!validationResult.IsValid)
            {
                await CreateAuditLog(
                    "Admin Creation Failed",
                    $"Validation failed for new admin creation with email: {adminDto.Email}",
                    "Admin Creation"
                );
                return ResponseFactory.Fail<AdminResponseDto>(
                    new FluentValidation.ValidationException(validationResult.Errors),
                    "Validation failed");
            }

            var existingUser = await _userManager.FindByEmailAsync(adminDto.Email);
            if (existingUser != null)
            {
                await CreateAuditLog(
                    "Admin Creation Failed",
                    $"Email already exists: {adminDto.Email}",
                    "Admin Creation"
                );
                return ResponseFactory.Fail<AdminResponseDto>("Email already exists");
            }

            var user = new ApplicationUser
            {
                UserName = adminDto.Email,
                Email = adminDto.Email,
                FirstName = adminDto.FirstName,
                LastName = adminDto.LastName,
                PhoneNumber = adminDto.PhoneNumber,
                ProfileImageUrl = adminDto.ProfileImageUrl,
                Gender = adminDto.Gender,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createUserResult = await _userManager.CreateAsync(user);
            if (!createUserResult.Succeeded)
            {
                await CreateAuditLog(
                    "Admin Creation Failed",
                    $"Failed to create user account for: {adminDto.Email}",
                    "Admin Creation"
                );
                return ResponseFactory.Fail<AdminResponseDto>(
                    new ValidationException((IEnumerable<FluentValidation.Results.ValidationFailure>)createUserResult
                    .Errors.Select(e => e.Description)),
                    "Failed to create user");
            }

            var admin = new Admin
            {
                UserId = user.Id,
                Position = adminDto.Position,
                Department = adminDto.Department,
                AdminLevel = adminDto.AdminLevel,
                HasDashboardAccess = adminDto.HasDashboardAccess,
                HasRoleManagementAccess = adminDto.HasRoleManagementAccess,
                HasTeamManagementAccess = adminDto.HasTeamManagementAccess,
                HasAuditLogAccess = adminDto.HasAuditLogAccess,
                StatsLastUpdatedAt = DateTime.UtcNow
            };

            _repository.AdminRepository.CreateAdmin(admin);
            await _repository.SaveChangesAsync();

            await _userManager.AddToRoleAsync(user, UserRoles.Admin);

            await CreateAuditLog(
                "Created Admin Account",
                $"Created admin account for {user.Email} ({user.FirstName} {user.LastName}) " +
                $"with role {admin.AdminLevel} in {admin.Department} department"
            );

            var createdAdmin = _mapper.Map<AdminResponseDto>(admin);
            return ResponseFactory.Success(createdAdmin, "Admin created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating admin");
            return ResponseFactory.Fail<AdminResponseDto>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<bool>> UpdateAdminProfile(string adminId, UpdateAdminProfileDto profileDto)
    {
        try
        {
            var validationResult = await _updateProfileValidator.ValidateAsync(profileDto);
            if (!validationResult.IsValid)
            {
                await CreateAuditLog(
                    "Profile Update Failed",
                    $"Validation failed for admin profile update ID: {adminId}",
                    "Profile Management"
                );
                return ResponseFactory.Fail<bool>(
                    new ValidationException(validationResult.Errors),
                    "Validation failed");
            }

            var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: true);
            if (admin == null)
            {
                await CreateAuditLog(
                    "Profile Update Failed",
                    $"Admin not found for ID: {adminId}",
                    "Profile Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("Admin not found"),
                    "Admin not found");
            }

            var user = await _userManager.FindByIdAsync(admin.UserId);
            if (user == null)
            {
                await CreateAuditLog(
                    "Profile Update Failed",
                    $"User not found for admin ID: {adminId}",
                    "Profile Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("User not found"),
                    "User not found");
            }

            // Track changes for audit log
            var changes = new List<string>();
            if (user.FirstName != profileDto.FirstName)
                changes.Add($"First Name: {user.FirstName} → {profileDto.FirstName}");
            if (user.LastName != profileDto.LastName)
                changes.Add($"Last Name: {user.LastName} → {profileDto.LastName}");
            if (user.PhoneNumber != profileDto.PhoneNumber)
                changes.Add($"Phone: {user.PhoneNumber} → {profileDto.PhoneNumber}");
            if (admin.Position != profileDto.Position)
                changes.Add($"Position: {admin.Position} → {profileDto.Position}");
            if (admin.Department != profileDto.Department)
                changes.Add($"Department: {admin.Department} → {profileDto.Department}");

            // Update user properties
            user.FirstName = profileDto.FirstName;
            user.LastName = profileDto.LastName;
            user.PhoneNumber = profileDto.PhoneNumber;
            user.ProfileImageUrl = profileDto.ProfileImageUrl;
            user.Gender = profileDto.Gender;

            var updateUserResult = await _userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                await CreateAuditLog(
                    "Profile Update Failed",
                    $"Failed to update user properties for admin ID: {adminId}",
                    "Profile Management"
                );
                return ResponseFactory.Fail<bool>(
                    new ValidationException(
                        (IEnumerable<FluentValidation.Results.ValidationFailure>)updateUserResult.Errors.Select(e => e.Description)),
                    "Failed to update user");
            }

            // Update admin properties
            admin.Position = profileDto.Position;
            admin.Department = profileDto.Department;

            _repository.AdminRepository.UpdateAdmin(admin);

            await CreateAuditLog(
                "Updated Admin Profile",
                $"Updated profile for {user.Email}. Changes: {string.Join(", ", changes)}",
                "Profile Management"
            );

            await _repository.SaveChangesAsync();

            return ResponseFactory.Success(true, "Admin profile updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin profile");
            return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<PaginatorDto<IEnumerable<AdminResponseDto>>>> GetAdmins(
        AdminFilterRequestDto filterDto, PaginationFilter paginationFilter)
    {
        try
        {
            var query = _repository.AdminRepository.GetFilteredAdminsQuery(filterDto);
            var paginatedAdmins = await query.Paginate(paginationFilter);

            var adminDtos = _mapper.Map<IEnumerable<AdminResponseDto>>(paginatedAdmins.PageItems);
            var result = new PaginatorDto<IEnumerable<AdminResponseDto>>
            {
                PageItems = adminDtos,
                PageSize = paginatedAdmins.PageSize,
                CurrentPage = paginatedAdmins.CurrentPage,
                NumberOfPages = paginatedAdmins.NumberOfPages
            };

            await CreateAuditLog(
                "Admin List Query",
                $"Retrieved admin list - Page {paginationFilter.PageNumber}, " +
                $"Size {paginationFilter.PageSize}, Filters: {JsonSerializer.Serialize(filterDto)}",
                "Admin Query"
            );

            return ResponseFactory.Success(result, "Admins retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admins");
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<AdminResponseDto>>>(
                ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<DashboardReportDto>> GetDashboardReportDataAsync(
          string lgaFilter = null,
          string marketFilter = null,
          int? year = null,
          TimeFrame timeFrame = TimeFrame.ThisWeek)
    {
        var correlationId = Guid.NewGuid().ToString();
        try
        {
            await CreateAuditLog(
                "Dashboard Data Request",
                $"CorrelationId: {correlationId} - Fetching dashboard data with filters: LGA={lgaFilter}, Market={marketFilter}, Year={year}, TimeFrame={timeFrame}",
            "Dashboard Management"
            );

            var dashboardData = await _repository.ReportRepository.GetDashboardReportDataAsync(
                lgaFilter,
                marketFilter,
                year,
                timeFrame);

            await CreateAuditLog(
                "Dashboard Data Retrieved",
                $"CorrelationId: {correlationId} - Dashboard data retrieved successfully",
                "Dashboard Management"
            );

            return ResponseFactory.Success(dashboardData, "Dashboard data retrieved successfully");
        }
        catch (Exception ex)
        {
            await CreateAuditLog(
                "Dashboard Data Request Failed",
                $"CorrelationId: {correlationId} - Error: {ex.Message}",
                "Dashboard Management"
            );

            return ResponseFactory.Fail<DashboardReportDto>(ex, "An unexpected error occurred while retrieving dashboard data");
        }
    }

    public async Task<BaseResponse<byte[]>> ExportReport(ReportExportRequestDto request)
    {
        var correlationId = Guid.NewGuid().ToString();
        try
        {
            // Log the export attempt with more detailed information
            await CreateAuditLog(
                "Report Export Requested",
                $"CorrelationId: {correlationId} - Exporting {request.ReportType} report in {request.ExportFormat} format. " +
                $"Date range: {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}. " +
                $"Market: {(string.IsNullOrEmpty(request.MarketId) ? "All Markets" : request.MarketId)}. " +
                $"LGA: {(string.IsNullOrEmpty(request.LGAId) ? "All LGAs" : request.LGAId)}",
                "Report Management"
            );

            // Validate date range
            if (request.EndDate < request.StartDate)
            {
                return ResponseFactory.Fail<byte[]>(
                    new ArgumentException("End date cannot be earlier than start date"),
                    "Invalid date range provided"
                );
            }

            // Retrieve report data with all filter parameters
            var report = await _repository.ReportRepository.ExportAdminReport(
                request.StartDate,
                request.EndDate,
                request.MarketId,
                request.LGAId,
                request.TimeZone
            );

            // Map repository data to DTO
            var reportData = _mapper.Map<ReportExportDto>(report);

            // Generate appropriate export format
            byte[] resultBytes;
            string formatName;

            switch (request.ExportFormat)
            {
                case ExportFormat.Excel:
                    resultBytes = await ExcelExportHelper.GenerateMarketReport(reportData);
                    formatName = "Excel";
                    break;

                case ExportFormat.PDF:
                    resultBytes = await PdfExportHelper.GenerateMarketReport(reportData);
                    formatName = "PDF";
                    break;

                case ExportFormat.CSV:
                    resultBytes = await CsvExportHelper.GenerateMarketReport(reportData);
                    formatName = "CSV";
                    break;

                default:
                    resultBytes = await ExcelExportHelper.GenerateMarketReport(reportData);
                    formatName = "Excel";
                    break;
            }

            // Log successful export
            await CreateAuditLog(
                "Report Exported Successfully",
                $"CorrelationId: {correlationId} - Report exported in {formatName} format. " +
                $"Size: {resultBytes.Length} bytes",
                "Report Management"
            );

            return ResponseFactory.Success(resultBytes, $"Report exported successfully in {formatName} format");
        }
        catch (Exception ex)
        {
            // Log detailed error information
            await CreateAuditLog(
                "Report Export Failed",
                $"CorrelationId: {correlationId} - Error: {ex.Message}\n" +
                $"Stack Trace: {ex.StackTrace}\n" +
                $"Report Parameters: Start={request.StartDate:yyyy-MM-dd}, End={request.EndDate:yyyy-MM-dd}, " +
                $"Market={request.MarketId}, Format={request.ExportFormat}",
                "Report Management"
            );

            return ResponseFactory.Fail<byte[]>(ex, "An unexpected error occurred while generating the report");
        }
    }

    public async Task<BaseResponse<AdminDashboardStatsDto>> GetDashboardStats(string adminId)
    {
        try
        {
            var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: true);
            if (admin == null)
            {
                await CreateAuditLog(
                    "Dashboard Access Failed",
                    $"Admin not found for ID: {adminId}",
                    "Dashboard Access"
                );
                return ResponseFactory.Fail<AdminDashboardStatsDto>(
                    new NotFoundException("Admin not found"),
                    "Admin not found");
            }

            if (!admin.HasDashboardAccess)
            {
                await CreateAuditLog(
                    "Dashboard Access Denied",
                    $"Access denied for admin ID: {adminId} - No dashboard access rights",
                    "Dashboard Access"
                );
                return ResponseFactory.Fail<AdminDashboardStatsDto>(
                    new UnauthorizedException("Access denied"),
                    "You don't have access to dashboard statistics");
            }

            // Update last dashboard access
            admin.LastDashboardAccess = DateTime.UtcNow;
            _repository.AdminRepository.UpdateAdmin(admin);

            // Get dashboard statistics
            var stats = await _repository.AdminRepository.GetAdminDashboardStatsAsync(adminId);
            var statsDto = _mapper.Map<AdminDashboardStatsDto>(stats);

            await CreateAuditLog(
                "Dashboard Access",
                $"Retrieved dashboard stats for admin ID: {adminId}",
                "Dashboard Access"
            );

            await _repository.SaveChangesAsync();

            return ResponseFactory.Success(statsDto, "Dashboard stats retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard stats");
            return ResponseFactory.Fail<AdminDashboardStatsDto>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<bool>> UpdateDashboardAccess(string adminId, UpdateAdminAccessDto accessDto)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: true);
            if (admin == null)
            {
                await CreateAuditLog(
                    "Access Update Failed",
                    $"Admin not found for ID: {adminId}",
                    "Access Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("Admin not found"),
                    "Admin not found");
            }

            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);
            if (currentAdmin == null || !currentAdmin.HasRoleManagementAccess)
            {
                await CreateAuditLog(
                    "Access Update Denied",
                    $"Unauthorized access update attempt for admin ID: {adminId} by user: {userId}",
                    "Access Management"
                );
                return ResponseFactory.Fail<bool>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to update admin access");
            }

            // Track changes for audit log
            var changes = new List<string>();
            if (admin.HasDashboardAccess != accessDto.HasDashboardAccess)
                changes.Add($"Dashboard Access: {admin.HasDashboardAccess} → {accessDto.HasDashboardAccess}");
            if (admin.HasRoleManagementAccess != accessDto.HasRoleManagementAccess)
                changes.Add($"Role Management Access: {admin.HasRoleManagementAccess} → {accessDto.HasRoleManagementAccess}");
            if (admin.HasTeamManagementAccess != accessDto.HasTeamManagementAccess)
                changes.Add($"Team Management Access: {admin.HasTeamManagementAccess} → {accessDto.HasTeamManagementAccess}");
            if (admin.HasAuditLogAccess != accessDto.HasAuditLogAccess)
                changes.Add($"Audit Log Access: {admin.HasAuditLogAccess} → {accessDto.HasAuditLogAccess}");

            // Update access permissions
            admin.HasDashboardAccess = accessDto.HasDashboardAccess;
            admin.HasRoleManagementAccess = accessDto.HasRoleManagementAccess;
            admin.HasTeamManagementAccess = accessDto.HasTeamManagementAccess;
            admin.HasAuditLogAccess = accessDto.HasAuditLogAccess;

            _repository.AdminRepository.UpdateAdmin(admin);

            var user = await _userManager.FindByIdAsync(admin.UserId);
            await CreateAuditLog(
                "Updated Access Permissions",
                $"Updated access permissions for {user?.Email ?? "Unknown"}. Changes: {string.Join(", ", changes)}",
                "Access Management"
            );

            await _repository.SaveChangesAsync();
            return ResponseFactory.Success(true, "Admin access updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin access");
            return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<bool>> DeactivateAdmin(string adminId)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: true);
            if (admin == null)
            {
                await CreateAuditLog(
                    "Admin Deactivation Failed",
                    $"Admin not found for ID: {adminId}",
                    "Admin Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("Admin not found"),
                    "Admin not found");
            }

            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);
            if (currentAdmin == null || !currentAdmin.HasRoleManagementAccess)
            {
                await CreateAuditLog(
                    "Admin Deactivation Denied",
                    $"Unauthorized deactivation attempt for admin ID: {adminId} by user: {userId}",
                    "Admin Management"
                );
                return ResponseFactory.Fail<bool>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to deactivate admins");
            }

            var user = await _userManager.FindByIdAsync(admin.UserId);
            if (user == null)
            {
                await CreateAuditLog(
                    "Admin Deactivation Failed",
                    $"User not found for admin ID: {adminId}",
                    "Admin Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("User not found"),
                    "Associated user not found");
            }

            user.IsActive = false;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                await CreateAuditLog(
                    "Admin Deactivation Failed",
                    $"Failed to update user status for admin ID: {adminId}",
                    "Admin Management"
                );
                return ResponseFactory.Fail<bool>(
                    new ValidationException((IEnumerable<FluentValidation.Results.ValidationFailure>)updateResult.
                    Errors.Select(e => e.Description)),
                    "Failed to deactivate admin");
            }

            await CreateAuditLog(
                "Deactivated Admin Account",
                $"Deactivated admin account for {user.Email} ({user.FirstName} {user.LastName})",
                "Admin Management"
            );

            await _repository.SaveChangesAsync();
            return ResponseFactory.Success(true, "Admin deactivated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating admin");
            return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<bool>> ReactivateAdmin(string adminId)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: true);
            if (admin == null)
            {
                await CreateAuditLog(
                    "Admin Reactivation Failed",
                    $"Admin not found for ID: {adminId}",
                    "Admin Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("Admin not found"),
                    "Admin not found");
            }

            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);
            if (currentAdmin == null || !currentAdmin.HasRoleManagementAccess)
            {
                await CreateAuditLog(
                    "Admin Reactivation Denied",
                    $"Unauthorized reactivation attempt for admin ID: {adminId} by user: {userId}",
                    "Admin Management"
                );
                return ResponseFactory.Fail<bool>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to reactivate admins");
            }

            var user = await _userManager.FindByIdAsync(admin.UserId);
            if (user == null)
            {
                await CreateAuditLog(
                    "Admin Reactivation Failed",
                    $"User not found for admin ID: {adminId}",
                    "Admin Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("User not found"),
                    "Associated user not found");
            }

            user.IsActive = true;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                await CreateAuditLog(
                    "Admin Reactivation Failed",
                    $"Failed to update user status for admin ID: {adminId}",
                    "Admin Management"
                );
                return ResponseFactory.Fail<bool>(
                    new ValidationException((IEnumerable<FluentValidation.Results.ValidationFailure>)
                    updateResult.Errors.Select(e => e.Description)),
                    "Failed to reactivate admin");
            }

            await CreateAuditLog(
                "Reactivated Admin Account",
                $"Reactivated admin account for {user.Email} ({user.FirstName} {user.LastName})",
                "Admin Management"
            );

            await _repository.SaveChangesAsync();
            return ResponseFactory.Success(true, "Admin reactivated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating admin");
            return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<PaginatorDto<IEnumerable<AuditLogResponseDto>>>> GetAdminAuditLogs(
    string adminId, DateTime? startDate, DateTime? endDate, PaginationFilter paginationFilter)
    {
        try
        {
            var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: false);
            if (admin == null)
            {
                await CreateAuditLog(
                    "Audit Log Access Failed",
                    $"Admin not found for ID: {adminId}",
                    "Audit Log Access"
                );
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<AuditLogResponseDto>>>(
                    new NotFoundException("Admin not found"),
                    "Admin not found");
            }

            if (!admin.HasAuditLogAccess)
            {
                await CreateAuditLog(
                    "Audit Log Access Denied",
                    $"Unauthorized audit log access attempt for admin ID: {adminId}",
                    "Audit Log Access"
                );
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<AuditLogResponseDto>>>(
                    new UnauthorizedException("Access denied"),
                    "You don't have access to audit logs");
            }

            var query = _repository.AdminRepository.GetAdminAuditLogsQuery(adminId, startDate, endDate);
            var paginatedLogs = await query.Paginate(paginationFilter);

            // Map to DTOs
            var logDtos = _mapper.Map<IEnumerable<AuditLogResponseDto>>(paginatedLogs.PageItems).ToList();

            // Add roles for each audit log
            foreach (var logDto in logDtos)
            {
                var user = await _userManager.FindByIdAsync(logDto.UserId);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    logDto.UserRole = roles.FirstOrDefault() ?? "Unknown";
                }
            }

            var result = new PaginatorDto<IEnumerable<AuditLogResponseDto>>
            {
                PageItems = logDtos,
                PageSize = paginatedLogs.PageSize,
                CurrentPage = paginatedLogs.CurrentPage,
                NumberOfPages = paginatedLogs.NumberOfPages
            };

            await CreateAuditLog(
                "Audit Log Access",
                $"Retrieved audit logs for admin ID: {adminId}, Date Range: {startDate?.ToString("yyyy-MM-dd") ?? "Start"} to {endDate?.ToString("yyyy-MM-dd") ?? "End"}",
                "Audit Log Access"
            );

            return ResponseFactory.Success(result, "Audit logs retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin audit logs");
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<AuditLogResponseDto>>>(
                ex, "An unexpected error occurred");
        }
    }

    // Role Management Methods matching UI/UX
    /* public async Task<BaseResponse<RoleResponseDto>> GetRoleById(string roleId)
     {
         try
         {
             var role = await _repository.AdminRepository.GetRoleByIdAsync(roleId, trackChanges: false);
             if (role == null)
             {
                 await CreateAuditLog(
                     "Role Lookup Failed",
                     $"Failed to find role with ID: {roleId}",
                     "Role Management"
                 );
                 return ResponseFactory.Fail<RoleResponseDto>(
                     new NotFoundException("Role not found"),
                     "Role not found");
             }

             var roleDto = _mapper.Map<RoleResponseDto>(role);

             await CreateAuditLog(
                 "Role Lookup",
                 $"Retrieved role details for ID: {roleId}",
                 "Role Management"
             );

             return ResponseFactory.Success(roleDto, "Role retrieved successfully");
         }
         catch (Exception ex)
         {
             _logger.LogError(ex, "Error retrieving role");
             return ResponseFactory.Fail<RoleResponseDto>(ex, "An unexpected error occurred");
         }
     }*/

    public async Task<BaseResponse<RoleResponseDto>> GetRoleById(string roleId)
    {
        try
        {
            var role = await _repository.AdminRepository.GetRoleByIdAsync(roleId, trackChanges: false);
            if (role == null)
            {
                await CreateAuditLog(
                    "Role Lookup Failed",
                    $"Failed to find role with ID: {roleId}",
                    "Role Management"
                );
                return ResponseFactory.Fail<RoleResponseDto>(
                    new NotFoundException("Role not found"),
                    "Role not found");
            }

            // Use the same private mapping method instead of AutoMapper
            var roleDto = MapToRoleResponseDto(role);

            await CreateAuditLog(
                "Role Lookup",
                $"Retrieved role details for ID: {roleId}",
                "Role Management"
            );
            return ResponseFactory.Success(roleDto, "Role retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role with ID: {RoleId}", roleId);
            return ResponseFactory.Fail<RoleResponseDto>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<PaginatorDto<IEnumerable<RoleResponseDto>>>> GetRoles(
    RoleFilterRequestDto filterDto,
    PaginationFilter paginationFilter)
    {
        try
        {
            if (paginationFilter == null)
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                    new ArgumentNullException(nameof(paginationFilter)),
                    "Pagination parameters are required");
            }

            // Create empty filter if null to get all records
            filterDto ??= new RoleFilterRequestDto();

            const int MinPageSize = 1;
            const int DefaultPageSize = 10;
            const int MaxPageSize = 100;
            paginationFilter.PageSize = paginationFilter.PageSize switch
            {
                < MinPageSize => DefaultPageSize,
                > MaxPageSize => MaxPageSize,
                _ => paginationFilter.PageSize
            };

            var query = _repository.AdminRepository.GetFilteredRolesQuery(filterDto);
            if (query == null)
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                    new InvalidOperationException("Query could not be created"),
                    "Failed to create roles query");
            }

            var paginatedRoles = await query.Paginate(paginationFilter);
            var roleDtos = paginatedRoles.PageItems.Select(MapToRoleResponseDto).ToList();

            var result = new PaginatorDto<IEnumerable<RoleResponseDto>>
            {
                PageItems = roleDtos,
                PageSize = paginatedRoles.PageSize,
                CurrentPage = paginatedRoles.CurrentPage,
                NumberOfPages = paginatedRoles.NumberOfPages
            };

            // Create audit log with appropriate message based on whether search was used
            var searchDescription = string.IsNullOrWhiteSpace(filterDto.SearchTerm)
                ? "all roles"
                : $"roles with search: {filterDto.SearchTerm}";

            await CreateAuditLog(
                "Role List Query",
                $"Retrieved {searchDescription} - Page {paginationFilter.PageNumber}, Size {paginationFilter.PageSize}",
                "Role Management"
            );

            return ResponseFactory.Success(result, "Roles retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving roles. Filter: {@FilterDto}, Pagination: {@PaginationFilter}",
                filterDto,
                paginationFilter);
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                ex, "An unexpected error occurred");
        }
    }
    /* public async Task<BaseResponse<PaginatorDto<IEnumerable<RoleResponseDto>>>> GetRoles(
         RoleFilterRequestDto filterDto,
         PaginationFilter paginationFilter)
     {
         try
         {
             if (paginationFilter == null)
             {
                 return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                     new ArgumentNullException(nameof(paginationFilter)),
                     "Pagination parameters are required");
             }

             const int MinPageSize = 1;
             const int DefaultPageSize = 10;
             const int MaxPageSize = 100;

             paginationFilter.PageSize = paginationFilter.PageSize switch
             {
                 < MinPageSize => DefaultPageSize,
                 > MaxPageSize => MaxPageSize,
                 _ => paginationFilter.PageSize
             };

             var query = _repository.AdminRepository.GetFilteredRolesQuery(filterDto);
             if (query == null)
             {
                 return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                     new InvalidOperationException("Query could not be created"),
                     "Failed to create roles query");
             }

             var paginatedRoles = await query.Paginate(paginationFilter);

             // Use private mapping method
             var roleDtos = paginatedRoles.PageItems.Select(MapToRoleResponseDto).ToList();

             var result = new PaginatorDto<IEnumerable<RoleResponseDto>>
             {
                 PageItems = roleDtos,
                 PageSize = paginatedRoles.PageSize,
                 CurrentPage = paginatedRoles.CurrentPage,
                 NumberOfPages = paginatedRoles.NumberOfPages
             };

             var response = ResponseFactory.Success(result, "Roles retrieved successfully");

             await CreateAuditLog(
                 "Role List Query",
                 $"Retrieved role list - Page {paginationFilter.PageNumber}, " +
                 $"Size {paginationFilter.PageSize}, Search: {filterDto.SearchTerm ?? "none"}",
                 "Role Management"
             );

             return response;
         }
         catch (Exception ex)
         {
             _logger.LogError(ex,
                 "Error retrieving roles. Filter: {@FilterDto}, Pagination: {@PaginationFilter}",
                 filterDto,
                 paginationFilter);
             return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                 ex, "An unexpected error occurred");
         }
     }
 */
    private static RoleResponseDto MapToRoleResponseDto(ApplicationRole role)
    {
        if (role == null) return null;

        return new RoleResponseDto
        {
            Id = role.Id,
            Name = role.Name,
            AllPermissions = role.Permissions?
                .Select(p => p.Name)
                .ToList() ?? new List<string>(),
            IsActive = role.IsActive,
            CreatedAt = role.CreatedAt,
            CreatedBy = role.CreatedBy,
            LastModifiedAt = role.LastModifiedAt,
            LastModifiedBy = role.LastModifiedBy
        };
    }
    //Working 
    /*  public async Task<BaseResponse<PaginatorDto<IEnumerable<RoleResponseDto>>>> GetRoles(
      RoleFilterRequestDto filterDto,
      PaginationFilter paginationFilter)
      {
          try
          {
              if (paginationFilter == null)
              {
                  return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                      new ArgumentNullException(nameof(paginationFilter)),
                      "Pagination parameters are required");
              }

              const int MinPageSize = 1;
              const int DefaultPageSize = 10;
              const int MaxPageSize = 100;

              paginationFilter.PageSize = paginationFilter.PageSize switch
              {
                  < MinPageSize => DefaultPageSize,
                  > MaxPageSize => MaxPageSize,
                  _ => paginationFilter.PageSize
              };

              var query = _repository.AdminRepository.GetFilteredRolesQuery(filterDto);
              if (query == null)
              {
                  return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                      new InvalidOperationException("Query could not be created"),
                      "Failed to create roles query");
              }

              *//*var paginatedRoles = await query
                          .ProjectTo<RoleResponseDto>(_mapper.ConfigurationProvider)
                          .Paginate(paginationFilter);*//*
              var paginatedRoles = await query.Paginate(paginationFilter);
              var roleDtos = _mapper.Map<IEnumerable<RoleResponseDto>>(paginatedRoles.PageItems);

              var result = new PaginatorDto<IEnumerable<RoleResponseDto>>
              {
                  PageItems = roleDtos,
                  PageSize = paginatedRoles.PageSize,
                  CurrentPage = paginatedRoles.CurrentPage,
                  NumberOfPages = paginatedRoles.NumberOfPages
              };

              var response = ResponseFactory.Success(result, "Roles retrieved successfully");

              await CreateAuditLog(
                  "Role List Query",
                  $"Retrieved role list - Page {paginationFilter.PageNumber}, " +
                  $"Size {paginationFilter.PageSize}, Search: {filterDto.SearchTerm ?? "none"}",
                  "Role Management"
              );

              return response;
          }
          catch (Exception ex)
          {
              _logger.LogError(ex,
                  "Error retrieving roles. Filter: {@FilterDto}, Pagination: {@PaginationFilter}",
                  filterDto,
                  paginationFilter);
              return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                  ex, "An unexpected error occurred");
          }
      }*/
    /*    public async Task<BaseResponse<PaginatorDto<IEnumerable<RoleResponseDto>>>> GetRoles(
            RoleFilterRequestDto filterDto, PaginationFilter paginationFilter)
        {
            try
            {
                // Default to 10 rows per page as shown in UI
                paginationFilter.PageSize = paginationFilter.PageSize <= 0 ? 10 : paginationFilter.PageSize;

                var query = _repository.AdminRepository.GetFilteredRolesQuery(filterDto);
                var paginatedRoles = await query.Paginate(paginationFilter);

                var roleDtos = _mapper.Map<IEnumerable<RoleResponseDto>>(paginatedRoles.PageItems);

                // Format response to match UI display
                var result = new PaginatorDto<IEnumerable<RoleResponseDto>>
                {
                    PageItems = roleDtos,
                    PageSize = paginatedRoles.PageSize,
                    CurrentPage = paginatedRoles.CurrentPage,
                    NumberOfPages = paginatedRoles.NumberOfPages
                };

                await CreateAuditLog(
                    "Role List Query",
                    $"Retrieved role list - Page {paginationFilter.PageNumber}, " +
                    $"Size {paginationFilter.PageSize}, Search: {filterDto.SearchTerm ?? "none"}",
                    "Role Management"
                );

                return ResponseFactory.Success(result, "Roles retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                    ex, "An unexpected error occurred");
            }
        }*/

    public async Task<BaseResponse<RoleResponseDto>> CreateRole(CreateRoleRequestDto createRoleDto)
    {
        try
        {
            var validationResult = await _createRoleValidator.ValidateAsync(createRoleDto);
            if (!validationResult.IsValid)
            {
                await CreateAuditLog(
                    "Role Creation Failed",
                    $"Validation failed for new role creation: {createRoleDto.Name}",
                    "Role Management"
                );
                return ResponseFactory.Fail<RoleResponseDto>(
                    new ValidationException(validationResult.Errors),
                    "Validation failed");
            }

            // Check if role exists
            if (await _repository.AdminRepository.RoleExistsAsync(createRoleDto.Name))
            {
                await CreateAuditLog(
                    "Role Creation Failed",
                    $"Role name already exists: {createRoleDto.Name}",
                    "Role Management"
                );
                return ResponseFactory.Fail<RoleResponseDto>("Role name already exists");
            }

            // Create role with UI-specified permissions
            var role = new ApplicationRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = createRoleDto.Name,
                NormalizedName = createRoleDto.Name.ToUpper(),
                CreatedBy = _currentUser.GetUserId(),
                LastModifiedBy = _currentUser.GetUserId(),
                Description = createRoleDto.Description,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow,
                IsActive = true,
                Permissions = createRoleDto.Permissions.Select(p => new RolePermission
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = p,
                    IsGranted = true
                }).ToList()
            };

            await _repository.AdminRepository.CreateRoleAsync(role);
            await _repository.SaveChangesAsync();

            await CreateAuditLog(
                "Created Role",
                $"Created role {role.Name} with permissions: {string.Join(", ", createRoleDto.Permissions)}",
                "Role Management"
            );

            var responseDto = _mapper.Map<RoleResponseDto>(role);
            return ResponseFactory.Success(responseDto, "Role created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return ResponseFactory.Fail<RoleResponseDto>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<RoleResponseDto>> UpdateRole(string roleId, UpdateRoleRequestDto updateRoleDto)
    {
        try
        {
            // Check current user's permission
            var userId = _currentUser.GetUserId();
            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);

            if (currentAdmin == null || !currentAdmin.HasRoleManagementAccess)
            {
                await CreateAuditLog(
                    "Role Update Denied",
                    $"Unauthorized role update attempt by user: {userId}",
                    "Role Management"
                );
                return ResponseFactory.Fail<RoleResponseDto>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to update roles");
            }

            // Validate request
            var validationResult = await _updateRoleValidator.ValidateAsync(updateRoleDto);
            if (!validationResult.IsValid)
            {
                await CreateAuditLog(
                    "Role Update Failed",
                    $"Validation failed for role update ID: {roleId}",
                    "Role Management"
                );
                return ResponseFactory.Fail<RoleResponseDto>(
                    new ValidationException(validationResult.Errors),
                    "Validation failed");
            }

            var role = await _repository.AdminRepository.GetRoleByIdAsync(roleId, trackChanges: true);
            if (role == null)
            {
                await CreateAuditLog(
                    "Role Update Failed",
                    $"Role not found for ID: {roleId}",
                    "Role Management"
                );
                return ResponseFactory.Fail<RoleResponseDto>(
                    new NotFoundException("Role not found"),
                    "Role not found");
            }

            // Track all changes for audit log
            var changes = new List<string>();

            // Debug logging
            _logger.LogInformation("Current Role Name: {Name}", role.Name);
            _logger.LogInformation("New Role Name: {Name}", updateRoleDto.Name);

            // Check for duplicate name only if name is changing
            if (role.Name != updateRoleDto.Name)
            {
                if (await _repository.AdminRepository.RoleExistsAsync(updateRoleDto.Name, roleId))
                {
                    await CreateAuditLog(
                        "Role Update Failed",
                        $"Role name already exists: {updateRoleDto.Name}",
                        "Role Management"
                    );
                    return ResponseFactory.Fail<RoleResponseDto>("Role name already exists");
                }

                changes.Add($"Name: {role.Name} → {updateRoleDto.Name}");
                role.Name = updateRoleDto.Name;
                role.NormalizedName = updateRoleDto.Name.ToUpper();
            }

            var originalPermissions = role.Permissions.Select(p => p.Name).ToList();
            var addedPermissions = updateRoleDto.Permissions.Except(originalPermissions).ToList();
            var removedPermissions = originalPermissions.Except(updateRoleDto.Permissions).ToList();

            /*            if (addedPermissions.Any() || removedPermissions.Any())
                        {
                            if (addedPermissions.Any())
                                changes.Add($"Added permissions: {string.Join(", ", addedPermissions)}");
                            if (removedPermissions.Any())
                                changes.Add($"Removed permissions: {string.Join(", ", removedPermissions)}");

                            // First, remove permissions that should be removed
                            var permissionsToRemove = role.Permissions
                                .Where(p => removedPermissions.Contains(p.Name))
                                .ToList();

                            foreach (var permission in permissionsToRemove)
                            {
                                _dbContext.RolePermissions.Remove(permission);
                            }

                            // Then add new permissions
                            var permissionsToAdd = addedPermissions.Select(p => new RolePermission
                            {
                                Id = Guid.NewGuid().ToString(),
                                RoleId = roleId,
                                Name = p,
                                IsGranted = true
                            }).ToList();

                            await _dbContext.RolePermissions.AddRangeAsync(permissionsToAdd);

                            // Save changes immediately to handle the permissions
                            await _dbContext.SaveChangesAsync();

                            // Refresh the role's permissions
                            role.Permissions = role.Permissions
                                .Where(p => !removedPermissions.Contains(p.Name))
                                .Concat(permissionsToAdd)
                                .ToList();
                        }*/

            if (addedPermissions.Any() || removedPermissions.Any())
            {
                if (addedPermissions.Any())
                    changes.Add($"Added permissions: {string.Join(", ", addedPermissions)}");
                if (removedPermissions.Any())
                    changes.Add($"Removed permissions: {string.Join(", ", removedPermissions)}");

                // Remove old permissions
                var permissionsToRemove = role.Permissions
                    .Where(p => removedPermissions.Contains(p.Name))
                    .ToList();

                _repository.AdminRepository.DeleteRolePermissions(permissionsToRemove);

                // Add new permissions
                var permissionsToAdd = addedPermissions.Select(p => new RolePermission
                {
                    Id = Guid.NewGuid().ToString(),
                    RoleId = roleId,
                    Name = p,
                    IsGranted = true
                }).ToList();

                await _repository.AdminRepository.AddRolePermissionsAsync(permissionsToAdd);

                // Save changes to handle permissions
                await _repository.SaveChangesAsync();

                // Update the role's permissions collection
                role.Permissions = role.Permissions
                    .Where(p => !removedPermissions.Contains(p.Name))
                    .Concat(permissionsToAdd)
                    .ToList();
            }

            // Update other role properties
            /*  if (role.Name != updateRoleDto.Name)
              {
                  if (await _repository.AdminRepository.RoleExistsAsync(updateRoleDto.Name, roleId))
                  {
                      await CreateAuditLog(
                          "Role Update Failed",
                          $"Role name already exists: {updateRoleDto.Name}",
                          "Role Management"
                      );
                      return ResponseFactory.Fail<RoleResponseDto>("Role name already exists");
                  }

                  changes.Add($"Name: {role.Name} → {updateRoleDto.Name}");
                  role.Name = updateRoleDto.Name;
                  role.NormalizedName = updateRoleDto.Name.ToUpper();
              }*/

            // Update description if changed
            if (role.Description != updateRoleDto.Description)
            {
                changes.Add($"Description: {role.Description} → {updateRoleDto.Description}");
                role.Description = updateRoleDto.Description;
            }

            // Update IsActive status if changed
            if (role.IsActive != updateRoleDto.IsActive)
            {
                changes.Add($"Active Status: {role.IsActive} → {updateRoleDto.IsActive}");
                role.IsActive = updateRoleDto.IsActive;
            }

            if (changes.Any())
            {
                role.LastModifiedBy = userId;
                role.LastModifiedAt = DateTime.UtcNow;

                _repository.AdminRepository.UpdateRole(role);
                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Updated Role",
                    $"Updated role {role.Name}. Changes: {string.Join("; ", changes)}",
                    "Role Management"
                );
            }

            var responseDto = _mapper.Map<RoleResponseDto>(role);
            return ResponseFactory.Success(responseDto, changes.Any()
                ? "Role updated successfully"
                : "No changes were made to role");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role");
            return ResponseFactory.Fail<RoleResponseDto>(ex, "An unexpected error occurred");
        }
    }
    /* public async Task<BaseResponse<RoleResponseDto>> UpdateRole(string roleId, UpdateRoleRequestDto updateRoleDto)
     {
         try
         {
             var validationResult = await _updateRoleValidator.ValidateAsync(updateRoleDto);
             if (!validationResult.IsValid)
             {
                 await CreateAuditLog(
                     "Role Update Failed",
                     $"Validation failed for role update ID: {roleId}",
                     "Role Management"
                 );
                 return ResponseFactory.Fail<RoleResponseDto>(
                     new ValidationException(validationResult.Errors),
                     "Validation failed");
             }

             var role = await _repository.AdminRepository.GetRoleByIdAsync(roleId, trackChanges: true);
             if (role == null)
             {
                 await CreateAuditLog(
                     "Role Update Failed",
                     $"Role not found for ID: {roleId}",
                     "Role Management"
                 );
                 return ResponseFactory.Fail<RoleResponseDto>(
                     new NotFoundException("Role not found"),
                     "Role not found");
             }

             // Check for duplicate name
             if (await _repository.AdminRepository.RoleExistsAsync(updateRoleDto.Name, roleId))
             {
                 await CreateAuditLog(
                     "Role Update Failed",
                     $"Role name already exists: {updateRoleDto.Name}",
                     "Role Management"
                 );
                 return ResponseFactory.Fail<RoleResponseDto>("Role name already exists");
             }

             // Track changes for audit
             var originalPermissions = role.Permissions.Select(p => p.Name).ToList();
             var addedPermissions = updateRoleDto.Permissions.Except(originalPermissions).ToList();
             var removedPermissions = originalPermissions.Except(updateRoleDto.Permissions).ToList();

             // Update role properties
             role.Name = updateRoleDto.Name;
             role.NormalizedName = updateRoleDto.Name.ToUpper();
             role.LastModifiedBy = _currentUser.GetUserId();
             role.LastModifiedAt = DateTime.UtcNow;

             // Update permissions
             role.Permissions.Clear();
             role.Permissions = updateRoleDto.Permissions.Select(p => new RolePermission
             {
                 Id = Guid.NewGuid().ToString(),
                 Name = p,
                 RoleId = roleId,  // Add this line to set the RoleId
                 IsGranted = true
             }).ToList();

             _repository.AdminRepository.UpdateRole(role);
             await _repository.SaveChangesAsync();

             var changes = new List<string>();
             if (addedPermissions.Any())
                 changes.Add($"Added permissions: {string.Join(", ", addedPermissions)}");
             if (removedPermissions.Any())
                 changes.Add($"Removed permissions: {string.Join(", ", removedPermissions)}");

             await CreateAuditLog(
                 "Updated Role",
                 $"Updated role {role.Name}. Changes: {string.Join("; ", changes)}",
                 "Role Management"
             );

             var responseDto = _mapper.Map<RoleResponseDto>(role);
             return ResponseFactory.Success(responseDto, "Role updated successfully");
         }
         catch (Exception ex)
         {
             _logger.LogError(ex, "Error updating role");
             return ResponseFactory.Fail<RoleResponseDto>(ex, "An unexpected error occurred");
         }
     }
 */
    public async Task<BaseResponse<bool>> DeleteRole(string roleId)
    {
        try
        {
            var role = await _repository.AdminRepository.GetRoleByIdAsync(roleId, trackChanges: true);
            if (role == null)
            {
                await CreateAuditLog(
                    "Role Deletion Failed",
                    $"Role not found for ID: {roleId}",
                    "Role Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("Role not found"),
                    "Role not found");
            }

            // Check if trying to delete Admin role
            if (role.Name.Equals(UserRoles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                await CreateAuditLog(
                    "Protected Role Deletion Attempted",
                    $"Attempted to delete protected Admin role (ID: {roleId})",
                    "Role Management"
                );
                return ResponseFactory.Fail<bool>(
                    new UnauthorizedException("Protected role"),
                    "The Admin role cannot be deleted as it is a protected system role");
            }

            // Check if role is in use
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            if (usersInRole.Any())
            {
                await CreateAuditLog(
                    "Role Deletion Failed",
                    $"Role {role.Name} is still assigned to users",
                    "Role Management"
                );
                return ResponseFactory.Fail<bool>("Cannot delete role as it is assigned to users");
            }

            // Instead of Clear(), remove each permission directly
            if (role.Permissions != null && role.Permissions.Any())
            {
                foreach (var permission in role.Permissions.ToList())
                {
                    _repository.AdminRepository.DeleteRolePermission(permission);
                }
                await _repository.SaveChangesAsync();
            }

            _repository.AdminRepository.DeleteRole(role);
            await _repository.SaveChangesAsync();

            await CreateAuditLog(
                "Deleted Role",
                $"Deleted role {role.Name}",
                "Role Management"
            );

            return ResponseFactory.Success(true, "Role deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role");
            return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
        }
    }
    public async Task<BaseResponse<TeamMemberResponseDto>> CreateTeamMember(CreateTeamMemberRequestDto requestDto)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);

            if (currentAdmin == null || !currentAdmin.HasTeamManagementAccess)
            {
                await CreateAuditLog(
                    "Team Member Creation Denied",
                    $"Unauthorized team member creation attempt by user: {userId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<TeamMemberResponseDto>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to create team members");
            }

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(requestDto.EmailAddress);
            if (existingUser != null)
            {
                await CreateAuditLog(
                    "Team Member Creation Failed",
                    $"Email already exists: {requestDto.EmailAddress}",
                    "Team Management"
                );
                return ResponseFactory.Fail<TeamMemberResponseDto>("Email address already exists");
            }

            // Generate a new unique ID for the team member
            var newUserId = Guid.NewGuid().ToString();

            var user = new ApplicationUser
            {
                Id = newUserId,    // Use the new unique ID instead of admin's ID
                UserName = requestDto.EmailAddress,
                Email = requestDto.EmailAddress,
                PhoneNumber = requestDto.PhoneNumber,
                NormalizedUserName = requestDto.EmailAddress,
                NormalizedEmail = requestDto.EmailAddress,
                FirstName = requestDto.FullName.Split(' ')[0],
                LastName = requestDto.FullName.Contains(" ") ?
                    string.Join(" ", requestDto.FullName.Split(' ').Skip(1)) : "",
                ProfileImageUrl = "",
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createUserResult = await _userManager.CreateAsync(user);
            if (!createUserResult.Succeeded)
            {
                await CreateAuditLog(
                    "Team Member Creation Failed",
                    $"Failed to create user account for: {requestDto.EmailAddress}",
                    "Team Management"
                );
                return ResponseFactory.Fail<TeamMemberResponseDto>(
                    new ValidationException(createUserResult.Errors.Select(e =>
                        new ValidationFailure(e.Code, e.Description))),
                    "Failed to create user account");
            }

            // Add to team member role
            await _userManager.AddToRoleAsync(user, UserRoles.TeamMember);

            await CreateAuditLog(
                "Created Team Member",
                $"Created team member account for {user.Email} ({requestDto.FullName})",
                "Team Management"
            );

            var responseDto = new TeamMemberResponseDto
            {
                Id = user.Id,
                FullName = requestDto.FullName,
                PhoneNumber = requestDto.PhoneNumber,
                EmailAddress = requestDto.EmailAddress,
                DateAdded = user.CreatedAt
            };

            return ResponseFactory.Success(responseDto, "Team member created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating team member");
            return ResponseFactory.Fail<TeamMemberResponseDto>(ex, "An unexpected error occurred");
        }
    }
    public async Task<BaseResponse<TeamMemberResponseDto>> UpdateTeamMember(
       string memberId,
       UpdateTeamMemberRequestDto requestDto)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);

            if (currentAdmin == null || !currentAdmin.HasTeamManagementAccess)
            {
                await CreateAuditLog(
                    "Team Member Update Denied",
                    $"Unauthorized team member update attempt by user: {userId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<TeamMemberResponseDto>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to update team members");
            }

            var user = await _userManager.FindByIdAsync(memberId);
            if (user == null)
            {
                await CreateAuditLog(
                    "Team Member Update Failed",
                    $"Team member not found with ID: {memberId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<TeamMemberResponseDto>(
                    new NotFoundException("Team member not found"),
                    "Team member not found");
            }

            // Track changes for audit log
            var changes = new List<string>();

            // Debug logging
            _logger.LogInformation("Current Values - FirstName: {FirstName}, LastName: {LastName}",
                user.FirstName, user.LastName);
            _logger.LogInformation("New FullName: {FullName}", requestDto.FullName);

            // Update name if provided
            if (!string.IsNullOrWhiteSpace(requestDto.FullName))
            {
                var currentFullName = $"{user.FirstName} {user.LastName}".Trim();
                var newFullName = requestDto.FullName.Trim();

                if (currentFullName != newFullName)
                {
                    changes.Add($"Name: {currentFullName} → {newFullName}");

                    var nameParts = newFullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    user.FirstName = nameParts[0];
                    user.LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";
                }
            }

            // Update email if provided
            if (!string.IsNullOrWhiteSpace(requestDto.EmailAddress) && user.Email != requestDto.EmailAddress)
            {
                changes.Add($"Email: {user.Email} → {requestDto.EmailAddress}");
                user.Email = requestDto.EmailAddress;
                user.UserName = requestDto.EmailAddress;
                user.EmailConfirmed = true;
            }

            // Update phone if provided
            if (!string.IsNullOrWhiteSpace(requestDto.PhoneNumber) && user.PhoneNumber != requestDto.PhoneNumber)
            {
                changes.Add($"Phone: {user.PhoneNumber} → {requestDto.PhoneNumber}");
                user.PhoneNumber = requestDto.PhoneNumber;
            }

            // Only proceed with update if there are changes
            if (changes.Any())
            {
                _logger.LogInformation("Applying changes: {@Changes}", changes);

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    await CreateAuditLog(
                        "Team Member Update Failed",
                        $"Failed to update user properties for ID: {memberId}",
                        "Team Management"
                    );
                    return ResponseFactory.Fail<TeamMemberResponseDto>(
                        new ValidationException(updateResult.Errors.Select(e =>
                            new ValidationFailure(e.Code, e.Description))),
                        "Failed to update team member");
                }

                await CreateAuditLog(
                    "Updated Team Member",
                    $"Updated team member {user.Email}. Changes: {string.Join(", ", changes)}",
                    "Team Management"
                );
            }
            else
            {
                _logger.LogInformation("No changes detected for user {UserId}", memberId);
            }

            var responseDto = new TeamMemberResponseDto
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                EmailAddress = user.Email,
                DateAdded = user.CreatedAt
            };

            return ResponseFactory.Success(responseDto, changes.Any()
                ? "Team member updated successfully"
                : "No changes were made to team member");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating team member");
            return ResponseFactory.Fail<TeamMemberResponseDto>(ex, "An unexpected error occurred");
        }
    }
    public async Task<BaseResponse<TeamMemberResponseDto>> GetTeamMemberById(string memberId)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);

            if (currentAdmin == null || !currentAdmin.HasTeamManagementAccess)
            {
                await CreateAuditLog(
                    "Team Member Access Denied",
                    $"Unauthorized team member access attempt by user: {userId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<TeamMemberResponseDto>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to view team members");
            }

            var user = await _userManager.FindByIdAsync(memberId);
            if (user == null)
            {
                await CreateAuditLog(
                    "Team Member Lookup Failed",
                    $"Team member not found with ID: {memberId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<TeamMemberResponseDto>(
                    new NotFoundException("Team member not found"),
                    "Team member not found");
            }

            var responseDto = new TeamMemberResponseDto
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                PhoneNumber = user.PhoneNumber,
                EmailAddress = user.Email,
                DateAdded = user.CreatedAt
            };

            await CreateAuditLog(
                "Team Member Lookup",
                $"Retrieved team member details for ID: {memberId}",
                "Team Management"
            );

            return ResponseFactory.Success(responseDto, "Team member retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving team member");
            return ResponseFactory.Fail<TeamMemberResponseDto>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<bool>> DeleteTeamMember(string memberId)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);

            if (currentAdmin == null || !currentAdmin.HasTeamManagementAccess)
            {
                await CreateAuditLog(
                    "Team Member Deletion Denied",
                    $"Unauthorized team member deletion attempt by user: {userId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<bool>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to delete team members");
            }

            var user = await _userManager.FindByIdAsync(memberId);
            if (user == null)
            {
                await CreateAuditLog(
                    "Team Member Deletion Failed",
                    $"Team member not found with ID: {memberId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("Team member not found"),
                    "Team member not found");
            }

            // Instead of hard delete, deactivate the user
            user.IsActive = false;
            var updateResult = await _userManager.DeleteAsync(user);
            if (!updateResult.Succeeded)
            {
                await CreateAuditLog(
                    "Team Member Deletion Failed",
                    $"Failed to deactivate user account for ID: {memberId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<bool>(
                    new ValidationException(updateResult.Errors.Select(e =>
                        new ValidationFailure(e.Code, e.Description))),
                    "Failed to delete team member");
            }

            await CreateAuditLog(
                "Deleted Team Member",
                $"Deactivated team member account for {user.Email} ({user.FirstName} {user.LastName})",
                "Team Management"
            );

            return ResponseFactory.Success(true, "Team member deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting team member");
            return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
        }
    }
    public async Task<BaseResponse<PaginatorDto<IEnumerable<TeamMemberResponseDto>>>> GetTeamMembers(
     TeamMemberFilterRequestDto filterDto,
     PaginationFilter paginationFilter)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);

            if (currentAdmin == null || !currentAdmin.HasTeamManagementAccess)
            {
                await CreateAuditLog(
                    "Team Members List Access Denied",
                    $"Unauthorized team members list access attempt by user: {userId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<TeamMemberResponseDto>>>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to view team members");
            }

            // Get all team member user IDs from UserRoles table
            var teamMemberIds = await _userManager.GetUsersInRoleAsync(UserRoles.TeamMember);
            var teamMemberIdList = teamMemberIds.Select(u => u.Id).ToList();

            // Query users with those IDs
            var query = _userManager.Users.Where(u => teamMemberIdList.Contains(u.Id));

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(filterDto?.SearchTerm))
            {
                var searchTerm = filterDto.SearchTerm.Trim().ToLower();
                query = query.Where(u =>
                    (u.FirstName + " " + u.LastName).ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)));
            }

            // Only include active members
            query = query.Where(u => u.IsActive);

            // Apply ordering
            query = query.OrderByDescending(u => u.CreatedAt);

            // Apply pagination
            var paginatedMembers = await query.Paginate(paginationFilter);

            // Map to response DTOs
            var memberDtos = paginatedMembers.PageItems.Select(user => new TeamMemberResponseDto
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                EmailAddress = user.Email,
                DateAdded = user.CreatedAt
            });

            // Create paginated result
            var result = new PaginatorDto<IEnumerable<TeamMemberResponseDto>>
            {
                PageItems = memberDtos,
                PageSize = paginatedMembers.PageSize,
                CurrentPage = paginatedMembers.CurrentPage,
                NumberOfPages = paginatedMembers.NumberOfPages
            };

            await CreateAuditLog(
                "Team Members List Retrieved",
                $"Retrieved team members list - Page {paginationFilter.PageNumber}, " +
                $"Size {paginationFilter.PageSize}, Search: {filterDto?.SearchTerm ?? "none"}",
                "Team Management"
            );

            return ResponseFactory.Success(result, "Team members retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving team members list: {Error}", ex.Message);
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<TeamMemberResponseDto>>>(
                ex, "An unexpected error occurred");
        }
    }
}