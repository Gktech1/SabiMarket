using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Requests
{
    public class ReportExportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Market Statistics
        public int TotalMarkets { get; set; }
        public List<MarketSummary> MarketDetails { get; set; }

        // Financial Metrics
        public decimal TotalRevenue { get; set; }
        public decimal DailyAverageRevenue { get; set; }
        public int TotalTransactions { get; set; }

        // Trader Metrics
        public int TotalTraders { get; set; }
        public int ActiveTraders { get; set; }
        public decimal TraderComplianceRate { get; set; }

        // Caretaker Metrics
        public int TotalCaretakers { get; set; }
        public int ActiveCaretakers { get; set; }

        // Payment Statistics
        public Dictionary<string, decimal> RevenueByPaymentMethod { get; set; }
        public Dictionary<string, int> TransactionsByMarket { get; set; }

        public class MarketSummary
        {
            public string MarketName { get; set; }
            public string Location { get; set; }
            public int TotalTraders { get; set; }
            public decimal Revenue { get; set; }
            public decimal ComplianceRate { get; set; }
            public int TransactionCount { get; set; }
        }
    }
}
