namespace SabiMarket.Domain.Entities
{
    public class Shop : Entity, IAuditable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string TraderId { get; set; }
        public Trader Trader { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}