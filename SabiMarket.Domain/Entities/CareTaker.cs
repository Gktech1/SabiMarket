namespace SabiMarket.Domain.Entities
{
    public class CareTaker : Entity, IAuditable
    {
        public string UserId { get; set; }
        public User User { get; set; }
        public ICollection<Market> Markets { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

    }
}