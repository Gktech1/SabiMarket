namespace SabiMarket.Application.DTOs.Responses
{
    public class ChairmanResponseDto
    {
        public string Id { get; set; }  = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string MarketId { get; set; } = string.Empty;
        public string MarketName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string DefaultPassword { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }  
        public DateTime? UpdatedAt { get; set; }  
    }
}
