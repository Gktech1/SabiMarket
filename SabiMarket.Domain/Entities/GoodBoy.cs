namespace SabiMarket.Domain.Entities
{
    public class GoodBoy : Entity, IAuditable
    {
        public string UserId { get; set; }
        public string MarketId { get; set; }
        public User User { get; set; }
        public Market Market { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}