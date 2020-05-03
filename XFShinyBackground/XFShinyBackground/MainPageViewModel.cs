using Shiny;
using Shiny.Locations;
using Shiny.Notifications;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace XFShinyBackground
{
    public class MainPageViewModel
    {
        public MainPageViewModel()
        {
            // shiny doesn't usually manage your viewmodels, so we'll do this for now
            var geofences = ShinyHost.Resolve<IGeofenceManager>();
            var notifications = ShinyHost.Resolve<INotificationManager>();

            Register = new Command(async () =>
            {
                // this is really only required on iOS, but do it to be safe
                var access = await notifications.RequestAccess();
                if (access == AccessState.Available)
                {
                    await geofences.StartMonitoring(new GeofenceRegion(
                        "CN Tower - Toronto, Canada",
                        new Position(35.7816473, 139.6182211),
                        Distance.FromMeters(200)
                    )
                    {
                        NotifyOnEntry = true,
                        NotifyOnExit = true,
                        SingleUse = false
                    });
                }
            });
        }

        public ICommand Register { get; }
    }
}
