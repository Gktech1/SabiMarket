using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs.PaymentsDto;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Enum;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Services.Payment
{
    public class Payments : IPayments
    {
        private readonly IPaymentService _paymentService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ApplicationDbContext _applicationDbContext;
        public Payments(IPaymentService paymentService, ApplicationDbContext applicationDbContext, IHttpContextAccessor contextAccessor = null)
        {
            _paymentService = paymentService;
            _applicationDbContext = applicationDbContext;
            _contextAccessor = contextAccessor;
        }
        public async Task<BaseResponse<string>> Initialize(FundWalletVM model)
        {
            var response = await _paymentService.InitializePayment(model);

            if (!response.Item1) return ResponseFactory.Fail<string>(new Exception("Failure to initialize payment."), response.Item2);

            //var walletId = (await (await _repository.GetAsync<Wallet>()).FirstAsync(w => w.UserId == userId)).Id;

            var transaction = new Transaction
            {
                Amount = model.Amount,
                SenderId = model.UserId,
                Reference = response.Item3,
                Status = false,
                Description = model.Description,
                TransactionType = TransactionTypes.Funding.ToString(),
                IsActive = true,
            };

            await _applicationDbContext.Transactions.AddAsync(transaction);
            _applicationDbContext.SaveChanges();
            //var url = response.Item2;
            return ResponseFactory.Success(response.Item3, response.Item2); ;
        }
        public async Task<BaseResponse<IEnumerable<Bank>>> GetAllBanks()
        {
            var response = await _paymentService.GetListOfBanks();

            //if (response is null || response.Count() < 1) return ResponseFactory.Fail<Bank>(new Exception("Unable to get bank list. Try again later."), null);

            return ResponseFactory.Success(response);
        }

        public async Task<BaseResponse<bool>> Verify(string reference)
        {
            var loggedInUser = Helper.GetUserDetails(_contextAccessor);

            var getVendor = await _applicationDbContext.Vendors.Where(x => x.UserId == loggedInUser.Id).FirstOrDefaultAsync();
            if (getVendor is null)
            {
                return ResponseFactory.Fail<bool>("Vendor does not exist");

            }
            var isSuccessful = await _paymentService.Verify(reference);

            if (!isSuccessful) return ResponseFactory.Fail<bool>(new Exception("Failure to verify payment."));

            var transaction = await _applicationDbContext.Transactions.FirstAsync(t => t.Reference == reference);

            transaction.Status = true;

            getVendor.IsSubscriptionActive = true;
            await _applicationDbContext.SaveChangesAsync();

            return ResponseFactory.Success<bool>(true, "Success"); ;
        }

        public async Task<IEnumerable<Bank>> GetBanks()
        {
            return await _paymentService.GetListOfBanks();
        }
    }
}
