namespace SabiMarket.Domain.Entities
{
    public class Trader : Entity, IAuditable
    {
        public string IdentityNumber { get; set; }
        public string UserId { get; set; }
        public string ShopId { get; set; }
        public string AddressId { get; set; }
        public User User { get; set; }
        public Shop Shop { get; set; }
        public Address Address { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}