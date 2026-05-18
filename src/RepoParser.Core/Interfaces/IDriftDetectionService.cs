using RepoParser.Core.Entities;

namespace RepoParser.Core.Interfaces;

public interface IDriftDetectionService
{
    Task<List<DriftAlert>> DetectDriftAsync(MethodDefinition currentMethod);
    Task<List<DriftAlert>> DetectDriftBatchAsync(List<MethodDefinition> methods);
}
