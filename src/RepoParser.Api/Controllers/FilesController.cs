using Microsoft.AspNetCore.Mvc;
using RepoParser.Core.Entities;
using RepoParser.Core.Interfaces;

namespace RepoParser.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IRepositoryService _repository;
    private readonly IAstParserService _astParser;

    public FilesController(IRepositoryService repository, IAstParserService astParser)
    {
        _repository = repository;
        _astParser = astParser;
    }

    [HttpGet]
    public async Task<ActionResult<List<SourceFile>>> GetAll([FromQuery] string? rootPath = null)
    {
        var files = await _repository.GetAllFilesAsync();
        if (!string.IsNullOrEmpty(rootPath))
        {
            var normalizedRoot = rootPath.TrimEnd('\\', '/');
            files = files.Where(f => f.FilePath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        return Ok(files);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SourceFile>> GetById(int id)
    {
        var files = await _repository.GetAllFilesAsync();
        var file = files.FirstOrDefault(f => f.Id == id);
        if (file is null) return NotFound();
        return Ok(file);
    }

    [HttpGet("by-path")]
    public async Task<ActionResult<SourceFile>> GetByPath([FromQuery] string path)
    {
        var file = await _repository.GetFileByPathAsync(path);
        if (file is null) return NotFound();
        return Ok(file);
    }

    [HttpGet("{id:int}/methods")]
    public async Task<ActionResult<List<MethodDefinition>>> GetMethods(int id)
    {
        var methods = await _repository.GetMethodsForFileAsync(id);
        return Ok(methods);
    }

    [HttpPost("scan")]
    public async Task<ActionResult<SourceFile>> ScanFile([FromBody] ScanRequest request)
    {
        if (!System.IO.File.Exists(request.FilePath))
            return BadRequest($"File not found: {request.FilePath}");

        var fileInfo = new FileInfo(request.FilePath);
        var content = await System.IO.File.ReadAllTextAsync(request.FilePath);
        var hash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(content)));

        var sourceFile = new SourceFile
        {
            FilePath = request.FilePath,
            Language = _astParser.DetectLanguage(request.FilePath),
            LastModified = fileInfo.LastWriteTimeUtc,
            ContentHash = hash
        };

        sourceFile = await _repository.AddOrUpdateFileAsync(sourceFile);

        var methods = await _astParser.ParseFileAsync(request.FilePath);
        foreach (var method in methods)
        {
            method.SourceFileId = sourceFile.Id;
            await _repository.AddOrUpdateMethodAsync(method);
        }

        return Ok(sourceFile);
    }
}

public record ScanRequest(string FilePath);
