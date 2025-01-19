using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SabiMarket.Domain.Entities
{
    [Table("AuditLogs")]
    public class AuditLog : BaseEntity
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Time { get; set; }

        [Required]
        public string User { get; set; }

        [Required]
        public string Activity { get; set; }

        // Additional metadata
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? PageAccessed { get; set; }
    }
}