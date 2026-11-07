using System;
using System.IO;
using System.Threading.Tasks;
using RAPIDDTimesheetPractice.Services;

namespace RAPIDDTimesheetPractice;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("================================================================");
        Console.WriteLine(" RAPIDD Technologies - Employee Timesheet Practice System");
        Console.WriteLine(" .NET 9.0 | ScottPlot 5.1.59 | Synthetic Data Demonstration");
        Console.WriteLine("================================================================\n");

        // Step 1: Initialize Services
        var dataService = new DataService();
        var calcService = new CalculationService();
        var htmlService = new HtmlReportService();
        var chartService = new ChartService();
        var testingService = new TestingService();

        // Define output directory and file paths
        string projectRoot = AppContext.BaseDirectory;
        // In local runs, place Output inside project root or execution directory
        string outputDir = Path.Combine(projectRoot, "Output");
        Directory.CreateDirectory(outputDir);

        string htmlPath = Path.Combine(outputDir, "index.html");
        string pngPath = Path.Combine(outputDir, "employee-hours.png");

        // Step 2: Retrieve Time Entries (Demonstrates HTTP GET / Fallback Synthetic Data)
        Console.WriteLine("--> [Step 1] Retrieving Timesheet Records...");
        // Demonstrating GET request with graceful fallback to synthetic test data
        var rawEntries = await dataService.FetchEntriesFromApiAsync("https://example.com/api/test-timeentries");
        Console.WriteLine($"    Total raw records loaded: {rawEntries.Count}");

        // Step 3: Business Logic (Data Cleansing, LINQ Grouping & Hours Calculation)
        Console.WriteLine("\n--> [Step 2] Processing Employee Working Hours...");
        var employeeSummaries = calcService.CalculateEmployeeSummaries(rawEntries);

        Console.WriteLine($"    Valid grouped employees: {employeeSummaries.Count}");
        Console.WriteLine("    ------------------------------------------------------------");
        Console.WriteLine("    Rank | Employee Name       | Total Hours | Share | Status");
        Console.WriteLine("    ------------------------------------------------------------");

        int rank = 1;
        foreach (var emp in employeeSummaries)
        {
            string status = emp.IsBelowThreshold ? "< 100 hrs (WARN)" : "OK";
            Console.WriteLine($"     {rank,2}  | {emp.EmployeeName,-20} | {emp.TotalHours,8:F2}h | {emp.PercentageOfTotal,5:F1}% | {status}");
            rank++;
        }
        Console.WriteLine("    ------------------------------------------------------------");

        // Step 4: Generate HTML Report
        Console.WriteLine("\n--> [Step 3] Generating HTML Table Report...");
        string html = htmlService.BuildHtmlReport(employeeSummaries, thresholdHours: 100.0);
        htmlService.SaveHtmlReport(html, htmlPath);
        Console.WriteLine($"    HTML Report saved to: {htmlPath}");

        // Step 5: Generate ScottPlot 5 Pie Chart
        Console.WriteLine("\n--> [Step 4] Generating ScottPlot 5.1.59 Pie Chart PNG...");
        chartService.ExportPieChart(employeeSummaries, pngPath);
        Console.WriteLine($"    Chart PNG saved to: {pngPath}");

        // Step 6: Automated Test Verification
        testingService.RunAllTests(htmlPath, pngPath);

        Console.WriteLine("================================================================");
        Console.WriteLine(" PRACTICE APPLICATION EXECUTION COMPLETE!");
        Console.WriteLine(" View your reports in:");
        Console.WriteLine($"   HTML: {htmlPath}");
        Console.WriteLine($"   PNG:  {pngPath}");
        Console.WriteLine("================================================================\n");
    }
}
