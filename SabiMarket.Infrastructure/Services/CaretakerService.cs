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
using System.Threading.Tasks.Sources;

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

        public CaretakerService(
            IRepositoryManager repository,
            ILogger<CaretakerService> logger,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IValidator<LoginRequestDto> loginValidator,
            IValidator<ChangePasswordDto> changePasswordValidator,
            IValidator<UpdateProfileDto> updateProfileValidator,
            IValidator<CreateGoodBoyDto> createGoodBoyValidator)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _loginValidator = loginValidator;
            _updateProfileValidator = updateProfileValidator;
            _createGoodBoyValidator = createGoodBoyValidator;
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

        public async Task<BaseResponse<CaretakerResponseDto>> CreateCaretaker(CaretakerForCreationRequestDto caretakerDto)
        {
            try
            {
                var exists = await _repository.CaretakerRepository.CaretakerExists(caretakerDto.UserId, caretakerDto.MarketId);
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

        public async Task<BaseResponse<PaginatorDto<IEnumerable<CaretakerResponseDto>>>> GetCaretakers(
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

    }
}
