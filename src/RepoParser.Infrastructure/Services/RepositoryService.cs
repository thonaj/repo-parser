using Microsoft.EntityFrameworkCore;
using RepoParser.Core.Entities;
using RepoParser.Core.Interfaces;
using RepoParser.Infrastructure.Data;

namespace RepoParser.Infrastructure.Services;

public class RepositoryService : IRepositoryService
{
    private readonly RepoDbContext _context;

    public RepositoryService(RepoDbContext context)
    {
        _context = context;
    }

    public async Task<SourceFile?> GetFileByPathAsync(string filePath)
    {
        return await _context.SourceFiles
            .Include(s => s.Methods)
            .FirstOrDefaultAsync(s => s.FilePath == filePath);
    }

    public async Task<List<SourceFile>> GetAllFilesAsync()
    {
        return await _context.SourceFiles
            .Include(s => s.Methods)
            .ToListAsync();
    }

    public async Task<SourceFile> AddOrUpdateFileAsync(SourceFile file)
    {
        var existing = await _context.SourceFiles
            .FirstOrDefaultAsync(s => s.FilePath == file.FilePath);

        if (existing is not null)
        {
            existing.LastModified = file.LastModified;
            existing.ContentHash = file.ContentHash;
            existing.Language = file.Language;
            return existing;
        }

        _context.SourceFiles.Add(file);
        await _context.SaveChangesAsync();
        return file;
    }

    public async Task DeleteFileAsync(int fileId)
    {
        var file = await _context.SourceFiles.FindAsync(fileId);
        if (file is not null)
        {
            _context.SourceFiles.Remove(file);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<MethodDefinition>> GetMethodsForFileAsync(int fileId)
    {
        return await _context.MethodDefinitions
            .Where(m => m.SourceFileId == fileId)
            .Include(m => m.Alerts)
            .ToListAsync();
    }

    public async Task<MethodDefinition> AddOrUpdateMethodAsync(MethodDefinition method)
    {
        var existing = await _context.MethodDefinitions
            .FirstOrDefaultAsync(m => m.SourceFileId == method.SourceFileId && m.Name == method.Name);

        if (existing is not null)
        {
            existing.Signature = method.Signature;
            existing.Parameters = method.Parameters;
            existing.ReturnType = method.ReturnType;
            existing.DocComment = method.DocComment;
            existing.LineStart = method.LineStart;
            existing.LineEnd = method.LineEnd;
            existing.CodeEmbedding = method.CodeEmbedding;
            existing.DocEmbedding = method.DocEmbedding;
            existing.LastEmbedded = method.LastEmbedded;
            return existing;
        }

        _context.MethodDefinitions.Add(method);
        await _context.SaveChangesAsync();
        return method;
    }

    public async Task<List<DriftAlert>> GetAlertsAsync(bool unresolvedOnly = true)
    {
        var query = _context.DriftAlerts
            .Include(a => a.MethodDefinition)
            .ThenInclude(m => m.SourceFile)
            .AsQueryable();

        if (unresolvedOnly)
            query = query.Where(a => !a.IsResolved);

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    public async Task<DriftAlert> AddAlertAsync(DriftAlert alert)
    {
        _context.DriftAlerts.Add(alert);
        await _context.SaveChangesAsync();
        return alert;
    }

    public async Task ResolveAlertAsync(int alertId)
    {
        var alert = await _context.DriftAlerts.FindAsync(alertId);
        if (alert is not null)
        {
            alert.IsResolved = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<DependencyEdge>> GetDependenciesAsync(int fileId)
    {
        return await _context.DependencyEdges
            .Where(e => e.SourceFileId == fileId)
            .ToListAsync();
    }

    public async Task AddDependencyEdgeAsync(DependencyEdge edge)
    {
        _context.DependencyEdges.Add(edge);
        await _context.SaveChangesAsync();
    }
}
