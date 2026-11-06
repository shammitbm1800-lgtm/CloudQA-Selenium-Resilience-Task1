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


##Approach & Limitations

Approach: Utilizes a Hybrid Architecture. C# handles recursive structural discovery (traversing deep Shadow DOM hosts and nested IFrames natively), while an injected JavaScript payload executes inside each boundary to bypass W3C restrictions, defeat React/Angular event blockers, and map fields positionally to avoid decoys.

Limitations: Third-party non-standard custom combobox components (like custom React-select wrappers) require explicit specialized click-and-type logic rather than native <select> element selectors.
