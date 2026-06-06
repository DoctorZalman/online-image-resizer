using Microsoft.AspNetCore.SignalR;

namespace ImageResizer.Hubs;

// - SignalR hub - groups connections by sessionId for targeted progress updates
public class ResizeHub : Hub
{
    // - called automatically when client connects via /hubs/resize
    public override async Task OnConnectedAsync()
    {
        var sessionId = Context.GetHttpContext()?.Items["SessionId"]?.ToString();

        if (!string.IsNullOrEmpty(sessionId))
        {
            // - add connection to session group so worker can target this user
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var sessionId = Context.GetHttpContext()?.Items["SessionId"]?.ToString();

        if (!string.IsNullOrEmpty(sessionId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
