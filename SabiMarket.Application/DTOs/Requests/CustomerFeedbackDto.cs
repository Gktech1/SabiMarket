namespace SabiMarket.Infrastructure.Services
{
    public class CustomerFeedbackDto
    {
        public string Id { get; set; }
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public string CustomerName { get; set; }
        public bool IsResolved { get; set; }

        public DateTime CreatedAt { get; set; }
    }

}