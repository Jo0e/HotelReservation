using Microsoft.AspNetCore.Builder;/////
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Utilities.Utility;
using Microsoft.Owin;
using Owin;

namespace HotelReservation
{
    public class Startup 
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add SignalR services
            services.AddSignalR();

            // Add MVC services if you have controllers or views
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // Map SignalR hub
                endpoints.MapHub<HotelHub>("/hotelHub");

                // Map other endpoints (like controllers)
                endpoints.MapControllers();
            });
        }
    }
}
