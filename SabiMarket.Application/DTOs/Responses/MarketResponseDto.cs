namespace SabiMarket.Application.DTOs.Responses
{
    public class MarketResponseDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int TotalTraders { get; set; }
        public int Capacity { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public IEnumerable<string> CaretakerIds { get; set; }
    }
}
