namespace RepoParser.Core.Entities;

public class SourceFile
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public string ContentHash { get; set; } = string.Empty;
    public ICollection<MethodDefinition> Methods { get; set; } = new List<MethodDefinition>();
}
