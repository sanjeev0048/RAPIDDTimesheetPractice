using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using RAPIDDTimesheetPractice.Models;

namespace RAPIDDTimesheetPractice.Services;

/// <summary>
/// Service responsible for retrieving and deserializing employee timesheet data.
/// Supports fetching from an HTTP API or loading realistic synthetic test data.
/// </summary>
public class DataService
{
    private readonly HttpClient _httpClient;

    public DataService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    /// <summary>
    /// Simulates or executes an HTTP GET request to retrieve JSON time entries.
    /// Demonstrates real HttpClient lifecycle, status code validation, and error handling.
    /// </summary>
    public async Task<List<TimeEntry>> FetchEntriesFromApiAsync(string apiUrl)
    {
        try
        {
            Console.WriteLine($"[DataService] Sending HTTP GET to: {apiUrl}...");
            using HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            // Validates status code 200-299; throws HttpRequestException on 401, 404, 500, etc.
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            return DeserializeJson(json);
        }
        catch (HttpRequestException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[DataService Warning] HTTP request failed: {ex.Message}");
            Console.WriteLine("[DataService Warning] Falling back to synthetic test dataset.");
            Console.ResetColor();
            return GetSyntheticDataSet();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[DataService Error] Unexpected error during fetch: {ex.Message}");
            Console.ResetColor();
            throw;
        }
    }

    /// <summary>
    /// Deserializes a JSON string into a List of TimeEntry objects using System.Text.Json.
    /// </summary>
    public List<TimeEntry> DeserializeJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<TimeEntry>();
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var result = JsonSerializer.Deserialize<List<TimeEntry>>(json, options);
        return result ?? new List<TimeEntry>();
    }

    /// <summary>
    /// Provides a rich synthetic dataset mimicking real timesheet entries.
    /// Includes both normal records (>100h and <100h) and edge cases
    /// (null names, soft-deleted entries, inverted timestamps).
    /// </summary>
    public List<TimeEntry> GetSyntheticDataSet()
    {
        return new List<TimeEntry>
        {
            // Employee 1: Devon Vance (High hours > 100h)
            new() { Id = "1", EmployeeName = "Devon Vance", StarTimeUtc = new DateTime(2024, 1, 1, 8, 0, 0), EndTimeUtc = new DateTime(2024, 1, 1, 17, 30, 0), EntryNotes = "Backend setup" },
            new() { Id = "2", EmployeeName = "Devon Vance", StarTimeUtc = new DateTime(2024, 1, 2, 8, 0, 0), EndTimeUtc = new DateTime(2024, 1, 15, 8, 0, 0), EntryNotes = "Sprint project load" }, // 312 hours

            // Employee 2: Aaliyah Khan (Moderate hours > 100h)
            new() { Id = "3", EmployeeName = "Aaliyah Khan", StarTimeUtc = new DateTime(2024, 1, 1, 9, 0, 0), EndTimeUtc = new DateTime(2024, 1, 6, 9, 0, 0), EntryNotes = "UI/UX revamp" }, // 120 hours

            // Employee 3: Liam Chen (Below 100h threshold -> 65 hours)
            new() { Id = "4", EmployeeName = "Liam Chen", StarTimeUtc = new DateTime(2024, 1, 1, 10, 0, 0), EndTimeUtc = new DateTime(2024, 1, 3, 17, 0, 0), EntryNotes = "DevOps migration" }, // 55 hours
            new() { Id = "5", EmployeeName = "Liam Chen", StarTimeUtc = new DateTime(2024, 1, 4, 9, 0, 0), EndTimeUtc = new DateTime(2024, 1, 4, 19, 0, 0), EntryNotes = "CI/CD fixes" }, // 10 hours

            // Employee 4: Marcus Sterling (Below 100h threshold -> 24 hours)
            new() { Id = "6", EmployeeName = "Marcus Sterling", StarTimeUtc = new DateTime(2024, 1, 5, 8, 0, 0), EndTimeUtc = new DateTime(2024, 1, 6, 8, 0, 0), EntryNotes = "Security audit" }, // 24 hours

            // Employee 5: Priya Sharma (High hours > 100h -> 140 hours)
            new() { Id = "7", EmployeeName = "Priya Sharma", StarTimeUtc = new DateTime(2024, 1, 1, 8, 0, 0), EndTimeUtc = new DateTime(2024, 1, 6, 20, 0, 0), EntryNotes = "Data modeling" }, // 132 hours
            new() { Id = "8", EmployeeName = "Priya Sharma", StarTimeUtc = new DateTime(2024, 1, 8, 9, 0, 0), EndTimeUtc = new DateTime(2024, 1, 8, 17, 0, 0), EntryNotes = "Query tuning" }, // 8 hours

            // EDGE CASE 1: Missing Employee Name (Should be filtered out)
            new() { Id = "9", EmployeeName = null, StarTimeUtc = new DateTime(2024, 1, 1, 8, 0, 0), EndTimeUtc = new DateTime(2024, 1, 1, 16, 0, 0), EntryNotes = "Unassigned work" },

            // EDGE CASE 2: Soft-deleted record (Should be filtered out)
            new() { Id = "10", EmployeeName = "Marcus Sterling", StarTimeUtc = new DateTime(2024, 1, 7, 8, 0, 0), EndTimeUtc = new DateTime(2024, 1, 7, 16, 0, 0), DeletedOn = new DateTime(2024, 1, 8) },

            // EDGE CASE 3: Inverted timestamps (End < Start) (Corrupt entry, should be filtered out)
            new() { Id = "11", EmployeeName = "Aaliyah Khan", StarTimeUtc = new DateTime(2024, 1, 10, 18, 0, 0), EndTimeUtc = new DateTime(2024, 1, 10, 8, 0, 0), EntryNotes = "Clock error" }
        };
    }
}
