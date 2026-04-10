using AIInterview.Application.Interface;
using AIInterview.Core.DTOs.Job;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AIInterview.Server.Hubs
{
    [Authorize]
    public class ApplicationChatHub : Hub
    {
        private readonly IApplicationRepository _applicationRepository;

        public ApplicationChatHub(IApplicationRepository applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }

        private static string GroupName(int applicationId) => $"application-chat-{applicationId}";

        public async Task JoinApplicationChat(int applicationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                throw new HubException("Unauthorized user.");

            var canAccess = await _applicationRepository.CanAccessApplicationChatAsync(applicationId, userId);
            if (!canAccess)
                throw new HubException("You are not allowed to join this chat.");

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(applicationId));
            await Clients.Caller.SendAsync("JoinedApplicationChat", applicationId);
        }

        public async Task LeaveApplicationChat(int applicationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(applicationId));
        }

        public async Task SendMessage(int applicationId, string message)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                throw new HubException("Unauthorized user.");

            if (string.IsNullOrWhiteSpace(message))
                throw new HubException("Message cannot be empty.");

            var created = await _applicationRepository.AddChatMessageAsync(applicationId, userId, message);
            if (created == null)
                throw new HubException("You are not allowed to send messages in this chat.");

            await Clients.Group(GroupName(applicationId)).SendAsync("ReceiveMessage", created);
        }
    }
}
