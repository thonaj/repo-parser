using Microsoft.AspNetCore.Mvc;
using RepoParser.Core.Entities;
using RepoParser.Core.Interfaces;

namespace RepoParser.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GraphController : ControllerBase
{
    private readonly IDependencyGraphService _graphService;
    private readonly IRepositoryService _repository;

    public GraphController(IDependencyGraphService graphService, IRepositoryService repository)
    {
        _graphService = graphService;
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<GraphResponse>> GetGraph()
    {
        var files = await _repository.GetAllFilesAsync();
        var allEdges = new List<DependencyEdge>();

        foreach (var file in files)
        {
            var fileEdges = await _graphService.GetDependenciesAsync(file.Id);
            allEdges.AddRange(fileEdges);
        }

        var nodes = files.Select(f => new GraphNode(
            f.Id,
            Path.GetFileName(f.FilePath),
            f.FilePath,
            f.Language,
            f.Methods.Count
        )).ToList();

        var edges = allEdges.Select(e => new GraphEdge(
            e.SourceFileId,
            e.TargetFilePath,
            e.DependencyType
        )).ToList();

        return Ok(new GraphResponse(nodes, edges));
    }

    [HttpPost("build")]
    public async Task<ActionResult> BuildGraph([FromBody] BuildGraphRequest request)
    {
        if (!Directory.Exists(request.RootPath))
            return BadRequest($"Directory not found: {request.RootPath}");

        await _graphService.BuildGraphAsync(request.RootPath);
        return Ok(new { message = "Graph built successfully" });
    }
}

public record GraphResponse(List<GraphNode> Nodes, List<GraphEdge> Edges);
public record GraphNode(int Id, string Label, string FilePath, string Language, int MethodCount);
public record GraphEdge(int From, string To, string Type);
public record BuildGraphRequest(string RootPath);
