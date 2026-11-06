# CloudQA Resilient Selenium Automation (Task 1)

A high-resiliency C# Selenium automation script designed to handle dynamic page changes, nested Shadow DOMs (up to 6+ levels), and complex IFrames without relying on brittle, hardcoded XPaths or CSS selectors.

## Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/) (.NET 10 or compatible)
- Google Chrome browser
- ChromeDriver (managed automatically by Selenium NuGet packages)

## Setup & Dependencies
1. Initialize a new C# console project:
   ```bash
   dotnet new console -n CloudQA_Task1
   cd CloudQA_Task1

##Install Selenium and Support packages:

Bash
dotnet add package Selenium.WebDriver
dotnet add package Selenium.Support
Replace Program.cs with the provided script.

##How to Run
Run the project with the default target URL:

Bash
dotnet run
Or pass a configurable URL via command-line arguments to test against modified versions:

Bash
dotnet run "[https://app.cloudqa.io/home/AutomationPracticeForm](https://app.cloudqa.io/home/AutomationPracticeForm)"


## Approach & Limitations

### Approach

The solution uses a hybrid C# Selenium and JavaScript architecture.

- **C# Selenium:** Recursively discovers and traverses Shadow DOM hosts and nested IFrames.
- **JavaScript:** Identifies form fields using their visible labels and interacts with them by setting values and dispatching input/change events.
- **Resilient identification:** Avoids depending on fixed IDs, names, classes, or hardcoded XPath selectors.
- **Dynamic URL:** Accepts a configurable target URL through command-line arguments or an environment variable.

### Limitations

- Closed Shadow DOMs cannot be accessed using standard Selenium Shadow DOM traversal.
- Custom dropdown components may require specialized interaction logic.
- Significant changes to visible labels or form semantics may require updates to the field-identification logic.
- JavaScript event dispatching may not work with every framework or custom control.
- Cross-origin and dynamically loaded frames may require additional handling and synchronization.
