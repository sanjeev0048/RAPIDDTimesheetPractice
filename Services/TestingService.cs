using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RAPIDDTimesheetPractice.Models;

namespace RAPIDDTimesheetPractice.Services;

/// <summary>
/// Automated verification service that tests calculations, edge-case resilience,
/// and output file integrity.
/// </summary>
public class TestingService
{
    private readonly CalculationService _calcService = new();

    public void RunAllTests(string htmlPath, string pngPath)
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" RUNNING AUTOMATED PRACTICE VERIFICATION TESTS");
        Console.WriteLine("========================================================");

        int passed = 0;
        int total = 6;

        if (TestEmptyDataset()) passed++;
        if (TestNullEmployeeNames()) passed++;
        if (TestInvertedTimestamps()) passed++;
        if (TestDeletedRecords()) passed++;
        if (TestPercentageMath()) passed++;
        if (TestOutputFiles(htmlPath, pngPath)) passed++;

        Console.WriteLine("--------------------------------------------------------");
        Console.WriteLine($" TEST RESULTS: {passed}/{total} Passed");
        Console.WriteLine("========================================================\n");
    }

    private bool TestEmptyDataset()
    {
        var result = _calcService.CalculateEmployeeSummaries(new List<TimeEntry>());
        bool ok = result.Count == 0;
        PrintTest("Empty dataset returns empty summary list without throwing", ok);
        return ok;
    }

    private bool TestNullEmployeeNames()
    {
        var entries = new List<TimeEntry>
        {
            new() { EmployeeName = null, StarTimeUtc = DateTime.UtcNow, EndTimeUtc = DateTime.UtcNow.AddHours(5) },
            new() { EmployeeName = "   ", StarTimeUtc = DateTime.UtcNow, EndTimeUtc = DateTime.UtcNow.AddHours(3) },
            new() { EmployeeName = "Valid Person", StarTimeUtc = DateTime.UtcNow, EndTimeUtc = DateTime.UtcNow.AddHours(4) }
        };

        var result = _calcService.CalculateEmployeeSummaries(entries);
        bool ok = result.Count == 1 && result[0].EmployeeName == "Valid Person";
        PrintTest("Null or whitespace employee names are safely filtered", ok);
        return ok;
    }

    private bool TestInvertedTimestamps()
    {
        var entries = new List<TimeEntry>
        {
            // Corrupt entry where EndTime is BEFORE StartTime
            new() { EmployeeName = "Corrupt Time", StarTimeUtc = DateTime.UtcNow.AddHours(5), EndTimeUtc = DateTime.UtcNow }
        };

        var result = _calcService.CalculateEmployeeSummaries(entries);
        bool ok = result.Count == 0;
        PrintTest("Inverted timestamps (EndTime <= StartTime) are excluded", ok);
        return ok;
    }

    private bool TestDeletedRecords()
    {
        var entries = new List<TimeEntry>
        {
            new() { EmployeeName = "Active Emp", StarTimeUtc = DateTime.UtcNow, EndTimeUtc = DateTime.UtcNow.AddHours(8), DeletedOn = null },
            new() { EmployeeName = "Deleted Emp", StarTimeUtc = DateTime.UtcNow, EndTimeUtc = DateTime.UtcNow.AddHours(8), DeletedOn = DateTime.UtcNow }
        };

        var result = _calcService.CalculateEmployeeSummaries(entries);
        bool ok = result.Count == 1 && result[0].EmployeeName == "Active Emp";
        PrintTest("Soft-deleted records (DeletedOn is set) are excluded", ok);
        return ok;
    }

    private bool TestPercentageMath()
    {
        var entries = new List<TimeEntry>
        {
            new() { EmployeeName = "Emp A", StarTimeUtc = DateTime.UtcNow, EndTimeUtc = DateTime.UtcNow.AddHours(40) },
            new() { EmployeeName = "Emp B", StarTimeUtc = DateTime.UtcNow, EndTimeUtc = DateTime.UtcNow.AddHours(60) }
        };

        var result = _calcService.CalculateEmployeeSummaries(entries);
        bool ok = _calcService.ValidatePercentages(result);
        PrintTest("Calculated percentages sum to 100.0% within tolerance", ok);
        return ok;
    }

    private bool TestOutputFiles(string htmlPath, string pngPath)
    {
        bool htmlExists = File.Exists(htmlPath) && new FileInfo(htmlPath).Length > 0;
        bool pngExists = File.Exists(pngPath) && new FileInfo(pngPath).Length > 0;
        bool ok = htmlExists && pngExists;
        PrintTest($"Output files verified on disk (HTML: {htmlExists}, PNG: {pngExists})", ok);
        return ok;
    }

    private void PrintTest(string description, bool success)
    {
        if (success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($" [PASS] {description}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" [FAIL] {description}");
        }
        Console.ResetColor();
    }
}
