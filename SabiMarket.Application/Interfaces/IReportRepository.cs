using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities;

public interface IReportRepository : IGeneralRepository<Report>
{
    Task<Report> GetDashboardSummary();
    Task<Report> GetDailyMetricsAsync(DateTime date);
    Task<Report> GetMetricsAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Report>> GetLevyPaymentsBreakdown(int year);
    Task<Report> GetMarketComplianceRates(string marketId);
    Task<IEnumerable<Report>> GetLevyCollectionPerMarket();
    Task<Report> ExportReport(DateTime startDate, DateTime endDate);
}