namespace AppStudioJsonViewer.Models;

/// <summary>
/// One row of Ice.XXXDef. The grid only displays <see cref="Key1"/> and
/// <see cref="Key2"/>, but every primary-key column is carried so the editor
/// can target the correct row on UPDATE.
/// </summary>
public sealed class XxxDefRow
{
    public string Company { get; init; } = "";
    public string ProductID { get; init; } = "";
    public string TypeCode { get; init; } = "";
    public string Key1 { get; init; } = "";
    public string Key2 { get; init; } = "";
    public string Key3 { get; init; } = "";
    public string CGCCode { get; init; } = "";

    /// <summary>Raw JSON stored in the Content column (minified in the DB).</summary>
    public string Content { get; set; } = "";
}
