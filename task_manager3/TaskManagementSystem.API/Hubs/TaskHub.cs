using Microsoft.AspNetCore.SignalR;

namespace TaskManagementSystem.API.Hubs
{
    public class TaskHub : Hub
    {
        // Called when a client wants to subscribe to a specific task's updates.
        public async Task SubscribeToTask(int taskId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Task_{taskId}");
        }
        
        // Called when a client wants to unsubscribe.
        public async Task UnsubscribeFromTask(int taskId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Task_{taskId}");
        }
    }
}
