using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using SabiMarket.Application.DTOs.Requests;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace SabiMarket.Infrastructure.Helpers
{
    public static class ExcelExportHelper
    {
        static ExcelExportHelper()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public static async Task<byte[]> GenerateMarketReport(ReportExportDto reportData)
        {
            using var package = new ExcelPackage();

            var summarySheet = CreateSummarySheet(package, reportData);
            var paymentSheet = CreatePaymentAnalysisSheet(package, reportData);

            // Auto-fit columns in both sheets
            summarySheet.Cells[summarySheet.Dimension.Address].AutoFitColumns();
            paymentSheet.Cells[paymentSheet.Dimension.Address].AutoFitColumns();

            return await package.GetAsByteArrayAsync();
        }

        private static ExcelWorksheet CreateSummarySheet(ExcelPackage package, ReportExportDto reportData)
        {
            var summarySheet = package.Workbook.Worksheets.Add("Summary");

            // Add title and date range
            summarySheet.Cells[1, 1].Value = "Market Performance Report";
            summarySheet.Cells[2, 1].Value = $"Period: {reportData.StartDate:d MMM yyyy} - {reportData.EndDate:d MMM yyyy}";

            // Style the header
            StyleHeader(summarySheet.Cells[1, 1, 1, 6]);

            // Add Overview Section
            int currentRow = 4;
            AddSectionHeader(summarySheet, currentRow, "Overview Metrics");

            currentRow += 2;
            AddMetricRows(summarySheet, ref currentRow, reportData);

            // Add Market Details Table
            currentRow += 2;
            AddSectionHeader(summarySheet, currentRow, "Market Performance Details");
            currentRow += 2;

            AddMarketDetailsTable(summarySheet, ref currentRow, reportData);

            return summarySheet;
        }

        private static void StyleHeader(ExcelRange headerRange)
        {
            headerRange.Merge = true;
            headerRange.Style.Font.Size = 16;
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(48, 84, 150));
            headerRange.Style.Font.Color.SetColor(Color.White);
        }

        private static void AddSectionHeader(ExcelWorksheet sheet, int row, string headerText)
        {
            sheet.Cells[row, 1].Value = headerText;
            using var range = sheet.Cells[row, 1, row, 6];
            range.Merge = true;
            range.Style.Font.Bold = true;
            range.Style.Font.Size = 12;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 225, 242));
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        }

        private static void AddMetricRows(ExcelWorksheet sheet, ref int currentRow, ReportExportDto reportData)
        {
            AddMetricRow(sheet, currentRow++, "Total Markets", reportData.TotalMarkets);
            AddMetricRow(sheet, currentRow++, "Total Revenue", reportData.TotalRevenue.ToString("C"));
            AddMetricRow(sheet, currentRow++, "Total Traders", reportData.TotalTraders);
            AddMetricRow(sheet, currentRow++, "Trader Compliance Rate", $"{reportData.TraderComplianceRate:F1}%");
            AddMetricRow(sheet, currentRow++, "Total Transactions", reportData.TotalTransactions);
            AddMetricRow(sheet, currentRow++, "Daily Average Revenue", reportData.DailyAverageRevenue.ToString("C"));
        }

        private static void AddMetricRow(ExcelWorksheet sheet, int row, string label, object value)
        {
            sheet.Cells[row, 1].Value = label;
            sheet.Cells[row, 2].Value = value;

            sheet.Cells[row, 1].Style.Font.Bold = true;

            if (value is int or decimal or double)
            {
                sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }
        }

        private static void AddMarketDetailsTable(ExcelWorksheet sheet, ref int currentRow, ReportExportDto reportData)
        {
            string[] headers = { "Market Name", "Location", "Traders", "Revenue", "Compliance Rate", "Transactions" };
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[currentRow, i + 1].Value = headers[i];
                FormatTableHeader(sheet.Cells[currentRow, i + 1]);
            }

            currentRow++;
            AddMarketDetails(sheet, ref currentRow, reportData.MarketDetails);
        }

        private static void AddMarketDetails(ExcelWorksheet sheet, ref int currentRow, List<ReportExportDto.MarketSummary> marketDetails)
        {
            foreach (var market in marketDetails)
            {
                sheet.Cells[currentRow, 1].Value = market.MarketName;
                sheet.Cells[currentRow, 2].Value = market.Location;
                sheet.Cells[currentRow, 3].Value = market.TotalTraders;
                sheet.Cells[currentRow, 4].Value = market.Revenue;
                sheet.Cells[currentRow, 5].Value = $"{market.ComplianceRate:F1}%";
                sheet.Cells[currentRow, 6].Value = market.TransactionCount;

                sheet.Cells[currentRow, 4].Style.Numberformat.Format = "#,##0.00";
                currentRow++;
            }
        }

        private static void FormatTableHeader(ExcelRange cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(189, 215, 238));
            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        private static ExcelWorksheet CreatePaymentAnalysisSheet(ExcelPackage package, ReportExportDto reportData)
        {
            var sheet = package.Workbook.Worksheets.Add("Payment Analysis");

            sheet.Cells[1, 1].Value = "Payment Methods Analysis";
            sheet.Cells[2, 1].Value = $"Period: {reportData.StartDate:d MMM yyyy} - {reportData.EndDate:d MMM yyyy}";

            using (var headerRange = sheet.Cells[1, 1, 1, 3])
            {
                headerRange.Merge = true;
                headerRange.Style.Font.Size = 14;
                headerRange.Style.Font.Bold = true;
            }

            AddPaymentMethodsTable(sheet, reportData);
            return sheet;
        }

        private static void AddPaymentMethodsTable(ExcelWorksheet sheet, ReportExportDto reportData)
        {
            int currentRow = 4;

            // Add headers
            sheet.Cells[currentRow, 1].Value = "Payment Method";
            sheet.Cells[currentRow, 2].Value = "Amount";
            sheet.Cells[currentRow, 3].Value = "Percentage";

            FormatTableHeader(sheet.Cells[currentRow, 1]);
            FormatTableHeader(sheet.Cells[currentRow, 2]);
            FormatTableHeader(sheet.Cells[currentRow, 3]);

            currentRow++;

            decimal total = reportData.RevenueByPaymentMethod.Values.Sum();
            foreach (var method in reportData.RevenueByPaymentMethod)
            {
                sheet.Cells[currentRow, 1].Value = method.Key;
                sheet.Cells[currentRow, 2].Value = method.Value;
                sheet.Cells[currentRow, 3].Value = total > 0 ? (method.Value / total) * 100 : 0;

                sheet.Cells[currentRow, 2].Style.Numberformat.Format = "#,##0.00";
                sheet.Cells[currentRow, 3].Style.Numberformat.Format = "0.0%";

                currentRow++;
            }
        }
    }
}