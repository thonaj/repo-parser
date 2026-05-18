using Microsoft.EntityFrameworkCore;
using RepoParser.Core.Entities;
using RepoParser.Core.Enums;
using RepoParser.Core.Interfaces;
using RepoParser.Infrastructure.Data;

namespace RepoParser.Infrastructure.Services;

public class DriftDetectionService : IDriftDetectionService
{
    private readonly RepoDbContext _context;
    private readonly IRepositoryService _repository;

    public DriftDetectionService(RepoDbContext context, IRepositoryService repository)
    {
        _context = context;
        _repository = repository;
    }

    public async Task<List<DriftAlert>> DetectDriftAsync(MethodDefinition currentMethod)
    {
        var alerts = new List<DriftAlert>();

        // Find the previous version of this method in the database
        var previousMethod = _context.MethodDefinitions
            .AsNoTracking()
            .FirstOrDefault(m => m.SourceFileId == currentMethod.SourceFileId
                              && m.Name == currentMethod.Name);

        if (previousMethod is null)
            return alerts;

        // Use the existing method's ID since it's already persisted
        var methodId = previousMethod.Id;

        // Check 1: Signature changed
        if (previousMethod.Signature != currentMethod.Signature)
        {
            alerts.Add(new DriftAlert
            {
                MethodDefinitionId = methodId,
                AlertType = AlertType.SignatureChanged,
                Severity = AlertSeverity.Warning,
                Message = $"Method '{currentMethod.Name}' signature changed",
                OldSignature = previousMethod.Signature,
                NewSignature = currentMethod.Signature,
                SimilarityScore = ComputeSimilarity(previousMethod.Signature, currentMethod.Signature)
            });
        }

        // Check 2: Doc comment is outdated (code changed but doc didn't)
        if (previousMethod.Signature != currentMethod.Signature &&
            previousMethod.DocComment == currentMethod.DocComment &&
            !string.IsNullOrEmpty(previousMethod.DocComment))
        {
            alerts.Add(new DriftAlert
            {
                MethodDefinitionId = methodId,
                AlertType = AlertType.DocOutdated,
                Severity = AlertSeverity.Info,
                Message = $"Documentation for '{currentMethod.Name}' may be outdated — code changed but doc did not",
                OldSignature = previousMethod.Signature,
                NewSignature = currentMethod.Signature,
                SimilarityScore = 0.5
            });
        }

        // Check 3: Doc contradicts code (using embedding similarity if available)
        if (previousMethod.CodeEmbedding is not null && previousMethod.DocEmbedding is not null)
        {
            var similarity = CosineSimilarity(previousMethod.CodeEmbedding, previousMethod.DocEmbedding);
            if (similarity < 0.6)
            {
                alerts.Add(new DriftAlert
                {
                    MethodDefinitionId = methodId,
                    AlertType = AlertType.CodeContradictsDoc,
                    Severity = AlertSeverity.Critical,
                    Message = $"Implementation of '{currentMethod.Name}' contradicts its documentation (similarity: {similarity:P1})",
                    OldSignature = previousMethod.Signature,
                    NewSignature = currentMethod.Signature,
                    SimilarityScore = similarity
                });
            }
        }

        // Save alerts to database
        foreach (var alert in alerts)
        {
            await _repository.AddAlertAsync(alert);
        }

        return alerts;
    }

    public async Task<List<DriftAlert>> DetectDriftBatchAsync(List<MethodDefinition> methods)
    {
        var allAlerts = new List<DriftAlert>();
        foreach (var method in methods)
        {
            var alerts = await DetectDriftAsync(method);
            allAlerts.AddRange(alerts);
        }
        return allAlerts;
    }

    private static double ComputeSimilarity(string oldSig, string newSig)
    {
        // Simple Levenshtein-based similarity for signatures
        var maxLen = Math.Max(oldSig.Length, newSig.Length);
        if (maxLen == 0) return 1.0;
        var distance = LevenshteinDistance(oldSig, newSig);
        return 1.0 - (double)distance / maxLen;
    }

    private static int LevenshteinDistance(string a, string b)
    {
        var matrix = new int[a.Length + 1, b.Length + 1];
        for (int i = 0; i <= a.Length; i++) matrix[i, 0] = i;
        for (int j = 0; j <= b.Length; j++) matrix[0, j] = j;
        for (int i = 1; i <= a.Length; i++)
        for (int j = 1; j <= b.Length; j++)
        {
            var cost = a[i - 1] == b[j - 1] ? 0 : 1;
            matrix[i, j] = Math.Min(
                Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                matrix[i - 1, j - 1] + cost);
        }
        return matrix[a.Length, b.Length];
    }

    private static double CosineSimilarity(byte[] vec1, byte[] vec2)
    {
        // Simple byte-level cosine similarity for embedding comparison
        if (vec1.Length != vec2.Length) return 0.0;

        var dotProduct = 0.0;
        var norm1 = 0.0;
        var norm2 = 0.0;

        for (int i = 0; i < vec1.Length; i++)
        {
            dotProduct += vec1[i] * vec2[i];
            norm1 += vec1[i] * vec1[i];
            norm2 += vec2[i] * vec2[i];
        }

        var magnitude = Math.Sqrt(norm1) * Math.Sqrt(norm2);
        return magnitude == 0 ? 0 : dotProduct / magnitude;
    }
}
