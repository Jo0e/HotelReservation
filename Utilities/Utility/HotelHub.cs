
using Microsoft.AspNetCore.SignalR;
using Models.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Utility
{
    public class HotelHub : Hub
    {

        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task HotelCreated(Hotel hotel)
        {
            await Clients.All.SendAsync("HotelCreated", hotel);
        }

        public async Task HotelUpdated(Hotel hotel)
        {
            await Clients.All.SendAsync("HotelUpdated", hotel);
        }

        public async Task HotelDeleted(int id)
        {
            await Clients.All.SendAsync("HotelDeleted", id);
        }
    }

   
}
