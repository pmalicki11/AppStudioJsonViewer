# AppStudioJsonViewer

WPF (.NET 8) utility for browsing and editing Epicor/Kinetic App Studio customization-layer JSON stored in `Ice.XXXDef`.

## What it does

- **Landing page** — lists `Ice.XXXDef` rows where `TypeCode = KNTCCustLayer`. Filter by Application ID or Layer Name, pick an environment, then click **Load**.
- **Editor** — clicking a Layer Name opens that row's `Content` column, prettified with JSON syntax coloring (AvalonEdit). Ctrl+F to search within the JSON.
- **Save** — edit the JSON and save it back to the database. Content is validated, then written minified.
- JSON is fetched fresh from the database each time you open a layer — no stale cache.

## Requirements

- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (x64) must be installed on the machine.
- Windows authentication access to the target SQL Server databases.

## Setup

1. Copy `appsettings.example.json` to `appsettings.json` next to the exe (or in the project root when running from source).
2. Fill in the connection strings for each environment:

```json
{
  "Environments": [
    {
      "Name": "Environment1",
      "ConnectionString": "Server=YOUR_SERVER;Database=YOUR_DB;Integrated Security=true;TrustServerCertificate=true;Encrypt=true;"
    },
    {
      "Name": "Environment2",
      "ConnectionString": "Server=YOUR_SERVER;Database=YOUR_DB;Integrated Security=true;TrustServerCertificate=true;Encrypt=true;"
    }
  ],
  "Query": {
    "Schema": "Ice",
    "Table": "XXXDef",
    "TypeCode": "KNTCCustLayer"
  }
}
```

You can add or remove environments freely — the dropdown is populated from this list. The first entry is selected by default.

`appsettings.json` is gitignored. The example file is safe to commit.

## Running

Select an environment from the dropdown and click **Load**. Changing the environment clears the grid without loading — click Load again to fetch data for the new environment.

## Building

Run the publish script to produce a single exe and copy it to your desktop:

```powershell
powershell -ExecutionPolicy Bypass -File .\publish.ps1
```

The script builds the project, places the exe in `bin\Release\framework-dependent\`, then copies it to your desktop. The exe requires .NET 8 Desktop Runtime to be installed on any machine it runs on.
