using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.DTOs.Requests
{
    public class CreateGoodBoyRequestDto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string CaretakerId { get; set; }
        public string MarketId { get; set; }
    }

    public class UpdateGoodBoyProfileDto
    {
        public string? FullName { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

        public string? Gender { get; set; }

        public string? Address { get; set; }

        public string? ProfileImage { get; set; }

        public bool? IsAvailableForDuty { get; set; }

    }

    public class GoodBoyFilterRequestDto
    {
        public string MarketId { get; set; }
        public string CaretakerId { get; set; }
        public StatusEnum? Status { get; set; }
    }

    public class ProcessLevyPaymentDto
    {
        public decimal Amount { get; set; }
    }
}
