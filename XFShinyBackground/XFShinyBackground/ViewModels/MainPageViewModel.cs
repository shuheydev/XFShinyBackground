using Shiny;
using Shiny.Locations;
using Shiny.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

            Notify = new Command(async () =>
            {
                await notifications.Send(new Notification
                {
                    Title = "Notyfication from Shiny",
                    Message = $"{DateTimeOffset.Now}",
                });
            });

            StartGPS = new Command(async () =>
            {
                if (gpsManager.IsListening)
                    return;

                //Rxを利用した例.
                gpsManager
                    .WhenReading()
                    .Subscribe(x =>
                    {
                        this.LocationMessage = $"{x.Position.Latitude}, {x.Position.Longitude}";
                        notifications.Send(new Notification
                        {
                            Title = $"gps {DateTimeOffset.Now}",
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

            GetOnceGPS = new Command(async () => {
                if (gpsManager.IsListening)
                    return;

                gpsManager
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

        public ICommand Register { get; }
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
