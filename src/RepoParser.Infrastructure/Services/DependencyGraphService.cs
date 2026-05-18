using RepoParser.Core.Entities;
using RepoParser.Core.Interfaces;
using RepoParser.Infrastructure.Data;

namespace RepoParser.Infrastructure.Services;

public class DependencyGraphService : IDependencyGraphService
{
    private readonly RepoDbContext _context;
    private readonly IRepositoryService _repository;

    public DependencyGraphService(RepoDbContext context, IRepositoryService repository)
    {
        _context = context;
        _repository = repository;
    }

    public async Task<List<DependencyEdge>> BuildGraphAsync(string rootPath)
    {
        var edges = new List<DependencyEdge>();
        var files = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories)
            .Where(f => IsSourceFile(f))
            .ToList();

        foreach (var filePath in files)
        {
            var sourceFile = await _repository.GetFileByPathAsync(filePath);
            if (sourceFile is null) continue;

            var dependencies = ExtractDependencies(filePath);
            foreach (var dep in dependencies)
            {
                var edge = new DependencyEdge
                {
                    SourceFileId = sourceFile.Id,
                    TargetFilePath = dep,
                    DependencyType = "Using"
                };
                edges.Add(edge);
                await _repository.AddDependencyEdgeAsync(edge);
            }
        }

        return edges;
    }

    public async Task<List<DependencyEdge>> GetDependenciesAsync(int sourceFileId)
    {
        return await _repository.GetDependenciesAsync(sourceFileId);
    }

    private static List<string> ExtractDependencies(string filePath)
    {
        var dependencies = new List<string>();
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        try
        {
            var content = File.ReadAllText(filePath);
            var lines = content.Split('\n');

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                // C# using statements
                if (trimmed.StartsWith("using ") && trimmed.EndsWith(";"))
                {
                    var ns = trimmed[6..^1].Trim();
                    dependencies.Add(ns);
                }
                // JavaScript/TypeScript imports
                else if (trimmed.StartsWith("import ") || trimmed.StartsWith("const ") && trimmed.Contains("require("))
                {
                    dependencies.Add(trimmed);
                }
                // Python imports
                else if (trimmed.StartsWith("import ") || trimmed.StartsWith("from "))
                {
                    dependencies.Add(trimmed);
                }
            }
        }
        catch
        {
            // Skip files that can't be read
        }

        return dependencies;
    }

    private static bool IsSourceFile(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext is ".cs" or ".js" or ".ts" or ".py" or ".java" or ".go" or ".rs";
    }
}
