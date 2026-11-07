using System;
using System.Collections.Generic;
using System.Linq;
using RAPIDDTimesheetPractice.Models;

namespace RAPIDDTimesheetPractice.Services;

/// <summary>
/// Business logic service responsible for data cleansing, LINQ grouping,
/// accurate TimeSpan hours calculation, sorting, and percentage distribution.
/// </summary>
public class CalculationService
{
    /// <summary>
    /// Processes raw entries into clean, sorted employee summaries.
    /// Filters out deleted records, missing employee names, and corrupt timestamps.
    /// </summary>
    public List<EmployeeSummary> CalculateEmployeeSummaries(IEnumerable<TimeEntry> entries)
    {
        if (entries == null)
        {
            return new List<EmployeeSummary>();
        }

        // 1. Data Cleansing / Defensive Filtering
        // - Ignore entries where EmployeeName is null, empty, or whitespace
        // - Exclude soft-deleted entries (DeletedOn is populated)
        // - Exclude invalid timestamps where EndTime is earlier than or equal to StartTime
        var validEntries = entries.Where(e =>
            !string.IsNullOrWhiteSpace(e.EmployeeName) &&
            e.DeletedOn == null &&
            e.EndTimeUtc > e.StarTimeUtc
        );

        // 2. LINQ Grouping & Hours Calculation
        // CRITICAL CONCEPT: Use (EndTime - StartTime).TotalHours
        // Never use .Hours because .Hours only gives the whole integer part (0-23)
        // and completely drops fractional minutes and multi-day shifts!
        var summaries = validEntries
            .GroupBy(e => e.EmployeeName!.Trim())
            .Select(group => new EmployeeSummary
            {
                EmployeeName = group.Key,
                TotalHours = group.Sum(e => (e.EndTimeUtc - e.StarTimeUtc).TotalHours)
            })
            // 3. Sort descending by total hours
            .OrderByDescending(s => s.TotalHours)
            .ToList();

        // 4. Calculate Percentage of Total Working Hours
        double grandTotal = summaries.Sum(s => s.TotalHours);
        if (grandTotal > 0)
        {
            foreach (var summary in summaries)
            {
                summary.PercentageOfTotal = (summary.TotalHours / grandTotal) * 100.0;
            }
        }

        return summaries;
    }

    /// <summary>
    /// Validates whether the calculated percentages add up to approximately 100%.
    /// Allows a small epsilon (tolerance) for floating-point arithmetic.
    /// </summary>
    public bool ValidatePercentages(IEnumerable<EmployeeSummary> summaries, double tolerance = 0.05)
    {
        var list = summaries.ToList();
        if (list.Count == 0) return true;

        double sum = list.Sum(s => s.PercentageOfTotal);
        return Math.Abs(sum - 100.0) <= tolerance;
    }
}
