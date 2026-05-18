using RepoParser.Core.Entities;

namespace RepoParser.Core.Interfaces;

public interface IRepositoryService
{
    Task<SourceFile?> GetFileByPathAsync(string filePath);
    Task<List<SourceFile>> GetAllFilesAsync();
    Task<SourceFile> AddOrUpdateFileAsync(SourceFile file);
    Task DeleteFileAsync(int fileId);
    Task<List<MethodDefinition>> GetMethodsForFileAsync(int fileId);
    Task<MethodDefinition> AddOrUpdateMethodAsync(MethodDefinition method);
    Task<List<DriftAlert>> GetAlertsAsync(bool unresolvedOnly = true);
    Task<DriftAlert> AddAlertAsync(DriftAlert alert);
    Task ResolveAlertAsync(int alertId);
    Task<List<DependencyEdge>> GetDependenciesAsync(int fileId);
    Task AddDependencyEdgeAsync(DependencyEdge edge);
}
