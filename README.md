# OptiScheduledJob.ExtraParameters

An add-on for **Optimizely CMS** (EPiServer) that lets you attach custom configuration
parameters to your scheduled jobs and edit them directly from the **Admin → Scheduled Jobs**
screen — something the stock CMS does not support out of the box.

Supports **CMS 12** and **CMS 13**.

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

## Which version do I need?

The add-on ships as **one package with two release lines**, split by CMS major version. Pick the
line that matches your site — the API and everything in [Usage](#usage) is the same in both.

| Package version | Optimizely CMS | .NET                   |
| --------------- | -------------- | ---------------------- |
| **1.x**         | 12.x           | 6.0 / 8.0 / 9.0 / 10.0 |
| **2.x**         | 13.x           | 10.0                   |

Each line pins its own `EPiServer.CMS` version range, so installing the wrong one fails NuGet restore
with a clear version conflict rather than misbehaving at runtime.

## Installation

1. Add the package from the **Optimizely NuGet feed** (https://nuget.optimizely.com/). It is
   published there, not on nuget.org, so register that feed as a source first:

   ```sh
   dotnet nuget add source https://api.nuget.optimizely.com/v3/index.json -n optimizely
   ```

   Then install the line matching your CMS:

   ```sh
   # Optimizely CMS 12
   dotnet add package OptiScheduledJob.ExtraParameters --version "[1.0,2.0)"

   # Optimizely CMS 13
   dotnet add package OptiScheduledJob.ExtraParameters --version "[2.0,3.0)"
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

Identical on CMS 12 and CMS 13.

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

Use `[ScheduledPlugInWithExtraParameters]` in place of the stock job attribute and point
`ExtraParameterDefinition` at the settings class:

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

The attribute carries the usual job settings (`DisplayName`, `Description`, `GUID`) on both lines —
it extends `ScheduledPlugInAttribute` on 1.x and `ScheduledJobAttribute` on 2.x, since CMS 13
obsoleted the former. Your job class inherits `ScheduledJobBase` either way.

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

## How it looks in the admin

Once your job carries the attribute, open **Admin → Scheduled Jobs → your job**. Below the
standard "When to run" controls, an **Extra Parameters** form appears automatically — each
property rendered with the right input for its type (text box, number, date picker, checkbox
or dropdown), one per row:

![The Extra Parameters form rendered under a scheduled job](docs/images/extra-parameters-form.png)

Hit **Save** and the values are converted to their CLR types, stored in the Dynamic Data
Store, and a toast notification confirms the result:

![Save confirmation notification](docs/images/save-notification.png)

*(Screenshots are from the CMS 12 admin UI.)*

## Building from source

There are no automated tests. Both lines build from the repository root:

```sh
dotnet build OptiScheduledJob.ExtraParameters.sln

# CMS 12 line — src/OptiScheduledJob.ExtraParameters
dotnet pack src/OptiScheduledJob.ExtraParameters/OptiScheduledJob.ExtraParameters.csproj -c Release

# CMS 13 line — src/OptiScheduledJob.ExtraParameters.Cms13
dotnet pack src/OptiScheduledJob.ExtraParameters.Cms13/OptiScheduledJob.ExtraParameters.Cms13.csproj -c Release
```

Bump `<Version>` in the relevant `.csproj` before producing a new package — keep 1.x for CMS 12 and
2.x for CMS 13.

## License

MIT — see [LICENSE](LICENSE).
