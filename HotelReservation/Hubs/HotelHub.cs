using Microsoft.AspNetCore.SignalR;

namespace HotelReservation.Hubs
{
    public class HotelHub : Hub
    {
        public async Task NotifyNewHotel(string hotelJson)
        {
            await Clients.All.SendAsync("NewHotelAdded", hotelJson);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
