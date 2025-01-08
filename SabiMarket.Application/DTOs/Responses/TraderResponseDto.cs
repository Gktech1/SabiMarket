namespace SabiMarket.Application.DTOs.Responses
{
    public class TraderResponseDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string BusinessName { get; set; }
        public string BusinessType { get; set; }
        public string TIN { get; set; }
        public string QRCode { get; set; }
        public ICollection<LevyPaymentResponseDto> LevyPayments { get; set; }
    }
}
