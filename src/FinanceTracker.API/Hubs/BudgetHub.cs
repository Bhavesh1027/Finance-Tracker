using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FinanceTracker.API.Hubs;

[Authorize]
public sealed class BudgetHub : Hub
{
    /// <summary>
    /// Automatically join the user's group on connection based on their user identifier.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("userId")?.Value;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Allows clients to explicitly join a user group for notifications.
    /// </summary>
    public Task JoinUserGroup(string userId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, userId);
    }
}

