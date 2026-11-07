# RAPIDD Employee Timesheet Practice System

A complete, beginner-friendly practice application built with **C#** and **.NET 9.0** demonstrating how to retrieve timesheet data, aggregate hours, generate styled HTML reports, export ScottPlot 5 pie charts, and perform automated testing.

---

## 🎯 Architecture & Project Structure

```
RAPIDDTimesheetPractice/
├── Models/
│   └── TimeEntry.cs           # TimeEntry DTO and EmployeeSummary data models
├── Services/
│   ├── DataService.cs         # HTTP GET handling & System.Text.Json deserialization
│   ├── CalculationService.cs  # LINQ grouping, TimeSpan.TotalHours & percentage math
│   ├── HtmlReportService.cs   # StringBuilder HTML generation with conditional CSS
│   ├── ChartService.cs        # ScottPlot 5.1.59 pie chart & PNG rendering
│   └── TestingService.cs      # Automated unit-like verification test suite
├── Output/                    # Generated report files
│   ├── index.html             # Styled HTML table highlighting < 100 hrs
│   └── employee-hours.png     # Rendered pie chart with employee percentage slices
├── Program.cs                 # Main orchestration pipeline
├── README.md                  # Complete documentation
└── RAPIDDTimesheetPractice.csproj
```

---

## 🚀 How to Run the Application

### 1. Open Terminal in VS Code
Ensure you are in the `RAPIDDTimesheetPractice` directory:
```bash
cd C:\RAPIDDEmployeeTimesheet\RAPIDDTimesheetPractice
```

### 2. Build the Project
```bash
dotnet build
```
Verify that the build outputs: `Build succeeded. 0 Warning(s), 0 Error(s)`.

### 3. Run the Program
```bash
dotnet run
```

---

## 💡 Key Architectural Concepts Explained Simply

### 1. Handling the Schema Typo with `[JsonPropertyName]`
* **The Problem:** The API schema contains the spelling `"StarTimeUtc"` (missing the letter `t`).
* **The Solution:** In `TimeEntry.cs`, we apply `[JsonPropertyName("StarTimeUtc")]`. This binds the JSON key cleanly to C# without requiring messy string manipulation.

### 2. Why Use `TimeSpan.TotalHours` (Not `TimeSpan.Hours`)?
* `(EndTime - StartTime).Hours` returns only the **integer component** of the current day (e.g., `8` hours for an 8 hour 45 minute shift, discarding 45 minutes!).
* `(EndTime - StartTime).TotalHours` returns the full fractional precision (e.g., `8.75` hours), ensuring exact payroll calculations.

### 3. LINQ Grouping & Sorting Pipeline
In `CalculationService.cs`:
```csharp
var summaries = validEntries
    .GroupBy(e => e.EmployeeName.Trim())
    .Select(group => new EmployeeSummary
    {
        EmployeeName = group.Key,
        TotalHours = group.Sum(e => (e.EndTimeUtc - e.StarTimeUtc).TotalHours)
    })
    .OrderByDescending(s => s.TotalHours)
    .ToList();
```
* **`GroupBy`**: Gathers all shifts belonging to the same employee.
* **`Select`**: Transforms each employee group into a single summary containing their summed hours.
* **`OrderByDescending`**: Places the highest contributors at the top.

### 4. Memory-Efficient HTML Composition
In `HtmlReportService.cs`, we compose HTML using **`StringBuilder`** instead of repeated `string += "..."` concatenation. In loops, string concatenation allocates memory repeatedly; `StringBuilder` uses an internal expandable buffer.

Conditional styling identifies employees below 100 hours:
```csharp
string rowClass = emp.TotalHours < 100.0 ? " class=\"highlight-low-hours\"" : "";
```

### 5. ScottPlot 5.1.59 Pie Chart Generation
In `ChartService.cs`:
* We instantiate a `Plot` object.
* We populate `PieSlice` items with `Value` (total hours) and `Label` (formatted percentage).
* We set `plot.ShowLegend()`, remove border axes with `plot.Axes.Frameless()`, and save to PNG via `plot.SavePng(...)`.

---

## 🧪 Automated Testing Included

When you run `dotnet run`, `TestingService.cs` automatically executes 6 verification checks:
1. **Empty Dataset Handling:** Verifies the app handles empty payloads gracefully without throwing exceptions.
2. **Null Employee Names:** Verifies unassigned records are filtered out.
3. **Inverted Timestamps:** Verifies corrupt records where `EndTime <= StartTime` are excluded.
4. **Deleted Records:** Verifies soft-deleted records (`DeletedOn != null`) are excluded.
5. **Percentage Validation:** Verifies that all employee percentages add up to 100% within tolerance.
6. **Output File Verification:** Confirms both `index.html` and `employee-hours.png` were created on disk and have non-zero file sizes.

---

## 📦 How to Publish as a Single-File Windows Executable

To compile a standalone `.exe` that runs on any 64-bit Windows machine without needing .NET installed:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Output directory:
`bin\Release\net9.0\win-x64\publish\RAPIDDTimesheetPractice.exe`
