using RepoParser.Core.Entities;

namespace RepoParser.Core.Interfaces;

public interface IDependencyGraphService
{
    Task<List<DependencyEdge>> BuildGraphAsync(string rootPath);
    Task<List<DependencyEdge>> GetDependenciesAsync(int sourceFileId);
}
