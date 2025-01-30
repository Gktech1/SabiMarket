using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.DTOs.Requests
{
    public class TraderDto
    {
        public string Id { get; set; }
        public string TraderName { get; set; }
        public string Market { get; set; }
    }

    public class CreateTraderDto
    {
        public string TraderName { get; set; }
        public string PhoneNumber { get; set; }
        public string Occupancy { get; set; }  // Changed from BusinessType to match UI
        public string MarketId { get; set; }
        public GenderEnum Gender { get; set; }
        public PaymentFrequencyEnum PaymentFrequency { get; set; }
        public string PhotoUrl { get; set; }
    }

    public class TraderDetailsDto
    {
        public string TraderName { get; set; }
        public string PhoneNumber { get; set; }
        public GenderEnum Gender { get; set; }
        public string Occupancy { get; set; }  // Changed from BusinessType
        public string Market { get; set; }
        public decimal PaymentAmount { get; set; }  
        public DateTime DateAdded { get; set; }
        public PaymentFrequencyEnum PaymentFrequency { get; set; }  // Format: "2 days - N500"
        public string TraderIdentityNumber { get; set; }  // Format: OSH/LAG/23401
        public string PhotoUrl { get; set; }
        public string QRCode { get; set; }
        public decimal OutstandingDebt { get; set; }
    }
    public class UpdateTraderDto
    {
        public string TraderName { get; set; }
        public string PhoneNumber { get; set; }
        public string Occupancy { get; set; }
        public string MarketId { get; set; }
        public GenderEnum Gender { get; set; }
        public PaymentFrequencyEnum PaymentFrequency { get; set; }
        public string PhotoUrl { get; set; }
    }

    public class TraderFilterDto
    {
        public string SearchTerm { get; set; }
        public string MarketId { get; set; }
        public string BusinessType { get; set; }
        public string Status { get; set; }
    }
}
