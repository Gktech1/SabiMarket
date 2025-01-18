public class DashboardMetricsResponseDto
{
    public MetricWithChange Traders { get; set; }
    public MetricWithChange Caretakers { get; set; }
    public MetricWithChange Levies { get; set; }
    public string TimePeriod { get; set; }
}

public class MetricWithChange
{
    public int CurrentValue { get; set; }
    public decimal PercentageChange { get; set; }
    public string ChangeDirection { get; set; }  // "Up" or "Down"
    public int PreviousValue { get; set; }
}