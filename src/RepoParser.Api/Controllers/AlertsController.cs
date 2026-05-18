using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RepoParser.Api.Hubs;
using RepoParser.Core.Entities;
using RepoParser.Core.Interfaces;

namespace RepoParser.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly IRepositoryService _repository;
    private readonly IHubContext<AlertHub> _hubContext;

    public AlertsController(IRepositoryService repository, IHubContext<AlertHub> hubContext)
    {
        _repository = repository;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<DriftAlert>>> GetAll([FromQuery] bool unresolvedOnly = true, [FromQuery] string? rootPath = null)
    {
        var alerts = await _repository.GetAlertsAsync(unresolvedOnly);
        if (!string.IsNullOrEmpty(rootPath))
        {
            var normalizedRoot = rootPath.TrimEnd('\\', '/');
            alerts = alerts.Where(a => 
                a.MethodDefinition?.SourceFile?.FilePath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase) ?? false)
                .ToList();
        }
        return Ok(alerts);
    }

    [HttpPost("{id:int}/resolve")]
    public async Task<ActionResult> Resolve(int id)
    {
        await _repository.ResolveAlertAsync(id);
        await _hubContext.Clients.Group("Alerts").SendAsync("AlertResolved", id);
        return NoContent();
    }
}
