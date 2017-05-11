using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using CoreLocation;
using Dirigent.Net.Logging;
using Dirigent.Net.Sys.Helpers;
using Dirigent.Net.UI.Components;
using Foundation;
using Google.Maps;
using UIKit;

namespace Dirigent.Net.UI {
	public partial class FirstViewController : UIViewController {
		private const float DefaultZoom = 16f;

		private ActionSheet actionSheet;
		private MapView mapView;
		private Geocoder geocoder;

		private static readonly Logger Logger = LogManager.GetLogger<FirstViewController>();

		protected FirstViewController(IntPtr handle) : base(handle) {
		}

		public override void ViewDidLoad() {
			base.ViewDidLoad();

			if (mapView == null) {
				geocoder = Geocoder.SharedGeocoder;
				mapView = CreateMapView(latitude: 56.948889, longitude: 24.106389, zoom: 5).Result;

				View.AutosizesSubviews = true;
				View.AutoresizingMask = UIViewAutoresizing.All;
				View.TranslatesAutoresizingMaskIntoConstraints = true;
				View.AddSubview(mapView);
				View.SendSubviewToBack(mapView);

				actionSheet = new ActionSheet("Actions", "Close", new [] {
					new ActionSheetItem("Zoom to my Location", OnZoomMyLocation),
					new ActionSheetItem("Navigate by Photo", OnNavigateByPhoto),
					new ActionSheetItem("Bookmark this Place", OnBookmarkPlace),
					new ActionSheetItem("Route Tracking", OnRouteTracking),
					new ActionSheetItem("Change Map Type", OnChangeMapType),
					new ActionSheetItem("Open Google Maps", OnOpenGoogleMaps)
				});

				AddImageButton.TouchUpInside += OnAddImageButtonTouchUpInside;
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

		public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator) {
			base.ViewWillTransitionToSize(toSize, coordinator);
			if (mapView != null && mapView.Handle != IntPtr.Zero) {
				mapView.Frame = new CGRect(View.Bounds.Location, toSize);
				View.LayoutIfNeeded();
			}
		}

		private async void OnAddImageButtonTouchUpInside(object sender, EventArgs args) {
			await actionSheet.Present(sender as UIView).ContinueWith(async t => {
				if (!t.IsCanceled && !t.IsFaulted && t.IsCompleted) {
					await t.Result.Handler(t.Result);
				}
				else {
					Logger.Debug("Menu item '{0}'", t.IsCanceled ? "cancelled" : "faulted");
				}
			});
		}

		private async Task OnZoomMyLocation(ActionSheetItem arg) {
			Logger.Debug("Menu item selected: '{0}'", arg.Title);
			var location = mapView.MyLocation;
			await ZoomToLocation(location.Coordinate.Latitude, location.Coordinate.Longitude, DefaultZoom);
		}

		private async Task OnNavigateByPhoto(ActionSheetItem arg) {
			Logger.Debug("Menu item selected: '{0}'", arg.Title);
		}

		private async Task OnBookmarkPlace(ActionSheetItem arg) {
			Logger.Debug("Menu item selected: '{0}'", arg.Title);
		}

		private async Task OnRouteTracking(ActionSheetItem arg) {
			Logger.Debug("Menu item selected: '{0}'", arg.Title);
		}

		private async Task OnChangeMapType(ActionSheetItem arg) {
			Logger.Debug("Menu item selected: '{0}'", arg.Title);
			var oldMapType = mapView.MapType;
			var newMapType = (MapViewType)((int)oldMapType + 1);
			if ((int)newMapType > Enum.GetValues(typeof(MapViewType)).Length) {
				newMapType = MapViewType.Normal;
			}
			ThreadUtils.SafeBeginInvokeOnMainThread(async () => {
				mapView.MapType = newMapType;
				var hud = new HeadUpDisplay();
				await hud.Present(View, newMapType.ToString(), (new CancellationTokenSource(3000)).Token);
			});

			Logger.Debug("Map type changed from {0} to {1}", oldMapType, newMapType);
		}

		private async Task OnOpenGoogleMaps(ActionSheetItem arg) {
			Logger.Debug("Menu item selected: '{0}'", arg.Title);
			ThreadUtils.SafeBeginInvokeOnMainThread(OnOpenGoogleMapsImpl);
		}

		private void OnOpenGoogleMapsImpl() {
			if (UIApplication.SharedApplication.CanOpenUrl(new NSUrl("comgooglemaps://"))) {
				var urlStr = String.Format("comgooglemaps://?center={0},{1}&zoom={2}",
					mapView.Layer.CameraLatitude, mapView.Layer.CameraLongitude, mapView.Layer.CameraZoomLevel);
				var url = new NSUrl(urlStr);
				UIApplication.SharedApplication.OpenUrl(url);
			}
			else {
				Logger.Warn("Can't open Google Maps");
			}
		}

		private Task<MapView> CreateMapView(double latitude, double longitude, float zoom) {
			var cameraPosition = CameraPosition.FromCamera(latitude:latitude, longitude: longitude, zoom: zoom);
			var cameraMapView = MapView.FromCamera(View.Bounds, cameraPosition);

			cameraMapView.CameraPositionChanged += OnCameraPositionChanged;
			cameraMapView.CameraPositionIdle += OnCameraPositionIdle;
			cameraMapView.TileRenderingStarted += OnTileRenderingStarted;
			cameraMapView.TileRenderingEnded += OnTileRenderingEnded;
			cameraMapView.DidTapMyLocationButton += OnDidTapMyLocationButton;
			cameraMapView.OverlayTapped += OnOverlayTapped;
			cameraMapView.CoordinateTapped += OnCoordinateTapped;
			cameraMapView.CoordinateLongPressed += OnCoordinateLongPressed;
			cameraMapView.PoiWithPlaceIdTapped += OnPoiWithPlaceIdTapped;

			cameraMapView.MapType = MapViewType.Normal;
			cameraMapView.MyLocationEnabled = true;
			cameraMapView.Settings.CompassButton = true;
			cameraMapView.Settings.MyLocationButton = true;
			cameraMapView.Settings.SetAllGesturesEnabled(true);

			return Task.FromResult(cameraMapView);
		}

		private Task ZoomToLocation(double latitude, double longitude, float zoom) {
			var cameraPosition = CameraPosition.FromCamera(latitude, longitude, zoom);
			var cameraUpdate = CameraUpdate.SetCamera(cameraPosition);
			ThreadUtils.SafeBeginInvokeOnMainThread(() => mapView.Animate(cameraUpdate));
			return Task.CompletedTask;
		}

		private Task<Tuple<string, string>> GetReverseGeocodingInfo(CLLocationCoordinate2D coord) {
			var tcs = new TaskCompletionSource<Tuple<string, string>>();
			geocoder.ReverseGeocodeCord(coord, (response, error) => {
				if (error != null) {
					tcs.TrySetException(new Exception(error.LocalizedDescription));
					return;
				}
				var title = response?.FirstResult?.Thoroughfare ?? String.Empty;
				var description = String.Join(", ", response?.FirstResult?.Lines ?? Enumerable.Empty<string>());
				tcs.TrySetResult(Tuple.Create(title, description));
			});
			return tcs.Task;
		}

		private async void OnCameraPositionChanged(object sender, GMSCameraEventArgs args) {
			//var info = await GetReverseGeocodingInfo(args.Position.Target);
			//Logger.Debug("Camera position changed: {0},{1} (z:{2},b:{3},a:{4})\n\t'{5}'", args.Position.Target.Latitude, args.Position.Target.Longitude, args.Position.Zoom, args.Position.Bearing, args.Position.ViewingAngle, info.Item2);
		}

		private async void OnCameraPositionIdle(object sender, GMSCameraEventArgs args) {
			var info = await GetReverseGeocodingInfo(args.Position.Target);
			Logger.Debug("Camera position idle: {0},{1} (z:{2},b:{3},a:{4})\n\t'{5}'", args.Position.Target.Latitude, args.Position.Target.Longitude, args.Position.Zoom, args.Position.Bearing, args.Position.ViewingAngle, info.Item2);
		}

		private void OnTileRenderingStarted(object sender, EventArgs args) {
			Logger.Debug("Tile rendering started");
		}

		private void OnTileRenderingEnded(object sender, EventArgs args) {
			Logger.Debug("Tile rendering ended");
		}

		private bool OnDidTapMyLocationButton(MapView view) {
			Logger.Debug("My Location button tapped");
			return true;
		}

		private void OnOverlayTapped(object sender, GMSOverlayEventEventArgs args) {
			Logger.Debug("Overlay tapped: {0}", args.Overlay.Title);
		}

		private async void OnCoordinateTapped(object sender, GMSCoordEventArgs args) {
			var info = await GetReverseGeocodingInfo(args.Coordinate);
			Logger.Debug("Coordinate tapped: {0},{1}\n\t'{2}'", args.Coordinate.Latitude, args.Coordinate.Longitude, info.Item2);
		}

		private async void OnCoordinateLongPressed(object sender, GMSCoordEventArgs args) {
			var info = await GetReverseGeocodingInfo(args.Coordinate);
			Logger.Debug("Coordinate pressed: {0},{1}\n\t'{2}'", args.Coordinate.Latitude, args.Coordinate.Longitude, info.Item2);
		}

		private async void OnPoiWithPlaceIdTapped(object sender, GMSPoiWithPlaceIdEventEventArgs args) {
			var info = await GetReverseGeocodingInfo(args.Location);
			Logger.Debug("POI Tapped: '{0} - {1}' {2}\n\t'{3}'", args.Name, args.PlaceId, args.Location, info.Item2);
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (mapView != null && mapView.Handle != IntPtr.Zero) {
					mapView.CameraPositionChanged -= OnCameraPositionChanged;
					mapView.CameraPositionIdle -= OnCameraPositionIdle;
					mapView.TileRenderingStarted -= OnTileRenderingStarted;
					mapView.TileRenderingEnded -= OnTileRenderingEnded;
					mapView.DidTapMyLocationButton -= OnDidTapMyLocationButton;
					mapView.OverlayTapped -= OnOverlayTapped;
					mapView.CoordinateTapped -= OnCoordinateTapped;
					mapView.CoordinateLongPressed -= OnCoordinateLongPressed;
					mapView.PoiWithPlaceIdTapped -= OnPoiWithPlaceIdTapped;
					mapView.MyLocationEnabled = false;
					mapView.StopRendering();
					mapView.Dispose();
					mapView = null;
				}

				if (actionSheet != null) {
					actionSheet.Dispose();
					actionSheet = null;
				}

				ReleaseDesignerOutlets();
			}

			base.Dispose(disposing);
		}
	}
}