using Shiny;
using Shiny.Locations;
using Shiny.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using XFShinyBackground.Services;

namespace XFShinyBackground
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public MainPageViewModel()
        {
            // shiny doesn't usually manage your viewmodels, so we'll do this for now
            var geofences = ShinyHost.Resolve<IGeofenceManager>();
            var notifications = ShinyHost.Resolve<INotificationManager>();
            var gpsManager = ShinyHost.Resolve<IGpsManager>();
            var gpsListener = ShinyHost.Resolve<IGpsListener>();
            //gpsListener.OnReadingReceived += OnReadingReceived;

            var backgroundJob = ShinyHost.Resolve<Shiny.Jobs.IJobManager>();

            StartGeofence = new Command(async () =>
            {
                // this is really only required on iOS, but do it to be safe
                var access = await notifications.RequestAccess();
                if (access == AccessState.Available)
                {
                    await geofences.StartMonitoring(new GeofenceRegion(
                        "My Home",
                        new Position(35.783403605517925, 139.61851208723849),
                        Distance.FromMeters(200)
                    )
                    {
                        NotifyOnEntry = true,
                        NotifyOnExit = true,
                        SingleUse = true,
                    });
                    //35.7839938,139.6150627
                    await geofences.StartMonitoring(new GeofenceRegion(
                        "Park",
                        new Position(35.7839938, 139.6150627),
                        Distance.FromMeters(200))
                    {
                        NotifyOnEntry = true,
                        NotifyOnExit = true,
                        SingleUse = true,
                    });

                    await notifications.Send(new Notification
                    {
                        Title = "Geo fencing started",
                        Message = $"{DateTimeOffset.Now}",
                    });
                }
            });
            StopGeofence = new Command(async () =>
            {
                if (!(await geofences.GetMonitorRegions()).Any())
                    return;

                await geofences.StopAllMonitoring();

                await notifications.Send(new Notification
                {
                    Title = "Geo fencing stopped",
                    Message = $"{DateTimeOffset.Now}",
                });
            });

            Notify = new Command(async () =>
            {
                await notifications.Send(new Notification
                {
                    Title = "Notyfication from Shiny",
                    Message = $"{DateTimeOffset.Now}",
                });
            });

            IDisposable rx = null;
            StartGPS = new Command(async () =>
            {
                if (gpsManager.IsListening)
                    return;

                rx?.Dispose();
                //Rxを利用した例.
                rx = gpsManager
                    .WhenReading()
                    .Subscribe(async x =>
                    {
                        this.LocationMessage = $"{x.Position.Latitude}, {x.Position.Longitude}";
                        await notifications.Send(new Notification
                        {
                            Title = $"gps periodic {DateTimeOffset.Now}",
                            Message = $" {this.LocationMessage}"
                        });
                    });

                var request = new GpsRequest
                {
                    Interval = TimeSpan.FromSeconds(5),
                    UseBackground = true,
                };
                await gpsManager.StartListener(request);
            });

            StopGPS = new Command(async () =>
            {
                await gpsManager.StopListener();

                LocationMessage = "Stopped";
            });
            
            GetOnceGPS = new Command(async () =>
            {
                if (gpsManager.IsListening)
                    return;
                rx?.Dispose();
                rx = gpsManager
                    .WhenReading()
                    .Take(1)
                    .Subscribe(async x =>
                    {
                        this.LocationMessage = $"{x.Position.Latitude}, {x.Position.Longitude}";
                        await notifications.Send(new Notification
                        {
                            Title = $"gps {DateTimeOffset.Now}",
                            Message = $" {this.LocationMessage}"
                        });
                        await gpsManager.StopListener();
                    });

                var request = new GpsRequest
                {
                    Interval = TimeSpan.FromSeconds(1),
                    UseBackground = true,
                };
                await gpsManager.StartListener(request);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propertyName = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void OnReadingReceived(object sender, GpsReadingEventArgs e)
        {
            LocationMessage = $"{e.Reading.Position.Latitude}, {e.Reading.Position.Longitude}";
            //Debug.WriteLine(LocationMessage);
        }

        public ICommand StartGeofence { get; }
        public ICommand StopGeofence { get; }
        public ICommand Notify { get; }
        public ICommand StartGPS { get; }
        public ICommand StopGPS { get; }
        public ICommand GetOnceGPS { get; }

        private string _locationMessage;
        public string LocationMessage
        {
            get => _locationMessage;
            private set
            {
                _locationMessage = value;
                RaisePropertyChanged();
            }
        }
    }
}
