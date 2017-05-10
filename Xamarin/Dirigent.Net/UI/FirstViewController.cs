using System;
using Google.Maps;
using UIKit;

namespace Dirigent.Net.UI {
	public partial class FirstViewController : UIViewController {
		private MapView mapView;

		protected FirstViewController(IntPtr handle) : base(handle) {
		}

		public override void ViewDidLoad() {
			base.ViewDidLoad();

			if (mapView == null) {
				var cameraPosition = CameraPosition.FromCamera(latitude: 56.948889, longitude: 24.106389, zoom: 5);
				mapView = MapView.FromCamera(View.Bounds, cameraPosition);
				mapView.MyLocationEnabled = true;

				View.AutosizesSubviews = true;
				View.AutoresizingMask = UIViewAutoresizing.All;
				View.AddSubview(mapView);
				View.SendSubviewToBack(mapView);
			}
		}

		public override void ViewWillAppear(bool animated) {
			base.ViewWillAppear(animated);
			mapView.StartRendering();
		}

		public override void ViewWillDisappear(bool animated) {
			mapView.StopRendering();
			base.ViewWillDisappear(animated);
		}
	}
}