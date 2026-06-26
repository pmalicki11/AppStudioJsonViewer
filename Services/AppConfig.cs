using System.IO;
using System.Reflection;
using System.Text.Json;

namespace AppStudioJsonViewer.Services;

public sealed class EnvironmentEntry
{
    public string Name { get; init; } = "";
    public string ConnectionString { get; init; } = "";
    public override string ToString() => Name;
}

/// <summary>
/// Minimal reader for appsettings.json (avoids pulling in the full
/// Microsoft.Extensions.Configuration stack).
/// </summary>
public sealed class AppConfig
{
    public IReadOnlyList<EnvironmentEntry> Environments { get; }
    public string Schema { get; }
    public string Table { get; }
    public string TypeCode { get; }

    private AppConfig(IReadOnlyList<EnvironmentEntry> environments, string schema, string table, string typeCode)
    {
        Environments = environments;
        Schema = schema;
        Table = table;
        TypeCode = typeCode;
    }

    public static AppConfig Load()
    {
        using var doc = JsonDocument.Parse(ReadConfigJson());
        var root = doc.RootElement;

        var environments = new List<EnvironmentEntry>();
        if (root.TryGetProperty("Environments", out var envArray))
        {
            foreach (var env in envArray.EnumerateArray())
            {
                var name = env.TryGetProperty("Name", out var n) ? n.GetString() ?? "" : "";
                var conn = env.TryGetProperty("ConnectionString", out var c) ? c.GetString() ?? "" : "";
                if (name.Length > 0 && conn.Length > 0)
                    environments.Add(new EnvironmentEntry { Name = name, ConnectionString = conn });
            }
        }

        if (environments.Count == 0)
            throw new InvalidOperationException("No environments defined in appsettings.json.");

        string schema = "Ice", table = "XXXDef", typeCode = "KNTCCustLayer";
        if (root.TryGetProperty("Query", out var q))
        {
            schema   = q.TryGetProperty("Schema",   out var s)  ? s.GetString()  ?? schema   : schema;
            table    = q.TryGetProperty("Table",     out var t)  ? t.GetString()  ?? table    : table;
            typeCode = q.TryGetProperty("TypeCode",  out var tc) ? tc.GetString() ?? typeCode : typeCode;
        }

        return new AppConfig(environments, schema, table, typeCode);
    }

    private static string ReadConfigJson()
    {
        var loosePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (File.Exists(loosePath))
            return File.ReadAllText(loosePath);

        var asm = Assembly.GetExecutingAssembly();
        var resourceName = asm.GetManifestResourceNames()
            .Single(n => n.EndsWith("appsettings.json", StringComparison.OrdinalIgnoreCase));
        using var stream = asm.GetManifestResourceStream(resourceName)
                           ?? throw new InvalidOperationException("Embedded appsettings.json not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
