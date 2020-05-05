using Shiny.Locations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XFShinyBackground.Services
{
    public class GpsReadingEventArgs : EventArgs
    {
        public IGpsReading Reading { get; }

        public GpsReadingEventArgs(IGpsReading reading)
        {
            Reading = reading;
        }

    }

    public interface IGpsListener
    {
        event EventHandler<GpsReadingEventArgs> OnReadingReceived;
    }

    public class GpsListener : IGpsListener
    {
        public event EventHandler<GpsReadingEventArgs> OnReadingReceived;

        void UpdateReading(IGpsReading reading)
        {
            OnReadingReceived?.Invoke(this, new GpsReadingEventArgs(reading));
        }

        public class LocationDelegate : IGpsDelegate
        {
            IGpsListener _gpsListener;

            public LocationDelegate(IGpsListener gpsListener)
            {
                _gpsListener = gpsListener;
            }

            public Task OnReading(IGpsReading reading)
            {
                (_gpsListener as GpsListener)?.UpdateReading(reading);
                return Task.CompletedTask;
            }
        }
    }
}
