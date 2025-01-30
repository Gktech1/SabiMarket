namespace SabiMarket.Application.DTOs.Responses
{
    public class AssistCenterOfficerResponseDto
    {
        // Basic Information
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PhotoUrl { get; set; }

        // Assignment Information
        public string ChairmanId { get; set; }
        public string ChairmanName { get; set; }
        public string MarketId { get; set; }
        public string MarketName { get; set; }
        public string LocalGovernmentId { get; set; }
        public string LocalGovernmentName { get; set; }

        // Access Level Information
        public string UserLevel { get; set; }
        public bool IsBlocked { get; set; }

        // Audit Information
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        // Statistics
        public int TotalTradersManaged { get; set; }
        public decimal TotalLeviesCollected { get; set; }
        public decimal TodayLeviesCollected { get; set; }
    }
}
