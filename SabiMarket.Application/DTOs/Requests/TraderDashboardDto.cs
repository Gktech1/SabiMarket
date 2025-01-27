using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Application.DTOs
{
    public class LevyPaymentDto
    {
        public string Id { get; set; }
        public string PaymentType { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentReference { get; set; }
        public string CollectedBy { get; set; }
    }

    public class TraderDashboardDto
    {
        public DateTime NextPaymentDate { get; set; }
        public decimal TotalLeviesPaid { get; set; }
        public List<PaymentHistoryDto> PaymentHistory { get; set; }
        public PaymentScheduleDto PaymentSchedule { get; set; }
        public NotificationsDto Notifications { get; set; }
    }

    public class PaymentHistoryDto
    {
        public string PaymentType { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
    }

    public class PaymentScheduleDto
    {
        public string Frequency { get; set; }
        public decimal Amount { get; set; }
        public DateTime NextDueDate { get; set; }
    }

    public class NotificationsDto
    {
        public List<NotificationItemDto> Items { get; set; }
        public bool HasUnread { get; set; }
    }

    public class NotificationItemDto
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; }
    }
}