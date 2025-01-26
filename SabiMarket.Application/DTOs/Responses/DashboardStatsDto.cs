public class DashboardStatsDto
{
    public int TotalTraders { get; set; }
    public decimal TotalLevies { get; set; }
    public int PendingPayments { get; set; }
    public decimal MonthlyCollection { get; set; }
    public decimal DailyCollection { get; set; }
    public int ActiveGoodBoys { get; set; }
    public ICollection<RecentActivityDto> RecentActivities { get; set; }
    public ICollection<MarketPerformanceDto> MarketPerformance { get; set; }
}

public class RecentActivityDto
{
    public string ActivityType { get; set; }
    public string Description { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; }
    public decimal? Amount { get; set; }
}

public class MarketPerformanceDto
{
    public string MarketId { get; set; }
    public string MarketName { get; set; }
    public decimal TotalCollection { get; set; }
    public int TotalTraders { get; set; }
    public decimal CollectionRate { get; set; }
}