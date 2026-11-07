using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScottPlot;
using RAPIDDTimesheetPractice.Models;

namespace RAPIDDTimesheetPractice.Services;

/// <summary>
/// Service responsible for rendering employee timesheet analytics into a pie chart
/// using ScottPlot 5.x and exporting it to a PNG file.
/// </summary>
public class ChartService
{
    /// <summary>
    /// Renders a pie chart where each slice represents an employee's share of total working hours.
    /// Slices are labelled with percentage values and the legend displays employee names and hours.
    /// </summary>
    public void ExportPieChart(IEnumerable<EmployeeSummary> summaries, string outputFilePath, int width = 900, int height = 650)
    {
        var summaryList = summaries.ToList();
        if (summaryList.Count == 0)
        {
            Console.WriteLine("[ChartService] Warning: No employees to plot.");
            return;
        }

        // 1. Initialize ScottPlot Plot object
        Plot plot = new();

        // 2. Build list of PieSlice objects
        // In ScottPlot 5, PieSlice holds Value (for proportions), Label (on the slice),
        // and LegendText (in the legend).
        var slices = summaryList.Select(emp => new PieSlice
        {
            Value = emp.TotalHours,
            Label = $"{emp.PercentageOfTotal:F1}%",
            LegendText = $"{emp.EmployeeName} ({emp.TotalHours:F1} hrs)"
        }).ToList();

        // 3. Add pie chart to the plot
        var pie = plot.Add.Pie(slices);
        pie.ExplodeFraction = 0.04;    // Slightly pull slices apart for readability
        pie.SliceLabelDistance = 0.65; // Position percentage text nicely inside the slice

        // 4. Style the plot for a clean presentation
        plot.ShowLegend();
        plot.Axes.Frameless(); // Hide axes as pie charts don't use X/Y coordinates
        plot.HideGrid();

        // 5. Ensure directory exists and export to PNG
        var directory = Path.GetDirectoryName(outputFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        plot.SavePng(outputFilePath, width, height);
        Console.WriteLine($"[ChartService] Exported pie chart PNG: {outputFilePath}");
    }
}
