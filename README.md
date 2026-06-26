# AppStudioJsonViewer

A small WPF (.NET 8) utility for viewing and editing Epicor App Studio
customization-layer JSON stored in `Ice.XXXDef`.

## What it does

1. **Landing page** — lists `Ice.XXXDef` rows where `TypeCode = KNTCCustLayer`,
   showing `Key1` and `Key2`. `Key1` is a clickable link.
2. **Editor** — clicking `Key1` opens that row's `Content` column, prettified
   (indented + JSON syntax coloring via AvalonEdit).
3. **Save** — you can edit the JSON and save it back to the database. It is
   validated, then written **minified** (no indents or line breaks).

## Setup

Edit `appsettings.json` (it is copied next to the .exe on build):

```json
{
  "ConnectionStrings": {
    "Epicor": "Server=YOUR_SQL_SERVER\\INSTANCE;Database=YOUR_EPICOR_DB;Integrated Security=true;TrustServerCertificate=true;Encrypt=true;"
  },
  "Query": {
    "Schema": "Ice",
    "Table": "XXXDef",
    "TypeCode": "KNTCCustLayer"
  }
}
```

- Connection uses **Windows authentication** (`Integrated Security=true`), so the
  Windows account running the app must have read/write access to the table.
- `Schema` / `Table` / `TypeCode` are configurable if you ever need to point it
  elsewhere.

## Run

```powershell
dotnet run --project AppStudioJsonViewer.csproj
```

## Update targeting

Rows are updated by their full natural key:
`Company, ProductID, TypeCode, Key1, Key2, Key3, CGCCode`. A save expects to
affect exactly one row; anything else is reported and treated as suspect.
