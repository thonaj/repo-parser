using Microsoft.AspNetCore.Mvc;
using RepoParser.Core.Interfaces;

namespace RepoParser.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IRepositoryService _repository;
    private readonly IAstParserService _astParser;
    private readonly IDriftDetectionService _driftDetection;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        IRepositoryService repository,
        IAstParserService astParser,
        IDriftDetectionService driftDetection,
        ILogger<WebhookController> logger)
    {
        _repository = repository;
        _astParser = astParser;
        _driftDetection = driftDetection;
        _logger = logger;
    }

    [HttpPost("github")]
    public ActionResult HandleGitHubWebhook([FromBody] GitHubWebhookPayload payload)
    {
        _logger.LogInformation("Received GitHub webhook: {Action} on {Repository}", 
            payload.Action, payload.Repository?.FullName);

        // Process changed files from the push event
        if (payload.Commits is not null)
        {
            foreach (var commit in payload.Commits)
            {
                var allFiles = (commit.Added ?? new List<string>())
                    .Concat(commit.Modified ?? new List<string>())
                    .Distinct();

                foreach (var filePath in allFiles)
                {
                    var fullPath = Path.Combine(payload.Repository?.FullName ?? "", filePath);
                    _logger.LogInformation("File changed: {FilePath}", fullPath);
                }
            }
        }

        return Ok(new { message = "Webhook received" });
    }
}

public record GitHubWebhookPayload(
    string Action,
    GitHubRepository? Repository,
    List<GitHubCommit>? Commits
);

public record GitHubRepository(string FullName, string CloneUrl);
public record GitHubCommit(string Id, string Message, List<string> Added, List<string> Modified, List<string> Removed);
