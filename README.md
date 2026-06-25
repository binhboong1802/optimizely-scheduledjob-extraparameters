# OptiScheduledJob.ExtraParameters

An add-on for **Optimizely CMS 12** (EPiServer) that lets you attach custom configuration
parameters to your scheduled jobs and edit them directly from the **Admin → Scheduled Jobs**
screen — something the stock CMS does not support out of the box.

## Why use it

Out of the box, Optimizely scheduled jobs have no place to store per-job settings, so teams
usually hard-code values, hang them off `appsettings.json`, or build a bespoke admin plugin.
This package gives each job a strongly-typed settings class whose values are:

- **Edited by CMS admins** in a form rendered automatically on the job's detail page.
- **Persisted** in Optimizely's Dynamic Data Store (one record per scheduled job).
- **Read back** inside the job at execution time through a simple service.

Supported field types render the appropriate input automatically: `bool` (checkbox),
integer types (number), floating-point types `decimal` / `double` / `float` (number),
`DateTime` (date picker), enumerable options (dropdown), and `string` / everything else
(text box).

## Requirements

- Optimizely CMS (`EPiServer.CMS`) **12.x**
- .NET **6.0** or **8.0**

## Installation

1. Add the package from the Optimizely NuGet feed:

   ```sh
   dotnet add package OptiScheduledJob.ExtraParameters
   ```

2. Register the module in your site's `Startup.cs`:

   ```csharp
   using OptiScheduledJob.ExtraParameters.Infrastructure.Configuration;

   public void ConfigureServices(IServiceCollection services)
   {
       services.AddCms();
       // ...
       services.AddScheduledJobExtraParameters();
   }
   ```

That's it — the package ships its own protected client module, so no manual file copying or
`module.config` editing is required.

## Usage

### 1. Define a settings class

Create a class that derives from `ScheduledJobExtraParametersBase`. Decorate each property with
`[ExtraParametersPropertyDisplay]` to control its label, help text, and (optionally) dropdown options.

```csharp
using OptiScheduledJob.ExtraParameters.Attributes;
using OptiScheduledJob.ExtraParameters.Models;

public class MyJobExtraParameters : ScheduledJobExtraParametersBase
{
    // string -> text box
    [ExtraParametersPropertyDisplay(DisplayName = "Source folder", Description = "Path to scan")]
    public string SourceFolder { get; set; }

    // integer types -> number input
    [ExtraParametersPropertyDisplay(DisplayName = "Batch size", Description = "Items per run")]
    public int BatchSize { get; set; }

    // floating-point types (decimal / double / float) -> number input
    [ExtraParametersPropertyDisplay(DisplayName = "Threshold", Description = "Minimum match score")]
    public decimal Threshold { get; set; }

    // bool -> checkbox
    [ExtraParametersPropertyDisplay(DisplayName = "Send notifications")]
    public bool SendNotifications { get; set; }

    // DateTime -> date picker
    [ExtraParametersPropertyDisplay(DisplayName = "Run after")]
    public DateTime RunAfter { get; set; }

    // Provide a string[] of options to render a dropdown. Each entry is used as both
    // the stored value and the option's display text.
    [ExtraParametersPropertyDisplay(DisplayName = "Priority", Options = new[] { "Low", "Medium", "High" })]
    public string Priority { get; set; }
}
```

### 2. Attach it to your scheduled job

Use `[ScheduledPlugInWithExtraParameters]` (a drop-in replacement for `[ScheduledPlugIn]`) and
point `ExtraParameterDefinition` at the settings class:

```csharp
using EPiServer.Scheduler;
using OptiScheduledJob.ExtraParameters.Attributes;

[ScheduledPlugInWithExtraParameters(
    DisplayName = "My Custom Job",
    ExtraParameterDefinition = typeof(MyJobExtraParameters))]
public class MyCustomJob : ScheduledJobBase
{
    // ...
}
```

After building and running, open **Admin → Scheduled Jobs → My Custom Job**; an "Extra Parameters"
form appears below the standard job controls.

### 3. Read the values when the job runs

Inject `IScheduledJobExtraParametersDataService` and load the saved settings by the job's instance id:

```csharp
using OptiScheduledJob.ExtraParameters.Services;

public class MyCustomJob : ScheduledJobBase
{
    private readonly IScheduledJobExtraParametersDataService _extraParameters;

    public MyCustomJob(IScheduledJobExtraParametersDataService extraParameters)
    {
        _extraParameters = extraParameters;
    }

    public override string Execute()
    {
        var settings = _extraParameters.Get<MyJobExtraParameters>(this.ScheduledJobId);
        var batchSize = settings?.BatchSize ?? 100;
        // ... use the settings
        return "Done";
    }
}
```

## License

MIT — see [LICENSE](LICENSE).
