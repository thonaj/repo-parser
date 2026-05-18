using Microsoft.AspNetCore.SignalR;

namespace RepoParser.Api.Hubs;

public class AlertHub : Hub
{
    public async Task JoinAlertGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Alerts");
    }

    public async Task LeaveAlertGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Alerts");
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        await Groups.AddToGroupAsync(Context.ConnectionId, "Alerts");
    }
}
