using System;
using CoreLocation;

namespace Dirigent.iOS.Services {
	public interface ILocationService : IDisposable {
		event LocationChangedEventHandler LocationChanged;
		void StartMonitorLocation();
		void StopMonitorLocation();
	}

	public class LocationChangedEventArgs : EventArgs {
		public CLLocation Location { get; private set; }

		public LocationChangedEventArgs(CLLocation location) {
			Location = location;
		}
	}

	public delegate void LocationChangedEventHandler(object sender, LocationChangedEventArgs e);
}