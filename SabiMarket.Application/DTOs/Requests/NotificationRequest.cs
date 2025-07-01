using SabiMarket.Domain.Enum;

namespace SabiMarket.API.Models.Notifications
{
    public class NotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Token { get; set; }
        public List<string>? Tokens { get; set; }
        public string? Topic { get; set; }
        public Dictionary<string, string>? Data { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class NotificationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? MessageId { get; set; }
        public List<string>? FailedTokens { get; set; }
    }

    public class UserNotification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? DataJson { get; set; } // Store as JSON string
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public NotificationType Type { get; set; }

        // Navigation properties
        public string? RelatedEntityId { get; set; } // For orders, products, etc.
        public string? ActionUrl { get; set; } // Deep link URL
    }

    public class DeviceToken
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty; // iOS, Android, Web
        public string? DeviceInfo { get; set; } // Browser, OS version, etc.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }

   // Request DTOs for API endpoints
    public class RegisterTokenRequest
    {
        public string Token { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string? DeviceInfo { get; set; }
    }

    public class RemoveTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }

    public class SendNotificationToUserRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public Dictionary<string, string>? Data { get; set; }
        public NotificationType Type { get; set; } = NotificationType.General;
        public string? ActionUrl { get; set; }
    }

    public class SendBulkNotificationRequest
    {
        public List<string> UserIds { get; set; } = new();
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public Dictionary<string, string>? Data { get; set; }
        public NotificationType Type { get; set; } = NotificationType.General;
    }
}