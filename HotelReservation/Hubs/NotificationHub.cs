using Microsoft.AspNetCore.SignalR;
using Utilities.Utility;

namespace HotelReservation.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task NotifyNewHotel(string hotelJson)
        {
            await Clients.All.SendAsync("NewHotelNotification", hotelJson);
        }
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            if (user != null && user.IsInRole(SD.AdminRole))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task NotifyAdmin(string contactUsInfo)
        {
            await Clients.Group("Admins").SendAsync("AdminNotification", contactUsInfo);
        }
    }
}
