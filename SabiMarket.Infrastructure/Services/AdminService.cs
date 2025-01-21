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

    public AdminService(
        IRepositoryManager repository,
        ILogger<AdminService> logger,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUser,
        IHttpContextAccessor httpContextAccessor,
        IValidator<CreateAdminRequestDto> createAdminValidator,
        IValidator<UpdateAdminProfileDto> updateProfileValidator)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
        _userManager = userManager;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
        _createAdminValidator = createAdminValidator;
        _updateProfileValidator = updateProfileValidator;
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
                return ResponseFactory.Fail<AdminResponseDto>( "Email already exists");
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

            var logDtos = _mapper.Map<IEnumerable<AuditLogResponseDto>>(paginatedLogs.PageItems);
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

    /* public async Task<BaseResponse<bool>> UpdateDashboardAccess(string adminId, UpdateAdminAccessDto accessDto)
     {
         try
         {
             var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: true);
             if (admin == null)
             {
                 return ResponseFactory.Fail<bool>(
                     new NotFoundException("Admin not found"),
                     "Admin not found");
             }

             // Check if current user has permission to update access
             var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(_currentUser.UserId, trackChanges: false);
             if (currentAdmin == null || !currentAdmin.HasRoleManagementAccess)
             {
                 return ResponseFactory.Fail<bool>(
                     new UnauthorizedException("Access denied"),
                     "You don't have permission to update admin access");
             }

             // Update access permissions
             admin.HasDashboardAccess = accessDto.HasDashboardAccess;
             admin.HasRoleManagementAccess = accessDto.HasRoleManagementAccess;
             admin.HasTeamManagementAccess = accessDto.HasTeamManagementAccess;
             admin.HasAuditLogAccess = accessDto.HasAuditLogAccess;

             _repository.AdminRepository.UpdateAdmin(admin);

             // Create audit log
             var auditLog = new AuditLog
             {
                 UserId = _currentUser.UserId,
                 Activity = "Updated admin access permissions",
                 Module = "Admin Management",
                 Details = $"Updated access permissions for admin ID: {adminId}",
                 IpAddress = _currentUser.IpAddress
             };
             auditLog.SetDateTime(DateTime.UtcNow);
             _repository.AuditLogRepository.CreateAuditLog(auditLog);

             await _repository.SaveChangesAsync();

             return ResponseFactory.Success(true, "Admin access updated successfully");
         }
         catch (Exception ex)
         {
             _logger.LogError(ex, "Error updating admin access");
             return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
         }
     }
 */
    /*public async Task<BaseResponse<bool>> DeactivateAdmin(string adminId)
    {
        try
        {
            var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: true);
            if (admin == null)
            {
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("Admin not found"),
                    "Admin not found");
            }

            // Check if current user has permission
            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(_currentUser.UserId, trackChanges: false);
            if (currentAdmin == null || !currentAdmin.HasRoleManagementAccess)
            {
                return ResponseFactory.Fail<bool>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to deactivate admins");
            }

            // Get associated user
            var user = await _userManager.FindByIdAsync(admin.UserId);
            if (user == null)
            {
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("User not found"),
                    "Associated user not found");
            }

            // Deactivate user
            user.IsActive = false;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return ResponseFactory.Fail<bool>(
                    new ValidationException(updateResult.Errors.Select(e => e.Description)),
                    "Failed to deactivate admin");
            }

            // Create audit log
            var auditLog = new AuditLog
            {
                UserId = _currentUser.UserId,
                Activity = "Deactivated admin account",
                Module = "Admin Management",
                Details = $"Deactivated admin account for {user.Email}",
                IpAddress = _currentUser.IpAddress
            };
            auditLog.SetDateTime(DateTime.UtcNow);
            _repository.AuditLogRepository.CreateAuditLog(auditLog);

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
            var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: true);
            if (admin == null)
            {
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("Admin not found"),
                    "Admin not found");
            }

            // Check if current user has permission
            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(_currentUser.UserId, trackChanges: false);
            if (currentAdmin == null || !currentAdmin.HasRoleManagementAccess)
            {
                return ResponseFactory.Fail<bool>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to reactivate admins");
            }

            // Get associated user
            var user = await _userManager.FindByIdAsync(admin.UserId);
            if (user == null)
            {
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("User not found"),
                    "Associated user not found");
            }

            // Reactivate user
            user.IsActive = true;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return ResponseFactory.Fail<bool>(
                    new ValidationException(updateResult.Errors.Select(e => e.Description)),
                    "Failed to reactivate admin");
            }

            // Create audit log
            var auditLog = new AuditLog
            {
                UserId = _currentUser.UserId,
                Activity = "Reactivated admin account",
                Module = "Admin Management",
                Details = $"Reactivated admin account for {user.Email}",
                IpAddress = _currentUser.IpAddress
            };
            auditLog.SetDateTime(DateTime.UtcNow);
            _repository.AuditLogRepository.CreateAuditLog(auditLog);

            await _repository.SaveChangesAsync();

            return ResponseFactory.Success(true, "Admin reactivated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating admin");
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

            return ResponseFactory.Success(result, "Admins retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admins");
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<AdminResponseDto>>>(
                ex, "An unexpected error occurred");
        }
    }

    // Update GetAdminAuditLogs method:
    public async Task<BaseResponse<PaginatorDto<IEnumerable<AuditLogResponseDto>>>> GetAdminAuditLogs(
        string adminId, DateTime? startDate, DateTime? endDate, PaginationFilter paginationFilter)
    {
        try
        {
            var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: false);
            if (admin == null)
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<AuditLogResponseDto>>>(
                    new NotFoundException("Admin not found"),
                    "Admin not found");
            }

            if (!admin.HasAuditLogAccess)
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<AuditLogResponseDto>>>(
                    new UnauthorizedException("Access denied"),
                    "You don't have access to audit logs");
            }

            var query = _repository.AuditLogRepository.GetAdminAuditLogsQuery(adminId, startDate, endDate);
            var paginatedLogs = await query.Paginate(paginationFilter);

            var logDtos = _mapper.Map<IEnumerable<AuditLogResponseDto>>(paginatedLogs.PageItems);
            var result = new PaginatorDto<IEnumerable<AuditLogResponseDto>>
            {
                PageItems = logDtos,
                PageSize = paginatedLogs.PageSize,
                CurrentPage = paginatedLogs.CurrentPage,
                NumberOfPages = paginatedLogs.NumberOfPages
            };

            return ResponseFactory.Success(result, "Audit logs retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin audit logs");
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<AuditLogResponseDto>>>(
                ex, "An unexpected error occurred");
        }
    }*/
}