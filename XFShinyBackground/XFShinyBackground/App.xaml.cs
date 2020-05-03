using Shiny;
using Shiny.Notifications;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XFShinyBackground
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override async void OnStart()
        {
            await SendNotificationNow();
            await ScheduleLocalNotification(DateTimeOffset.Now.AddMinutes(1));
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        Task SendNotificationNow()
        {
            var notification = new Notification
            {
                Title = "Testing Local Notifications",
                Message = "It's working",
            };

            return ShinyHost.Resolve<INotificationManager>().RequestAccessAndSend(notification);
        }

        Task ScheduleLocalNotification(DateTimeOffset scheduledTime)
        {
            var notification = new Notification
            {
                Title = "Testing Local Notifications",
                Message = "It's working",
                ScheduleDate = scheduledTime
            };

            return ShinyHost.Resolve<INotificationManager>().Send(notification);
        }
    }
}
