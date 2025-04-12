using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Application.IServices;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Exceptions;
using AutoMapper;
using SabiMarket.Domain.Enum;
using SabiMarket.Infrastructure.Utilities;
using SabiMarket.Infrastructure.Helpers;
using SabiMarket.Domain.Entities;
using Microsoft.AspNetCore.Http;
using ValidationException = FluentValidation.ValidationException;

namespace SabiMarket.Infrastructure.Services
{
    public class CaretakerService : ICaretakerService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILogger<CaretakerService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IValidator<LoginRequestDto> _loginValidator;
        private readonly IValidator<UpdateProfileDto> _updateProfileValidator;
        private readonly IValidator<CreateGoodBoyDto> _createGoodBoyValidator;
        private readonly ICurrentUserService _currentUser;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IValidator<CaretakerForCreationRequestDto> _createCaretakerValidator;


        public CaretakerService(
            IRepositoryManager repository,
            ILogger<CaretakerService> logger,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IValidator<LoginRequestDto> loginValidator,
            IValidator<ChangePasswordDto> changePasswordValidator,
            IValidator<UpdateProfileDto> updateProfileValidator,
            IValidator<CreateGoodBoyDto> createGoodBoyValidator,
            ICurrentUserService currentUser = null,
            IHttpContextAccessor httpContextAccessor = null,
            IValidator<CaretakerForCreationRequestDto> createCaretakerValidator = null)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _loginValidator = loginValidator;
            _updateProfileValidator = updateProfileValidator;
            _createGoodBoyValidator = createGoodBoyValidator;
            _currentUser = currentUser;
            _httpContextAccessor = httpContextAccessor;
            _createCaretakerValidator = createCaretakerValidator;
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

        // Caretaker Management Methods
        public async Task<BaseResponse<CaretakerResponseDto>> GetCaretakerById(string userId)
        {
            try
            {
                var caretaker = await _repository.CaretakerRepository.GetCaretakerById(userId, trackChanges: false);
                if (caretaker == null)
                {
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new NotFoundException("Caretaker not found"),
                        "Caretaker not found");
                }

                var caretakerDto = _mapper.Map<CaretakerResponseDto>(caretaker);
                return ResponseFactory.Success(caretakerDto, "Caretaker retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving caretaker");
                return ResponseFactory.Fail<CaretakerResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<CaretakerResponseDto>> CreateCaretaker(CaretakerForCreationRequestDto request)
        {
            var correlationId = Guid.NewGuid().ToString();
            var userId = _currentUser.GetUserId();

            try
            {
                // Validate request
                var validationResult = await _createCaretakerValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new ValidationException(validationResult.Errors),
                        "Invalid caretaker data"
                    );
                }

                // Check if user already exists by email
                var existingUser = await _userManager.FindByEmailAsync(request.EmailAddress);
                if (existingUser != null)
                {
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new BadRequestException("Email address is already registered"),
                        "Email already exists"
                    );
                }

