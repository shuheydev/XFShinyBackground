using Microsoft.Extensions.DependencyInjection;
using Shiny;
using Shiny.Locations;
using System;
using System.Collections.Generic;
using System.Text;
using XFShinyBackground.Services;
using static XFShinyBackground.Services.GpsListener;

namespace XFShinyBackground
{
    public class MyStartup : Shiny.ShinyStartup
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.UseGeofencing<MyGeofenceDelegate>(true);
            services.UseNotifications(true);
            services.UseGps<LocationDelegate>();
            services.AddSingleton<IGpsListener, GpsListener>();
        }
    }
}
