namespace SabiMarket.Application.DTOs.Responses
{
    public class TraderResponseDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string MarketId { get; set; }
        public string MarketName { get; set; }
        public string Gender { get; set; }
        public string IdentityNumber { get; set; }
        public DateTime DateAdded { get; set; }
        public string QRCode { get; set; }
        public bool IsActive { get; set; }
    }

    public class TraderDetailsDto : TraderResponseDto
    {
        public string Address { get; set; }
        public string BusinessType { get; set; }
        public decimal TotalLeviesPaid { get; set; }
        public DateTime LastLevyPayment { get; set; }
        public string PaymentStatus { get; set; }
        public ICollection<LevyResponseDto> RecentPayments { get; set; }
    }
}

