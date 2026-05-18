using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RepoParser.Api.Hubs;
using RepoParser.Core.Entities;
using RepoParser.Core.Interfaces;

namespace RepoParser.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly IRepositoryService _repository;
    private readonly IAstParserService _astParser;
    private readonly IDriftDetectionService _driftDetection;
    private readonly IHubContext<AlertHub> _hubContext;

    public AnalysisController(
        IRepositoryService repository,
        IAstParserService astParser,
        IDriftDetectionService driftDetection,
        IHubContext<AlertHub> hubContext)
    {
        _repository = repository;
        _astParser = astParser;
        _driftDetection = driftDetection;
        _hubContext = hubContext;
    }

    [HttpPost("scan-all")]
    public async Task<ActionResult<ScanAllResponse>> ScanAll([FromBody] ScanAllRequest request)
    {
        if (!Directory.Exists(request.RootPath))
            return BadRequest($"Directory not found: {request.RootPath}");

        var excludedDirs = new[] { "node_modules", "bin", "obj", ".git", ".vs", "dist", "packages" };

        var files = Directory.GetFiles(request.RootPath, "*.*", SearchOption.AllDirectories)
            .Where(f => !excludedDirs.Any(d => f.Contains($"\\{d}\\") || f.Contains($"/{d}/")))
            .Where(f => _astParser.DetectLanguage(f) != "Unknown")
            .ToList();

        var scannedCount = 0;
        var alertCount = 0;
        var errors = new List<string>();

        foreach (var filePath in files)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                var content = await System.IO.File.ReadAllTextAsync(filePath);
                var hash = Convert.ToHexString(
                    System.Security.Cryptography.SHA256.HashData(
                        System.Text.Encoding.UTF8.GetBytes(content)));

                var sourceFile = new SourceFile
                {
                    FilePath = filePath,
                    Language = _astParser.DetectLanguage(filePath),
                    LastModified = fileInfo.LastWriteTimeUtc,
                    ContentHash = hash
                };

                sourceFile = await _repository.AddOrUpdateFileAsync(sourceFile);

                var methods = await _astParser.ParseFileAsync(filePath);
                foreach (var method in methods)
                {
                    method.SourceFileId = sourceFile.Id;
                    await _repository.AddOrUpdateMethodAsync(method);
                }

                var alerts = await _driftDetection.DetectDriftBatchAsync(methods);
                alertCount += alerts.Count;

                foreach (var alert in alerts)
                {
                    await _hubContext.Clients.Group("Alerts")
                        .SendAsync("NewAlert", alert);
                }

                scannedCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"{filePath}: {ex.Message}");
            }
        }

        return Ok(new ScanAllResponse(scannedCount, alertCount, errors));
    }
}

public record ScanAllRequest(string RootPath);
public record ScanAllResponse(int FilesScanned, int AlertsGenerated, List<string>? Errors = null);
