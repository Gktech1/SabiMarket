namespace SabiMarket.Domain.Entities
{
    public class ChairMan : Entity, IAuditable
    {
        public string UserId { get; set; }
        public string LGAId { get; set; }
        public LGA LGA { get; set; }
        public User User { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}