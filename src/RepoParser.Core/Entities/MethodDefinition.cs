namespace RepoParser.Core.Entities;

public class MethodDefinition
{
    public int Id { get; set; }
    public int SourceFileId { get; set; }
    public SourceFile SourceFile { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string Parameters { get; set; } = string.Empty; // JSON array
    public string ReturnType { get; set; } = string.Empty;
    public string DocComment { get; set; } = string.Empty;
    public int LineStart { get; set; }
    public int LineEnd { get; set; }
    public byte[]? CodeEmbedding { get; set; }
    public byte[]? DocEmbedding { get; set; }
    public DateTime LastEmbedded { get; set; }
    public ICollection<DriftAlert> Alerts { get; set; } = new List<DriftAlert>();
}
