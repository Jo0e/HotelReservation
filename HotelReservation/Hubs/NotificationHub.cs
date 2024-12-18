using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Utilities.Utility;

namespace HotelReservation.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly Dictionary<string, string> UserConnections = new Dictionary<string, string>();

        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            if (user != null)
            {
                var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    UserConnections[userId] = Context.ConnectionId;

                    if (user.IsInRole(SD.AdminRole))
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                    }
                    if (user.IsInRole(SD.CustomerRole))
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, "Customers");
                    }
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = Context.User;
            if (user != null)
            {
                var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    UserConnections.Remove(userId);

                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Customers");
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task NotifyAdmin(string contactUsInfo)
        {
            await Clients.Group("Admins").SendAsync("AdminNotification", contactUsInfo);
        }

        public async Task NotifyAdminReservation(string reservationInfo)
        {
            await Clients.Group("Admins").SendAsync("AdminNotification", reservationInfo);
        }

        public async Task NotifyUser(string messageInfo,int messageCount, string userId)

        {
            if (UserConnections.TryGetValue(userId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("CustomerNotification", messageInfo, messageCount);
            }
        }
    }

}
