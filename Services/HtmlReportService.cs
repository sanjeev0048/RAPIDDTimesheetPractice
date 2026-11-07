using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using RAPIDDTimesheetPractice.Models;

namespace RAPIDDTimesheetPractice.Services;

/// <summary>
/// Service responsible for composing and exporting an HTML report.
/// Demonstrates efficient string concatenation via StringBuilder and conditional CSS styling.
/// </summary>
public class HtmlReportService
{
    /// <summary>
    /// Generates the HTML string from employee summaries.
    /// Employees below 100 hours are highlighted with warning styling.
    /// </summary>
    public string BuildHtmlReport(IEnumerable<EmployeeSummary> summaries, double thresholdHours = 100.0)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"UTF-8\">");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine("    <title>RAPIDD Employee Timesheet Report</title>");
        sb.AppendLine("    <style>");
        sb.AppendLine("        :root {");
        sb.AppendLine("            --primary: #2563eb;");
        sb.AppendLine("            --bg: #f8fafc;");
        sb.AppendLine("            --card-bg: #ffffff;");
        sb.AppendLine("            --text: #1e293b;");
        sb.AppendLine("            --border: #e2e8f0;");
        sb.AppendLine("            --warning-bg: #fee2e2;");
        sb.AppendLine("            --warning-text: #991b1b;");
        sb.AppendLine("            --warning-border: #fca5a5;");
        sb.AppendLine("        }");
        sb.AppendLine("        * { box-sizing: border-box; }");
        sb.AppendLine("        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: var(--bg); color: var(--text); margin: 0; padding: 2rem; line-height: 1.5; }");
        sb.AppendLine("        .container { max-width: 1000px; margin: 0 auto; background: var(--card-bg); border-radius: 12px; padding: 2.5rem; box-shadow: 0 10px 15px -3px rgba(0,0,0,0.07); border: 1px solid var(--border); }");
        sb.AppendLine("        header { border-bottom: 2px solid var(--border); padding-bottom: 1rem; margin-bottom: 1.5rem; display: flex; justify-content: space-between; align-items: baseline; flex-wrap: wrap; }");
        sb.AppendLine("        h1 { margin: 0; color: #0f172a; font-size: 1.75rem; font-weight: 700; }");
        sb.AppendLine("        .timestamp { color: #64748b; font-size: 0.875rem; }");
        sb.AppendLine("        .stats { display: flex; gap: 1.5rem; margin-bottom: 1.5rem; flex-wrap: wrap; }");
        sb.AppendLine("        .stat-card { background: #f1f5f9; padding: 1rem 1.25rem; border-radius: 8px; flex: 1; min-width: 180px; }");
        sb.AppendLine("        .stat-label { font-size: 0.75rem; text-transform: uppercase; color: #64748b; font-weight: 600; }");
        sb.AppendLine("        .stat-value { font-size: 1.5rem; font-weight: 700; color: #0f172a; margin-top: 0.25rem; }");
        sb.AppendLine("        table { width: 100%; border-collapse: collapse; margin-top: 1rem; }");
        sb.AppendLine("        th, td { padding: 12px 16px; text-align: left; border-bottom: 1px solid var(--border); }");
        sb.AppendLine("        th { background: #f8fafc; font-weight: 600; color: #475569; font-size: 0.85rem; text-transform: uppercase; letter-spacing: 0.05em; }");
        sb.AppendLine("        tr:hover { background-color: #f1f5f9; }");
        sb.AppendLine("        /* Highlight row for employees below 100 hours */");
        sb.AppendLine("        tr.highlight-low-hours { background-color: var(--warning-bg) !important; color: var(--warning-text); font-weight: 600; }");
        sb.AppendLine("        tr.highlight-low-hours:hover { background-color: #fecaca !important; }");
        sb.AppendLine("        .badge { display: inline-block; padding: 3px 8px; border-radius: 9999px; font-size: 0.75rem; font-weight: 600; }");
        sb.AppendLine("        .badge-warning { background-color: #fecaca; color: #b91c1c; border: 1px solid var(--warning-border); }");
        sb.AppendLine("        .badge-success { background-color: #dcfce7; color: #166534; }");
        sb.AppendLine("        .legend { margin-top: 1.5rem; padding: 0.75rem 1rem; background-color: #f8fafc; border-radius: 6px; border: 1px solid var(--border); font-size: 0.875rem; color: #475569; display: flex; align-items: center; gap: 0.75rem; }");
        sb.AppendLine("        .legend-indicator { width: 16px; height: 16px; background-color: var(--warning-bg); border: 1px solid var(--warning-border); border-radius: 4px; display: inline-block; }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("    <div class=\"container\">");
        sb.AppendLine("        <header>");
        sb.AppendLine("            <h1>RAPIDD Employee Timesheet Summary</h1>");
        sb.AppendLine($"            <div class=\"timestamp\">Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</div>");
        sb.AppendLine("        </header>");

        var summaryList = summaries.ToList();
        double totalHoursAll = summaryList.Sum(s => s.TotalHours);
        int lowCount = summaryList.Count(s => s.TotalHours < thresholdHours);

        sb.AppendLine("        <div class=\"stats\">");
        sb.AppendLine($"            <div class=\"stat-card\"><div class=\"stat-label\">Total Employees</div><div class=\"stat-value\">{summaryList.Count}</div></div>");
        sb.AppendLine($"            <div class=\"stat-card\"><div class=\"stat-label\">Total Hours Logged</div><div class=\"stat-value\">{totalHoursAll:F2} hrs</div></div>");
        sb.AppendLine($"            <div class=\"stat-card\"><div class=\"stat-label\">Below {thresholdHours:F0} Hours</div><div class=\"stat-value\" style=\"color: #b91c1c;\">{lowCount}</div></div>");
        sb.AppendLine("        </div>");

        sb.AppendLine("        <table>");
        sb.AppendLine("            <thead>");
        sb.AppendLine("                <tr>");
        sb.AppendLine("                    <th>Rank</th>");
        sb.AppendLine("                    <th>Employee Name</th>");
        sb.AppendLine("                    <th>Total Time Worked</th>");
        sb.AppendLine("                    <th>Share of Total</th>");
        sb.AppendLine("                    <th>Status</th>");
        sb.AppendLine("                </tr>");
        sb.AppendLine("            </thead>");
        sb.AppendLine("            <tbody>");

        int rank = 1;
        foreach (var emp in summaryList)
        {
            bool isLow = emp.TotalHours < thresholdHours;
            string rowClass = isLow ? " class=\"highlight-low-hours\"" : "";

            sb.AppendLine($"                <tr{rowClass}>");
            sb.AppendLine($"                    <td>{rank++}</td>");
            sb.AppendLine($"                    <td>{WebUtility.HtmlEncode(emp.EmployeeName)}</td>");
            sb.AppendLine($"                    <td>{emp.TotalHours:F2} hrs</td>");
            sb.AppendLine($"                    <td>{emp.PercentageOfTotal:F2}%</td>");

            if (isLow)
            {
                sb.AppendLine($"                    <td><span class=\"badge badge-warning\">Below {thresholdHours:F0} hrs</span></td>");
            }
            else
            {
                sb.AppendLine("                    <td><span class=\"badge badge-success\">Satisfactory</span></td>");
            }

            sb.AppendLine("                </tr>");
        }

        sb.AppendLine("            </tbody>");
        sb.AppendLine("        </table>");

        sb.AppendLine("        <div class=\"legend\">");
        sb.AppendLine("            <span class=\"legend-indicator\"></span>");
        sb.AppendLine($"            <span>Employees highlighted with reddish background worked fewer than {thresholdHours:F0} hours.</span>");
        sb.AppendLine("        </div>");
        sb.AppendLine("    </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    /// <summary>
    /// Writes the generated HTML report to the specified file path.
    /// Creates directory if it does not exist.
    /// </summary>
    public void SaveHtmlReport(string htmlContent, string outputFilePath)
    {
        var directory = Path.GetDirectoryName(outputFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(outputFilePath, htmlContent);
    }
}
