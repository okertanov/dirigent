using CoreLocation;
using Dirigent.Common.Core;
using Dirigent.iOS.Logging;
using Foundation;

namespace Dirigent.iOS.Services.Impl {
	public class LocationService : ILocationService {
		private CLLocationManager locationManager;
		private static readonly Logger Logger = LogManager.GetLogger<LocationService>();

		public event LocationChangedEventHandler LocationChanged;

		public LocationService() {
			locationManager = new CLLocationManager() {
				Delegate = new LocationManagerDelegate(this)
			};
		}

		public void StartMonitorLocation() {
			locationManager.StartUpdatingHeading();
			locationManager.StartUpdatingLocation();
			locationManager.StartMonitoringSignificantLocationChanges();
		}

		public void StopMonitorLocation() {
			locationManager.StopUpdatingHeading();
			locationManager.StopUpdatingLocation();
			locationManager.StopMonitoringSignificantLocationChanges();
		}

		public void Dispose() {
			if (locationManager != null) {
				locationManager.Dispose();
				locationManager = null;
			}
		}

		internal class LocationManagerDelegate : CLLocationManagerDelegate {
			private readonly WeakReferenceEx<LocationService> parentRef;

			public LocationManagerDelegate(LocationService parent) {
				parentRef = WeakReferenceEx.Create(parent);
			}

			public override void Failed(CLLocationManager manager, NSError error) {
				LocationService.Logger.Warn("Failed: {0}-{1}-{2}", error.Domain, error.Code, error.LocalizedDescription);
			}

			public override void UpdatedHeading(CLLocationManager manager, CLHeading newHeading) {
				//LocationService.Logger.Debug("Updated Heading: {0}", newHeading);
			}

			public override void UpdatedLocation(CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation) {
				//LocationService.Logger.Debug("Updated Location: from: '{0}' to '{1}'", oldLocation, newLocation);
				if (parentRef.Target.LocationChanged != null) {
					parentRef.Target.LocationChanged.Invoke(parentRef.Target, new LocationChangedEventArgs(newLocation));
				}
			}
		}
	}
}