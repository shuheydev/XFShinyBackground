using Shiny.Locations;
using Shiny.Notifications;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XFShinyBackground
{
    public class MyGeofenceDelegate : IGeofenceDelegate
    {
        private readonly INotificationManager _notifications;

        public string Test = "Hello";

        public MyGeofenceDelegate(INotificationManager notifications)
        {
            this._notifications = notifications;
        }     

        public async Task OnStatusChanged(GeofenceState newStatus, GeofenceRegion region)
        {
            if (newStatus == GeofenceState.Entered)
            {
                await this._notifications.Send(new Notification
                {
                    Title = "WELCOME!",
                    Message = "It is good to have you back " + region.Identifier
                });
            }
            else
            {
                await this._notifications.Send(new Notification
                {
                    Title = "GOODBYE!",
                    Message = "You will be missed at " + region.Identifier
                });
            }
        }
    }
}
