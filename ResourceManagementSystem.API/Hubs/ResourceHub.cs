using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using ResourceManagementSystem.Core.DTOs.Resource; // Dla ResourceDto
using System;
using Microsoft.AspNetCore.Authorization;

namespace ResourceManagementSystem.API.Hubs
{
    
    [Authorize]
    public class ResourceHub : Hub
    {
        public async Task SendResourceUpdateMessage(ResourceDto resource)
        {
            await Clients.All.SendAsync("ReceiveResourceUpdate", resource);
        }

        public async Task SendResourceCreatedMessage(ResourceDto resource)
        {
            await Clients.All.SendAsync("ReceiveResourceCreated", resource);
        }

        public async Task SendResourceDeletedMessage(Guid resourceId)
        {
            await Clients.All.SendAsync("ReceiveResourceDeleted", resourceId);
        }
        
        public override async Task OnConnectedAsync()
        {
            
            await Clients.Caller.SendAsync("WelcomeMessage", "Successfully connected to ResourceHub!");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}