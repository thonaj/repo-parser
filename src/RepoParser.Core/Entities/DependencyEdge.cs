namespace RepoParser.Core.Entities;

public class DependencyEdge
{
    public int Id { get; set; }
    public int SourceFileId { get; set; }
    public SourceFile SourceFile { get; set; } = null!;
    public string TargetFilePath { get; set; } = string.Empty;
    public string DependencyType { get; set; } = string.Empty; // "Import", "Using", "Require"
}