                // Check if the market already has a caretaker
                var existingCaretaker = await _repository.CaretakerRepository.CaretakerExists(userId, request.MarketId);
                if (existingCaretaker)
                {
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new BadRequestException("Market already has an assigned caretaker"),
                        "Caretaker already exists for this market"
                    );
                }

                // Create ApplicationUser
                var defaultPassword = GenerateDefaultPassword(request.FullName);
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = request.EmailAddress,
                    Email = request.EmailAddress,
                    PhoneNumber = request.PhoneNumber,
                    FirstName = request.FullName.Split(' ')[0],
                    LastName = request.FullName.Split(' ').Length > 1 ? string.Join(" ", request.FullName.Split(' ').Skip(1)) : "",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Gender = request.Gender,
                    ProfileImageUrl = "",
                    LocalGovernmentId = request.LocalGovernmentId
                };

                var createUserResult = await _userManager.CreateAsync(user, defaultPassword);
                if (!createUserResult.Succeeded)
                {
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new Exception(string.Join(", ", createUserResult.Errors.Select(e => e.Description))),
                        "Failed to create user account"
                    );
                }

                // Assign role
                var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.Caretaker);
                if (!roleResult.Succeeded)
                {
                    // Rollback user creation if role assignment fails
                    await _userManager.DeleteAsync(user);
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new Exception("Failed to assign caretaker role"),
                        "Role assignment failed"
                    );
                }

                // Get Chairman details
                var chairman = await _repository.ChairmanRepository.GetChairmanById(userId, false);
                if (chairman == null)
                {
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new NotFoundException("Chairman not found"),
                        "Chairman does not exist"
                    );
                }

                // Create Caretaker entity
                var caretaker = new Caretaker
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    MarketId = request.MarketId,
                    ChairmanId = chairman.Id,  // Set the ChairmanId here
                    LocalGovernmentId = request.LocalGovernmentId,
                    IsBlocked = false, 
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    User = user
                };

                // Add caretaker to the repository
                _repository.CaretakerRepository.CreateCaretaker(caretaker);
                await _repository.SaveChangesAsync();

                // Map response
                var response = _mapper.Map<CaretakerResponseDto>(caretaker);
                response.DefaultPassword = defaultPassword;

                return ResponseFactory.Success(response,
                    "Caretaker created successfully. Please note down the default password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating caretaker");
                return ResponseFactory.Fail<CaretakerResponseDto>(ex, "An unexpected error occurred");
            }
        }


        /* public async Task<BaseResponse<CaretakerResponseDto>> CreateCaretaker(CaretakerForCreationRequestDto request)
         {
             var correlationId = Guid.NewGuid().ToString();
             var userId = _currentUser.GetUserId();
             try
             {
                 await CreateAuditLog(
                     "Caretaker Creation",
                     $"CorrelationId: {correlationId} - Creating new caretaker: {request.FullName}",
                     "Caretaker Management"
                 );

                 // Validate request
                 var validationResult = await _createCaretakerValidator.ValidateAsync(request);
                 if (!validationResult.IsValid)
                 {
                     await CreateAuditLog(
                         "Creation Failed",
                         $"CorrelationId: {correlationId} - Validation failed",
                         "Caretaker Management"
                     );
                     return ResponseFactory.Fail<CaretakerResponseDto>(
                         new ValidationException(validationResult.Errors),
                         "Validation failed"
                     );
                 }

                 // Check if user already exists by email
                 var existingUser = await _userManager.FindByEmailAsync(request.EmailAddress);
                 if (existingUser != null)
                 {
                     await CreateAuditLog(
                         "Creation Failed",
                         $"CorrelationId: {correlationId} - Email already registered",
                         "Caretaker Management"
                     );
                     return ResponseFactory.Fail<CaretakerResponseDto>(
                         new BadRequestException("Email address is already registered"),
                         "Email already exists"
                     );
                 }

                 // Check if the market already has a caretaker
                 var existingCaretaker = await _repository.CaretakerRepository.CaretakerExists(userId, request.MarketId);
                 if (existingCaretaker)
                 {
                     await CreateAuditLog(
                         "Creation Failed",
                         $"CorrelationId: {correlationId} - Market already has a caretaker",
                         "Caretaker Management"
                     );
                     return ResponseFactory.Fail<CaretakerResponseDto>(
                         new BadRequestException("Market already has an assigned caretaker"),
                         "Caretaker already exists for this market"
                     );
                 }

                 // Create ApplicationUser
                 var defaultPassword = GenerateDefaultPassword(request.FullName);
                 var user = new ApplicationUser
                 {
                     Id = Guid.NewGuid().ToString(),
                     UserName = request.EmailAddress,
                     Email = request.EmailAddress,
                     PhoneNumber = request.PhoneNumber,
                     FirstName = request.FullName.Split(' ')[0],
                     LastName = request.FullName.Split(' ').Length > 1 ? string.Join(" ", request.FullName.Split(' ').Skip(1)) : "",
                     EmailConfirmed = true,
                     IsActive = true,
                     CreatedAt = DateTime.UtcNow,
                     Gender = request.Gender,
                     ProfileImageUrl = "",
                     LocalGovernmentId = request.LocalGovernmentId
                 };

                 var createUserResult = await _userManager.CreateAsync(user, defaultPassword);
                 if (!createUserResult.Succeeded)
                 {
                     await CreateAuditLog(
                         "Creation Failed",
                         $"CorrelationId: {correlationId} - Failed to create user account",
                         "Caretaker Management"
                     );
                     return ResponseFactory.Fail<CaretakerResponseDto>(
                         new Exception(string.Join(", ", createUserResult.Errors.Select(e => e.Description))),
                         "Failed to create user account"
                     );
                 }

                 // Assign role
                 var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.Caretaker);
                 if (!roleResult.Succeeded)
                 {
                     // Rollback user creation if role assignment fails
                     await _userManager.DeleteAsync(user);
                     await CreateAuditLog(
                         "Creation Failed",
                         $"CorrelationId: {correlationId} - Failed to assign caretaker role",
                         "Caretaker Management"
                     );
                     return ResponseFactory.Fail<CaretakerResponseDto>(
                         new Exception("Failed to assign caretaker role"),
                         "Role assignment failed"
                     );
                 }

                 // Ensure ChairmanId exists in the database
                 var chairman = await _repository.ChairmanRepository.GetChairmanById(userId, false);
                 if (chairman == null)
                 {
                     await CreateAuditLog(
                         "Creation Failed",
                         $"CorrelationId: {correlationId} - Chairman not found with Id: {userId}",
                         "Caretaker Management"
                     );
                     return ResponseFactory.Fail<CaretakerResponseDto>(new NotFoundException("Chairman not found"), "Chairman does not exist");
                 }

                 // Create Caretaker entity
                 var caretaker = new Caretaker
                 {
                     Id = Guid.NewGuid().ToString(),
                     UserId = user.Id,
                     MarketId = request.MarketId,
                     ChairmanId = chairman.Id,  // Ensure ChairmanId exists
                     LocalGovernmentId = request.LocalGovernmentId,
                     IsBlocked = false,
                     CreatedAt = DateTime.UtcNow,
                     User = user
                 };

                 _repository.CaretakerRepository.CreateCaretaker(caretaker);
                 await _repository.SaveChangesAsync();

                 // Map response
                 var response = _mapper.Map<CaretakerResponseDto>(caretaker);
                 response.DefaultPassword = defaultPassword;

                 await CreateAuditLog(
                     "Caretaker Created",
                     $"CorrelationId: {correlationId} - Caretaker created successfully with ID: {caretaker.Id}",
                     "Caretaker Management"
                 );

                 return ResponseFactory.Success(response,
                     "Caretaker created successfully. Please note down the default password.");
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error creating caretaker");
                 await CreateAuditLog(
                     "Creation Failed",
                     $"CorrelationId: {correlationId} - Error: {ex.Message}",
                     "Caretaker Management"
                 );
                 return ResponseFactory.Fail<CaretakerResponseDto>(ex, "An unexpected error occurred");
             }
         }
 */

        private string GenerateDefaultPassword(string fullName)
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

        public async Task<BaseResponse<PaginatorDto<IEnumerable<CaretakerResponseDto>>>> GetCaretakers(
    PaginationFilter paginationFilter)
        {
            try
            {
                var caretakersPage = await _repository.CaretakerRepository
                    .GetCaretakersWithPagination(paginationFilter, trackChanges: false);

                // Replace AutoMapper with manual mapping
                var caretakerDtos = caretakersPage.PageItems.Select(c => new CaretakerResponseDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Email = c.User?.Email,
                    FirstName = c.User?.FirstName ?? "Default",  // Provide default if null
                    LastName = c.User?.LastName ?? "User",      // Provide default if null
                    MarketId = c.MarketId,
                    PhoneNumber = c.User?.PhoneNumber,
                    ProfileImageUrl = c.User?.ProfileImageUrl ?? "",
                    IsActive = c.User?.IsActive ?? false,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    IsBlocked = c.IsBlocked
                }).ToList();

                var response = new PaginatorDto<IEnumerable<CaretakerResponseDto>>
                {
                    PageItems = caretakerDtos,
                    CurrentPage = caretakersPage.CurrentPage,
                    PageSize = caretakersPage.PageSize,
                    NumberOfPages = caretakersPage.NumberOfPages
                };

                return ResponseFactory.Success(response, "Caretakers retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving caretakers");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<CaretakerResponseDto>>>(
                    ex, "An unexpected error occurred");
            }
        }


        /*  public async Task<BaseResponse<CaretakerResponseDto>> CreateCaretaker(CaretakerForCreationRequestDto caretakerDto)
          {
              try
              {
                  var userId = _currentUser.GetUserId();
                  var exists = await _repository.CaretakerRepository.CaretakerExists(userId, caretakerDto.MarketId);
                  if (exists)
                  {
                      return ResponseFactory.Fail<CaretakerResponseDto>(
                          new BadRequestException("Caretaker already exists for this market"),
                          "Caretaker already exists");
                  }

                  var caretaker = _mapper.Map<Caretaker>(caretakerDto);
                  _repository.CaretakerRepository.CreateCaretaker(caretaker);
                  await _repository.SaveChangesAsync();

                  var createdCaretaker = _mapper.Map<CaretakerResponseDto>(caretaker);
                  return ResponseFactory.Success(createdCaretaker, "Caretaker created successfully");
              }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "Error creating caretaker");
                  return ResponseFactory.Fail<CaretakerResponseDto>(ex, "An unexpected error occurred");
              }
          }
  */

        //Recent on Caretaker 
        /*  public async Task<BaseResponse<PaginatorDto<IEnumerable<CaretakerResponseDto>>>> GetCaretakers(
              PaginationFilter paginationFilter)
          {
              try
              {
                  var caretakersPage = await _repository.CaretakerRepository
                      .GetCaretakersWithPagination(paginationFilter, trackChanges: false);

                  var caretakerDtos = _mapper.Map<IEnumerable<CaretakerResponseDto>>(caretakersPage.PageItems);
                  var response = new PaginatorDto<IEnumerable<CaretakerResponseDto>>
                  {
                      PageItems = caretakerDtos,
                      CurrentPage = caretakersPage.CurrentPage,
                      PageSize = caretakersPage.PageSize,
                      NumberOfPages = caretakersPage.NumberOfPages
                  };

                  return ResponseFactory.Success(response, "Caretakers retrieved successfully");
              }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "Error retrieving caretakers");
                  return ResponseFactory.Fail<PaginatorDto<IEnumerable<CaretakerResponseDto>>>(
                      ex, "An unexpected error occurred");
              }
          }
  */
        // Trader Management Methods
        public async Task<BaseResponse<bool>> AssignTraderToCaretaker(string caretakerId, string traderId)
        {
            try
            {
                var caretaker = await _repository.CaretakerRepository
                    .GetCaretakerById(caretakerId, trackChanges: true);

                if (caretaker == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Caretaker not found"),
                        "Caretaker not found");
                }

                var trader = await _repository.TraderRepository.GetTraderById(traderId, trackChanges: true);
                if (trader == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Trader not found"),
                        "Trader not found");
                }

                if (trader.CaretakerId != null)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("Trader is already assigned to a caretaker"),
                        "Trader already assigned");
                }

                trader.CaretakerId = caretakerId;
                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "Trader assigned successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning trader");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        // Trader Management
        public async Task<BaseResponse<bool>> RemoveTraderFromCaretaker(string caretakerId, string traderId)
        {
            try
            {
                var caretaker = await _repository.CaretakerRepository
                    .GetCaretakerById(caretakerId, trackChanges: true);

                if (caretaker == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Caretaker not found"),
                        "Caretaker not found");
                }

                var trader = caretaker.AssignedTraders
                    .FirstOrDefault(t => t.Id == traderId);

                if (trader == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Trader not found or not assigned to this caretaker"),
                        "Trader not found");
                }

                trader.CaretakerId = null;
                caretaker.AssignedTraders.Remove(trader);
                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "Trader removed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing trader from caretaker");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>> GetAssignedTraders(
            string caretakerId, PaginationFilter paginationFilter)
        {
            try
            {
                var caretaker = await _repository.CaretakerRepository
                    .GetCaretakerById(caretakerId, trackChanges: false);

                if (caretaker == null)
                {
                    return ResponseFactory.Fail<PaginatorDto<IEnumerable<TraderResponseDto>>>(
                        new NotFoundException("Caretaker not found"),
                        "Caretaker not found");
                }

                var tradersQuery = caretaker.AssignedTraders.AsQueryable();
                var paginatedTraders = await tradersQuery.Paginate(paginationFilter);

                var traderDtos = _mapper.Map<IEnumerable<TraderResponseDto>>(paginatedTraders.PageItems);
                var response = new PaginatorDto<IEnumerable<TraderResponseDto>>
                {
                    PageItems = traderDtos,
                    CurrentPage = paginatedTraders.CurrentPage,
                    PageSize = paginatedTraders.PageSize,
                    NumberOfPages = paginatedTraders.NumberOfPages
                };

                return ResponseFactory.Success(response, "Traders retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assigned traders");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<TraderResponseDto>>>(
                    ex, "An unexpected error occurred");
            }
        }

        // Levy Management
        public async Task<BaseResponse<PaginatorDto<IEnumerable<LevyPaymentResponseDto>>>> GetLevyPayments(
            string caretakerId, PaginationFilter paginationFilter)
        {
            try
            {
                var levyPayments = await _repository.CaretakerRepository
                    .GetLevyPayments(caretakerId, paginationFilter, trackChanges: false);

                if (levyPayments == null)
                {
                    return ResponseFactory.Fail<PaginatorDto<IEnumerable<LevyPaymentResponseDto>>>(
                        new NotFoundException("No levy payments found"),
                        "Levy payments not found");
                }

                var levyPaymentDtos = _mapper.Map<IEnumerable<LevyPaymentResponseDto>>(levyPayments.PageItems);
                var response = new PaginatorDto<IEnumerable<LevyPaymentResponseDto>>
                {
                    PageItems = levyPaymentDtos,
                    CurrentPage = levyPayments.CurrentPage,
                    PageSize = levyPayments.PageSize,
                    NumberOfPages = levyPayments.NumberOfPages
                };

                return ResponseFactory.Success(response, "Levy payments retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving levy payments");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<LevyPaymentResponseDto>>>(
                    ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<LevyPaymentResponseDto>> GetLevyPaymentDetails(string levyId)
        {
            try
            {
                var levyPayment = await _repository.CaretakerRepository
                    .GetLevyPaymentDetails(levyId, trackChanges: false);

                if (levyPayment == null)
                {
                    return ResponseFactory.Fail<LevyPaymentResponseDto>(
                        new NotFoundException("Levy payment not found"),
                        "Levy payment not found");
                }

                var levyPaymentDto = _mapper.Map<LevyPaymentResponseDto>(levyPayment);
                return ResponseFactory.Success(levyPaymentDto, "Levy payment details retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving levy payment details");
                return ResponseFactory.Fail<LevyPaymentResponseDto>(ex, "An unexpected error occurred");
            }
        }

        // GoodBoy Management
        public async Task<BaseResponse<GoodBoyResponseDto>> AddGoodBoy(string caretakerId, CreateGoodBoyDto goodBoyDto)
        {
            try
            {
                var validationResult = await _createGoodBoyValidator.ValidateAsync(goodBoyDto);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Fail<GoodBoyResponseDto>(
                        new FluentValidation.ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                var caretaker = await _repository.CaretakerRepository
                    .GetCaretakerById(caretakerId, trackChanges: true);

                if (caretaker == null)
                {
                    return ResponseFactory.Fail<GoodBoyResponseDto>(
                        new NotFoundException("Caretaker not found"),
                        "Caretaker not found");
                }

                var nameParts = goodBoyDto.FullName.Trim().Split(' ', 2);
                var firstName = nameParts[0];
                var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;


                // Create user account for GoodBoy
                var user = new ApplicationUser
                {
                    UserName = goodBoyDto.Email,
                    Email = goodBoyDto.Email,
                    PhoneNumber = goodBoyDto.PhoneNumber,
                    FirstName = firstName,
                    LastName = lastName
                };
               
                var password = goodBoyDto.PhoneNumber.TrimStart('0');

                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    return ResponseFactory.Fail<GoodBoyResponseDto>(
                        new BadRequestException(result.Errors.First().Description),
                        "Failed to create user account");
                }

                // Create GoodBoy entity
                var goodBoy = new GoodBoy
                {
                    UserId = user.Id,
                    CaretakerId = caretakerId
                };

                 _repository.GoodBoyRepository.AddGoodBoy(goodBoy);    
                 await _repository.SaveChangesAsync();

                var goodBoyResponseDto = _mapper.Map<GoodBoyResponseDto>(goodBoy);
                return ResponseFactory.Success(goodBoyResponseDto, "GoodBoy created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating GoodBoy");
                return ResponseFactory.Fail<GoodBoyResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>> GetGoodBoys(
            string caretakerId, PaginationFilter paginationFilter)
        {
            try
            {
                var goodBoys = await _repository.CaretakerRepository
                    .GetGoodBoys(caretakerId, paginationFilter, trackChanges: false);

                var goodBoyDtos = _mapper.Map<IEnumerable<GoodBoyResponseDto>>(goodBoys.PageItems);
                var response = new PaginatorDto<IEnumerable<GoodBoyResponseDto>>
                {
                    PageItems = goodBoyDtos,
                    CurrentPage = goodBoys.CurrentPage,
                    PageSize = goodBoys.PageSize,
                    NumberOfPages = goodBoys.NumberOfPages
                };

                return ResponseFactory.Success(response, "GoodBoys retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GoodBoys");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>(
                    ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> BlockGoodBoy(string caretakerId, string goodBoyId)
        {
            try
            {
                var caretaker = await _repository.CaretakerRepository
                    .GetCaretakerById(caretakerId, trackChanges: true);

                if (caretaker == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Caretaker not found"),
                        "Caretaker not found");
                }

                var goodBoy = caretaker.GoodBoys
                    .FirstOrDefault(gb => gb.Id == goodBoyId);

                if (goodBoy == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("GoodBoy not found"),
                        "GoodBoy not found");
                }

                // Instead of removing, we update the status
                goodBoy.Status = StatusEnum.Blocked;  // You'd need to define this enum

                // Optionally disable the user account
                var user = await _userManager.FindByIdAsync(goodBoy.UserId);
                if (user != null)
                {
                    user.LockoutEnd = DateTimeOffset.MaxValue; // Or some other blocking mechanism
                    await _userManager.UpdateAsync(user);
                }

                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "GoodBoy blocked successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking GoodBoy");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> UnblockGoodBoy(string caretakerId, string goodBoyId)
        {
            try
            {
                var caretaker = await _repository.CaretakerRepository
                    .GetCaretakerById(caretakerId, trackChanges: true);

                if (caretaker == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Caretaker not found"),
                        "Caretaker not found");
                }

                var goodBoy = caretaker.GoodBoys
                    .FirstOrDefault(gb => gb.Id == goodBoyId);

                if (goodBoy == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("GoodBoy not found"),
                        "GoodBoy not found");
                }

                // Update the status to Active
                goodBoy.Status = StatusEnum.Unlocked; // Ensure the enum has an Active state

                // Optionally re-enable the user account
                var user = await _userManager.FindByIdAsync(goodBoy.UserId);
                if (user != null)
                {
                    user.LockoutEnd = null; // Remove the lockout
                    await _userManager.UpdateAsync(user);
                }

                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "GoodBoy unblocked successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unblocking GoodBoy");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

    }
}
