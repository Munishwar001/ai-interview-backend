using AIInterview.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AIInterview.Server.Hubs
{
    [Authorize]
    public class InterviewHub : Hub
    {
        private readonly IApplicationRepository _applicationRepository;

        public InterviewHub(IApplicationRepository applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }

        private static string GroupName(int interviewId) => $"interview-{interviewId}";

        public async Task JoinInterview(int interviewId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                throw new HubException("Unauthorized user.");

            var canAccess = await _applicationRepository.CanAccessInterviewAsync(interviewId, userId);
            if (!canAccess)
                throw new HubException("You are not allowed to join this interview.");

            var group = GroupName(interviewId);
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await Clients.OthersInGroup(group).SendAsync("ParticipantJoined", userId);
              // Notify others that someone joined
             await Clients.OthersInGroup(group).SendAsync("ParticipantJoined", userId);
    
            // Confirm to the caller they joined successfully
            await Clients.Caller.SendAsync("JoinedInterview", interviewId);
        }

        public async Task LeaveInterview(int interviewId)
        {
            var group = GroupName(interviewId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        }

        public Task SendOffer(int interviewId, string sdp)
            => Clients.OthersInGroup(GroupName(interviewId)).SendAsync("ReceiveOffer", sdp);

        public Task SendAnswer(int interviewId, string sdp)
            => Clients.OthersInGroup(GroupName(interviewId)).SendAsync("ReceiveAnswer", sdp);

        public Task SendIceCandidate(int interviewId, string candidate)
            => Clients.OthersInGroup(GroupName(interviewId)).SendAsync("ReceiveIceCandidate", candidate);
    }
}
