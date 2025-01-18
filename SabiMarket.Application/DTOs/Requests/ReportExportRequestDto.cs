using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Requests
{
    public class ReportExportRequestDto
    {
        // Date Range Properties
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        // Market Selection
        public string MarketId { get; set; } // Optional - if null means all markets

        // Report Type Selection
        [Required(ErrorMessage = "Report type is required")]
        public ReportType ReportType { get; set; }

        // Export Format
        [Required(ErrorMessage = "Export format is required")]
        public ExportFormat ExportFormat { get; set; }

        // Optional filtering parameters
        public bool IncludeComplianceRates { get; set; } = true;
        public bool IncludeRevenueBreakdown { get; set; } = true;
        public bool IncludeMarketMetrics { get; set; } = true;
        public string TimeZone { get; set; } = "UTC";
    }

    public enum ReportType
    {
        MarketCount,
        TotalRevenue,
        LevyBreakdown,
        ComplianceRates,
        MarketRevenue,
        Comprehensive // Includes all report types
    }

    public enum ExportFormat
    {
        PDF,
        Excel,
        CSV
    }
}
