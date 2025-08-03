using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PayStack.Net;
using SabiMarket.Application.DTOs.PaymentsDto;
using SabiMarket.Infrastructure.Data;

namespace SabiMarket.Infrastructure.Services;

public class PayStackService : IPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly PayStackApi _payStack;
    ApplicationDbContext _context;
    IHttpContextAccessor _httpContextAccessor; // inject this

    public string Url { get; set; }

    public PayStackService(IConfiguration configuration, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _secretKey = _configuration["Payment:PayStackSecretKey"];
        _payStack = new PayStackApi(_secretKey);
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Tuple<bool, string, string>> InitializePayment(FundWalletVM model)
    {
        var senderEmail = await _context.Users.Where(x => x.Id == model.UserId)
            .Select(s => s.Email)
            .FirstOrDefaultAsync();

        var transactionRef = $"SabiMart_" + Guid.NewGuid();
        // Read callback URL from request header
        var headers = _httpContextAccessor.HttpContext?.Request?.Headers;
        var callbackBaseUrl = headers != null && headers.ContainsKey("PaystackCallBackUrl")
         ? headers["PaystackCallBackUrl"].ToString()
        : _configuration["Payment:PayStackCallbackUrl"]; // fallback if not in header
        var callbackUrl = callbackBaseUrl;// + _configuration["Payment:PayStackCallbackEndpoint"] ?? "";
        var request = new TransactionInitializeRequest
        {
            AmountInKobo = (int)model.Amount * 100,
            Email = senderEmail ?? "",
            Currency = "NGN",
            CallbackUrl = callbackUrl,
            Reference = transactionRef,
        };

        var response = _payStack.Transactions.Initialize(request);

        if (!response.Status) return new Tuple<bool, string, string>(false, response.Message, transactionRef);

        return new Tuple<bool, string, string>(true, response.Data.AuthorizationUrl, transactionRef);
    }

    public async Task<bool> Verify(string reference)
    {
        var verifyResponse = _payStack.Transactions.Verify(reference);

        return verifyResponse.Status;
    }

    public async Task<IEnumerable<SabiMarket.Application.DTOs.PaymentsDto.Bank>> GetListOfBanks()
    {
        var result = _payStack.Get<ApiResponse<dynamic>>("bank?currency=NGN");

        if (!result.Status)
            throw new Exception("Unable to fetch banks");

        var banks = result.Data.ToObject<List<SabiMarket.Application.DTOs.PaymentsDto.Bank>>();

        return banks;
    }


}