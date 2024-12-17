
namespace SabiMarket.Domain.Entities
{
    public class LGA : Entity, IAuditable
    {
        public string Name { get; set; }
        public ChairMan ChairMan { get; set; }
        public ICollection<Market> Markets { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}