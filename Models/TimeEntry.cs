using System;
using System.Text.Json.Serialization;

namespace RAPIDDTimesheetPractice.Models;

/// <summary>
/// Represents a raw employee timesheet entry from the JSON payload.
/// Note the [JsonPropertyName("StarTimeUtc")] attribute to handle the API typo.
/// </summary>
public class TimeEntry
{
    [JsonPropertyName("Id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("EmployeeName")]
    public string? EmployeeName { get; set; }

    [JsonPropertyName("StarTimeUtc")]
    public DateTime StarTimeUtc { get; set; }

    [JsonPropertyName("EndTimeUtc")]
    public DateTime EndTimeUtc { get; set; }

    [JsonPropertyName("EntryNotes")]
    public string? EntryNotes { get; set; }

    [JsonPropertyName("DeletedOn")]
    public DateTime? DeletedOn { get; set; }
}

/// <summary>
/// Represents an aggregated summary of working hours and analytics for a single employee.
/// </summary>
public class EmployeeSummary
{
    public string EmployeeName { get; set; } = string.Empty;
    public double TotalHours { get; set; }
    public double PercentageOfTotal { get; set; }
    public bool IsBelowThreshold => TotalHours < 100.0;
}
