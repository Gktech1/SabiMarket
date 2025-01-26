namespace SabiMarket.Infrastructure.Configuration
{
    public class PaymentConfigOptions
    {
        public MarketConfig Market { get; set; }
    }

    public class MarketConfig
    {
        public int DefaultFrequencyInDays { get; set; }
        public decimal DefaultAmount { get; set; }
        public List<MarketSpecificConfig> MarketSpecific { get; set; }
    }

    public class MarketSpecificConfig
    {
        public string MarketId { get; set; }
        public int FrequencyInDays { get; set; }
        public decimal Amount { get; set; }
        public List<BusinessTypeConfig> BusinessTypes { get; set; }
    }

    public class BusinessTypeConfig
    {
        public string Type { get; set; }
        public int FrequencyInDays { get; set; }
        public decimal Amount { get; set; }
    }
}