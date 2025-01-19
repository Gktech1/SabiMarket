using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Application.DTOs.Requests
{
    public class LevySetupRequestDto
    {
        [Required]
        public string TraderOccupancy { get; set; }  // e.g., Shop, Kiosk, Open Space

        [Required]
        public int PaymentFrequencyDays { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }
    public class AuditLogDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }
    }

    public class LevySetupResponseDto
    {
        public string Id { get; set; }
        public string TraderOccupancy { get; set; }
        public int PaymentFrequencyDays { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}