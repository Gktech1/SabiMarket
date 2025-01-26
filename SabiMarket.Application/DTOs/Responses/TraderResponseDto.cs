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
        public string TraderName { get; set; }
        public string TraderOccupancy { get; set; }
        public string TraderIdentityNumber { get; set; }
        public string PaymentFrequency { get; set; }
        public string LastPaymentDate { get; set; }
        public string UpdatePaymentUrl { get; set; }
    }
}

