using RepoParser.Core.Enums;

namespace RepoParser.Core.Entities;

public class DriftAlert
{
    public int Id { get; set; }
    public int MethodDefinitionId { get; set; }
    public MethodDefinition MethodDefinition { get; set; } = null!;
    public AlertType AlertType { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string OldSignature { get; set; } = string.Empty;
    public string NewSignature { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
