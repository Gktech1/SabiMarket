using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Infrastructure.Helpers;
using System.Text.Json;
using SabiMarket.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Infrastructure.Utilities;
using SabiMarket.Domain.Enum;
using TraderDetailsDto = SabiMarket.Application.DTOs.Requests.TraderDetailsDto;

namespace SabiMarket.Infrastructure.Services
{
    public class AssistCenterOfficerService : IAssistCenterOfficerService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILogger<AssistCenterOfficerService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUser;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IValidator<CreateAssistCenterOfficerRequestDto> _createValidator;
        private readonly IValidator<UpdateAssistCenterOfficerProfileDto> _updateProfileValidator;

        public AssistCenterOfficerService(
            IRepositoryManager repository,
            ILogger<AssistCenterOfficerService> logger,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUser,
            IHttpContextAccessor httpContextAccessor,
            IValidator<CreateAssistCenterOfficerRequestDto> createValidator,
            IValidator<UpdateAssistCenterOfficerProfileDto> updateProfileValidator)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _currentUser = currentUser;
            _httpContextAccessor = httpContextAccessor;
            _createValidator = createValidator;
            _updateProfileValidator = updateProfileValidator;
        }

        private string GetCurrentIpAddress()
        {
            return _httpContextAccessor.GetRemoteIPAddress();
        }

        private async Task CreateAuditLog(string activity, string details, string module = "Assist Center Management")
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

        public async Task<BaseResponse<DashboardStatsDto>> GetDashboardStats(string officerId)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var officer = await _repository.AssistCenterOfficerRepository.GetByIdAsync(officerId, false);

                if (officer == null)
                {
                    return ResponseFactory.Fail<DashboardStatsDto>(
                        new NotFoundException("Officer not found"),
                        "Officer not found");
                }

                var todayLevies = await _repository.LevyPaymentRepository.GetTodayLevies(officerId);

                var stats = new DashboardStatsDto
                {
                    TotalTraders = await _repository.TraderRepository.CountTraders(),
                    TotalLevies = await _repository.LevyPaymentRepository.GetTotalLevies(officerId),
                    TodayLevies = todayLevies
                };


                return ResponseFactory.Success(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard stats");
                return ResponseFactory.Fail<DashboardStatsDto>(ex);
            }
        }
        public async Task<BaseResponse<bool>> ProcessLevyPayment(ProcessLevyPaymentDto paymentDto)
        {
            try
            {
                var trader = await _repository.TraderRepository.GetTraderById(paymentDto.TraderId, false);
                if (trader == null)
                {
                    await CreateAuditLog(
                        "Levy Payment Failed",
                        $"Trader not found for ID: {paymentDto.TraderId}",
                        "Levy Payment"
                    );
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Trader not found"),
                        "Trader not found");
                }

                // Verify no duplicate payment for today
                var existingPayment = await _repository.LevyPaymentRepository
                    .FindByCondition(p =>
                        p.TraderId == paymentDto.TraderId &&
                        p.PaymentDate.Date == DateTime.UtcNow.Date, false)
                    .AnyAsync();

                if (existingPayment)
                {
                    return ResponseFactory.Fail<bool>(
                        "Payment already processed for today");
                }

                var levyPayment = new LevyPayment
                {
                    TraderId = paymentDto.TraderId,
                    Amount = paymentDto.Amount,
                    PaymentDate = DateTime.UtcNow,
                    Id = _currentUser.GetUserId(),
                    PaymentStatus = PaymentStatusEnum.Paid
                };

                _repository.LevyPaymentRepository.Create(levyPayment);

                await CreateAuditLog(
                    "Levy Payment Processed",
                    $"Processed levy payment of {paymentDto.Amount} for Trader ID: {paymentDto.TraderId}",
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

        public async Task<BaseResponse<IEnumerable<LevyPaymentResponseDto>>> GetLevyPayments(
            string marketId, LevyPaymentFilterDto filterDto)
        {
            try
            {
                var query = _repository.LevyPaymentRepository.FindAll(false)
                    .Include(p => p.Trader)
                    .Where(p => p.Trader.MarketId == marketId);

                // Apply filters
                if (filterDto.FromDate.HasValue)
                {
                    query = query.Where(p => p.PaymentDate.Date >= filterDto.FromDate.Value.Date);
                }

                if (filterDto.ToDate.HasValue)
                {
                    query = query.Where(p => p.PaymentDate.Date <= filterDto.ToDate.Value.Date);
                }

                if (filterDto.Status.HasValue)
                {
                    query = query.Where(p => p.PaymentStatus == filterDto.Status.Value);
                }

                var payments = await query
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                var paymentDtos = _mapper.Map<IEnumerable<LevyPaymentResponseDto>>(payments);

                return ResponseFactory.Success(paymentDtos, "Levy payments retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving levy payments");
                return ResponseFactory.Fail<IEnumerable<LevyPaymentResponseDto>>(ex, "An unexpected error occurred");
            }
        }

        /* public async Task<BaseResponse<LevyPaymentResponseDto>> CreateLevyPayment(CreateLevyPaymentDto createDto)
         {
             try
             {
                 // Verify trader exists
                 var trader = await _repository.TraderRepository.GetTraderById(createDto.TraderId, false);
                 if (trader == null)
                 {
                     return ResponseFactory.Fail<LevyPaymentResponseDto>(
                         new NotFoundException("Trader not found"));
                 }

                 // Check for duplicate payment
                 var existingPayment = await _repository.LevyPaymentRepository
                     .FindByCondition(p =>
                         p.TraderId == createDto.TraderId &&
                         p.PaymentDate.Date == DateTime.UtcNow.Date, false)
                     .AnyAsync();

                 if (existingPayment)
                 {
                     return ResponseFactory.Fail<LevyPaymentResponseDto>(
                         "Payment already processed for today");
                 }

                 var levyPayment = new LevyPayment
                 {
                     Id = Guid.NewGuid().ToString(),
                     TraderId = createDto.TraderId,
                     Amount = createDto.Amount,
                     PaymentDate = DateTime.UtcNow,
                     CollectedById = _currentUser.GetUserId(),
                     PaymentStatus = PaymentStatusEnum.Paid
                 };

                 _repository.LevyPaymentRepository.Create(levyPayment);

                 await CreateAuditLog(
                     "Levy Payment Created",
                     $"Created levy payment of {createDto.Amount} for Trader ID: {createDto.TraderId}"
                 );

                 await _repository.SaveChangesAsync();

                 var responseDto = _mapper.Map<LevyPaymentResponseDto>(levyPayment);
                 return ResponseFactory.Success(responseDto, "Levy payment created successfully");
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error creating levy payment");
                 return ResponseFactory.Fail<LevyPaymentResponseDto>(ex);
             }
         }
 */
        /*  public async Task<BaseResponse<LevyPaymentResponseDto>> UpdateLevyPayment(string paymentId, UpdateLevyPaymentDto updateDto)
          {
              try
              {
                  var levyPayment = await _repository.LevyPaymentRepository
                      .FindByCondition(p => p.Id == paymentId, true)
                      .FirstOrDefaultAsync();

                  if (levyPayment == null)
                  {
                      return ResponseFactory.Fail<LevyPaymentResponseDto>(
                          new NotFoundException("Levy payment not found"));
                  }

                  levyPayment.Amount = updateDto.Amount;
                  levyPayment.PaymentStatus = updateDto.Status;

                  _repository.LevyPaymentRepository.Update(levyPayment);

                  await CreateAuditLog(
                      "Levy Payment Updated",
                      $"Updated levy payment ID: {paymentId} with amount: {updateDto.Amount}"
                  );

                  await _repository.SaveChangesAsync();

                  var responseDto = _mapper.Map<LevyPaymentResponseDto>(levyPayment);
                  return ResponseFactory.Success(responseDto, "Levy payment updated successfully");
              }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "Error updating levy payment");
                  return ResponseFactory.Fail<LevyPaymentResponseDto>(ex);
              }
          }

          public async Task<BaseResponse<PaginatorDto<IEnumerable<LevyPaymentResponseDto>>>> GetLevyPayments(
              string marketId,
              LevyPaymentFilterDto filterDto,
              PaginationFilter paginationFilter)
          {
              try
              {
                  var query = _repository.LevyPaymentRepository.FindAll(false)
                      .Include(p => p.Trader)
                          .ThenInclude(t => t.User)
                      .Include(p => p.CollectedBy)
                      .Where(p => p.Trader.MarketId == marketId);

                  // Apply filters
                  if (filterDto.FromDate.HasValue)
                  {
                      query = query.Where(p => p.PaymentDate.Date >= filterDto.FromDate.Value.Date);
                  }

                  if (filterDto.ToDate.HasValue)
                  {
                      query = query.Where(p => p.PaymentDate.Date <= filterDto.ToDate.Value.Date);
                  }

                  if (filterDto.Status.HasValue)
                  {
                      query = query.Where(p => p.PaymentStatus == filterDto.Status.Value);
                  }

                  // Apply pagination
                  var paginatedPayments = await query
                      .OrderByDescending(p => p.PaymentDate)
                      .Paginate(paginationFilter);

                  var paymentDtos = paginatedPayments.PageItems
                      .Select(p => new LevyPaymentResponseDto
                      {
                          Id = p.Id,
                          Amount = p.Amount,
                          TraderName = $"{p.Trader.User.FirstName} {p.Trader.User.LastName}",
                          PaymentDate = p.PaymentDate,
                          Status = p.PaymentStatus,
                          CollectedBy = p.FirstName + " " + p.CollectedBy.LastName
                      });

                  var result = new PaginatorDto<IEnumerable<LevyPaymentResponseDto>>
                  {
                      PageItems = paymentDtos,
                      PageSize = paginatedPayments.PageSize,
                      CurrentPage = paginatedPayments.CurrentPage,
                      NumberOfPages = paginatedPayments.NumberOfPages
                  };

                  return ResponseFactory.Success(result);
              }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "Error retrieving levy payments");
                  return ResponseFactory.Fail<PaginatorDto<IEnumerable<LevyPaymentResponseDto>>>(ex);
              }
          }

          public async Task<BaseResponse<LevyPaymentDetailsDto>> GetLevyPaymentDetails(string paymentId)
          {
              try
              {
                  var payment = await _repository.LevyPaymentRepository
                      .FindByCondition(p => p.Id == paymentId, false)
                      .Include(p => p.Trader)
                          .ThenInclude(t => t.User)
                      .Include(p => p.Trader.Market)
                      .Include(p => p.CollectedBy)
                      .FirstOrDefaultAsync();

                  if (payment == null)
                  {
                      return ResponseFactory.Fail<LevyPaymentDetailsDto>(
                          new NotFoundException("Levy payment not found"));
                  }

                  var paymentDetails = new LevyPaymentDetailsDto
                  {
                      Id = payment.Id,
                      TraderName = $"{payment.Trader.User.FirstName} {payment.Trader.User.LastName}",
                      Market = payment.Trader.Market.Name,
                      Amount = payment.Amount,
                      PaymentDate = payment.PaymentDate,
                      CollectedBy = $"{payment.CollectedBy.FirstName} {payment.CollectedBy.LastName}",
                      Status = payment.PaymentStatus
                  };

                  return ResponseFactory.Success(paymentDetails);
              }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "Error retrieving levy payment details");
                  return ResponseFactory.Fail<LevyPaymentDetailsDto>(ex);
              }
          }

          public async Task<BaseResponse<bool>> DeleteLevyPayment(string paymentId)
          {
              try
              {
                  var payment = await _repository.LevyPaymentRepository
                      .FindByCondition(p => p.Id == paymentId, true)
                      .FirstOrDefaultAsync();

                  if (payment == null)
                  {
                      return ResponseFactory.Fail<bool>(
                          new NotFoundException("Levy payment not found"));
                  }

                  _repository.LevyPaymentRepository.Delete(payment);

                  await CreateAuditLog(
                      "Levy Payment Deleted",
                      $"Deleted levy payment ID: {paymentId}"
                  );

                  await _repository.SaveChangesAsync();
                  return ResponseFactory.Success(true, "Levy payment deleted successfully");
              }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "Error deleting levy payment");
                  return ResponseFactory.Fail<bool>(ex);
              }
          }
      }*/
    }
}

