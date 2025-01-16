using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Responses
{
    namespace SabiMarket.Application.DTOs.Responses
    {
        public class DashboardMetricsResponseDto
        {
            /// <summary>
            /// Total number of traders in the system.
            /// </summary>
            public int TotalTraders { get; set; }

            /// <summary>
            /// Total number of caretakers in the system.
            /// </summary>
            public int TotalCaretakers { get; set; }

            /// <summary>
            /// Total levies collected.
            /// </summary>
            public decimal TotalLeviesCollected { get; set; }

            /// <summary>
            /// Any additional information that can be displayed on the dashboard.
            /// </summary>
            public string? AdditionalInfo { get; set; }
        }
    }

}
