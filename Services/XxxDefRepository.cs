using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AppStudioJsonViewer.Models;
using Microsoft.Data.SqlClient;

namespace AppStudioJsonViewer.Services;

/// <summary>Reads and updates rows of the Ice.XXXDef table.</summary>
public sealed class XxxDefRepository
{
    private readonly AppConfig _config;
    private readonly string _qualifiedTable;

    public XxxDefRepository(AppConfig config, string connectionString)
    {
        _config = config;
        _qualifiedTable = $"{QuoteIdentifier(config.Schema)}.{QuoteIdentifier(config.Table)}";
        _connectionString = connectionString;
    }

    private readonly string _connectionString;

    /// <summary>
    /// Loads the landing-page rows (TypeCode = the configured value), off the UI
    /// thread so the caller can keep the window responsive while it runs.
    /// </summary>
    public async Task<List<XxxDefRow>> GetLayerRowsAsync(CancellationToken cancellationToken = default)
    {
        const string columns =
            "Company, ProductID, TypeCode, Key1, Key2, Key3, CGCCode, Content";
        var sql = $"SELECT {columns} FROM {_qualifiedTable} " +
                  "WHERE TypeCode = @TypeCode ORDER BY Key1, Key2";

        var rows = new List<XxxDefRow>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@TypeCode", _config.TypeCode);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new XxxDefRow
            {
                Company = reader["Company"] as string ?? "",
                ProductID = reader["ProductID"] as string ?? "",
                TypeCode = reader["TypeCode"] as string ?? "",
                Key1 = reader["Key1"] as string ?? "",
                Key2 = reader["Key2"] as string ?? "",
                Key3 = reader["Key3"] as string ?? "",
                CGCCode = reader["CGCCode"] as string ?? "",
                Content = reader["Content"] as string ?? "",
            });
        }
        return rows;
    }

    /// <summary>Re-fetches the Content column for a single row from the database.</summary>
    public async Task<string> GetContentAsync(XxxDefRow row)
    {
        var sql = $"SELECT Content FROM {_qualifiedTable} " +
                  "WHERE Company = @Company AND ProductID = @ProductID AND TypeCode = @TypeCode " +
                  "AND Key1 = @Key1 AND Key2 = @Key2 AND Key3 = @Key3 AND CGCCode = @CGCCode";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Company", row.Company);
        cmd.Parameters.AddWithValue("@ProductID", row.ProductID);
        cmd.Parameters.AddWithValue("@TypeCode", row.TypeCode);
        cmd.Parameters.AddWithValue("@Key1", row.Key1);
        cmd.Parameters.AddWithValue("@Key2", row.Key2);
        cmd.Parameters.AddWithValue("@Key3", row.Key3);
        cmd.Parameters.AddWithValue("@CGCCode", row.CGCCode);

        var result = await cmd.ExecuteScalarAsync();
        return result as string ?? "";
    }

    /// <summary>
    /// Writes <paramref name="minifiedContent"/> back to the Content column,
    /// targeting the row by its full natural key. Returns rows affected.
    /// </summary>
    public int UpdateContent(XxxDefRow row, string minifiedContent)
    {
        var sql =
            $"UPDATE {_qualifiedTable} SET Content = @Content " +
            "WHERE Company = @Company AND ProductID = @ProductID AND TypeCode = @TypeCode " +
            "AND Key1 = @Key1 AND Key2 = @Key2 AND Key3 = @Key3 AND CGCCode = @CGCCode";

        using var conn = new SqlConnection(_connectionString);
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Content", minifiedContent);
        cmd.Parameters.AddWithValue("@Company", row.Company);
        cmd.Parameters.AddWithValue("@ProductID", row.ProductID);
        cmd.Parameters.AddWithValue("@TypeCode", row.TypeCode);
        cmd.Parameters.AddWithValue("@Key1", row.Key1);
        cmd.Parameters.AddWithValue("@Key2", row.Key2);
        cmd.Parameters.AddWithValue("@Key3", row.Key3);
        cmd.Parameters.AddWithValue("@CGCCode", row.CGCCode);

        return cmd.ExecuteNonQuery();
    }

    /// <summary>Brackets a SQL identifier, rejecting anything that isn't a plain name.</summary>
    private static string QuoteIdentifier(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || !Regex.IsMatch(name, "^[A-Za-z0-9_]+$"))
            throw new ArgumentException($"Invalid SQL identifier: '{name}'");
        return $"[{name}]";
    }
}
