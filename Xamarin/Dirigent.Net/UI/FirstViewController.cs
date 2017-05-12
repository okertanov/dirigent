using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using CoreLocation;
using Dirigent.Net.Logging;
using Dirigent.Net.Messaging;
using Dirigent.Net.Services;
using Dirigent.Net.Sys.Helpers;
using Dirigent.Net.UI.Components;
using Foundation;
using Google.Maps;
using Photos;
using TinyIoC;
using TinyMessenger;
using UIKit;

namespace Dirigent.Net.UI {
	public partial class FirstViewController : UIViewController {
		private const float DefaultZoom = 16f;

		private UIGestureRecognizer doubleTapRecognizer;
		private ActionSheet actionSheet;
		private MapView mapView;
		private Geocoder geocoder;
		private ILocationService locationService;
		private IDisposable appLifecycleSubscription;

		private static readonly Logger Logger = LogManager.GetLogger<FirstViewController>();

		public bool InitialPlaceDetected { get; private set; }

		public bool ChromeShown { get; private set; }

		protected FirstViewController(IntPtr handle) : base(handle) {
		}

		public override void ViewDidLoad() {
			base.ViewDidLoad();

			var messengerHub = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>();
			appLifecycleSubscription = messengerHub.Subscribe<AppLifecycleMessage>(OnAppLifecycleMessage);

			locationService = TinyIoCContainer.Current.Resolve<ILocationService>();
			locationService.LocationChanged += OnLocationServiceLocationChanged;
			locationService.StartMonitorLocation();

			if (mapView == null) {
				geocoder = Geocoder.SharedGeocoder;
				mapView = CreateMapView(latitude: 56.948889, longitude: 24.106389, zoom: DefaultZoom).Result;

				doubleTapRecognizer = new UITapGestureRecognizer(OnDoubleTapGesture) {
					NumberOfTouchesRequired = 1,
					NumberOfTapsRequired = 2,
					Enabled = true,
					ShouldRecognizeSimultaneously = (r, o) => {
						return true;
					},
					ShouldBeRequiredToFailBy = (r, o) => {
						var require = o is UITapGestureRecognizer && ((UITapGestureRecognizer)o).NumberOfTapsRequired > 1;
						return require;
					}
				};
				mapView.AddGestureRecognizer(doubleTapRecognizer);

				View.UserInteractionEnabled = true;
				View.AutosizesSubviews = true;
				View.AutoresizingMask = UIViewAutoresizing.All;
				View.TranslatesAutoresizingMaskIntoConstraints = true;
				View.AddSubview(mapView);
				View.SendSubviewToBack(mapView);

				actionSheet = new ActionSheet(null, "Close", new [] {
					new ActionSheetItem("Zoom to my Location", OnZoomMyLocation),
					new ActionSheetItem("Navigate by Photo", OnNavigateByPhoto),
					new ActionSheetItem("Bookmark this Place", OnBookmarkPlace),
					new ActionSheetItem("Route Tracking", OnRouteTracking),
					new ActionSheetItem("Change Map Type", OnChangeMapType),
					new ActionSheetItem("Open Google Maps", OnOpenGoogleMaps)
				});

				AddImageButton.TouchUpInside += OnAddImageButtonTouchUpInside;

				InitChrome();
			}
		}

		public override void ViewWillAppear(bool animated) {
			base.ViewWillAppear(animated);

			StartMapActivities();
		}

		public override void ViewWillDisappear(bool animated) {
			StopMapActivities();

			base.ViewWillDisappear(animated);
		}

		public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator) {
			base.ViewWillTransitionToSize(toSize, coordinator);
			if (mapView != null && mapView.Handle != IntPtr.Zero) {
				mapView.Frame = new CGRect(View.Bounds.Location, toSize);
				View.LayoutIfNeeded();
			}
		}

		#region Menu Handlers

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
			ThreadUtils.SafeBeginInvokeOnMainThread(OnNavigateByPhotoImpl);
		}

		private async void OnNavigateByPhotoImpl() {
			var picker = new GalleryImagePicker();
			var imagesPath = await picker.Pick(this);
			var imagePath = imagesPath.FirstOrDefault();
			Logger.Debug("Image selected: {0}", imagePath);
			var imagesUrls = imagesPath.Select(p => new NSUrl(p)).ToArray();
			var assetResult = PHAsset.FetchAssets(imagesUrls, new PHFetchOptions());
			var asset = assetResult.First() as PHAsset;
			PHImageManager.DefaultManager.RequestImageForAsset(asset, CGSize.Empty, PHImageContentMode.Default, new PHImageRequestOptions(), (result, info) => {
				Logger.Debug("PH Image Manager: Image: {0}, info: {1}", result, info);
			});
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
			if ((int)newMapType >= Enum.GetValues(typeof(MapViewType)).Length) {
				newMapType = MapViewType.Normal;
			}
			ThreadUtils.SafeBeginInvokeOnMainThread(async () => {
				mapView.MapType = newMapType;
				var statusbarStyle = mapView.MapType == MapViewType.Satellite ||
							mapView.MapType == MapViewType.Hybrid ?
							StatusbarStyle.White : StatusbarStyle.Black;
				UIUtils.SetStatusBarStyle(statusbarStyle);
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

		#endregion // Menu Handlers

		#region Event Handlers

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
			OnAddImageButtonTouchUpInside(view, EventArgs.Empty);
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

		private async void OnLocationServiceLocationChanged(object sender, Services.LocationChangedEventArgs args) {
			if (mapView != null && mapView.Handle != IntPtr.Zero && mapView.MyLocation != null && !InitialPlaceDetected) {
				var location = mapView.MyLocation;
				await ZoomToLocation(location.Coordinate.Latitude, location.Coordinate.Longitude, mapView.Layer.CameraZoomLevel);
				InitialPlaceDetected = true;
			}
		}

		private async void OnAppLifecycleMessage(AppLifecycleMessage message) {
			Logger.Debug("App Lifecycle Message: {0}", message.Content);
			switch (message.Content) {
				case AppLifecycleState.EnterForeground:
				StartMapActivities();
				break;
				case AppLifecycleState.EnterBackground:
				InitialPlaceDetected = false;
				StopMapActivities();
				break;
			}
		}

		#endregion // Event Handlers

		private void StartMapActivities() {
			locationService.StartMonitorLocation();
			mapView.StartRendering();
		}

		private void StopMapActivities() {
			locationService.StopMonitorLocation();
			mapView.StopRendering();
		}

		private Task<MapView> CreateMapView(double latitude, double longitude, float zoom) {
			var cameraPosition = CameraPosition.FromCamera(latitude: latitude, longitude: longitude, zoom: zoom);
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

			// cameraMapView.Delegate = new SearchMapViewDelegate();

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

		private void OnDoubleTapGesture(UIGestureRecognizer r) {
			if (r.State == UIGestureRecognizerState.Ended) {
				ChromeShown = !ChromeShown;
				UpdateChrome();
			}
		}

		private void UpdateChrome() {
			if (ChromeShown) {
				UIUtils.ShowStatusBar(true);
				UIUtils.ShowTabBar(this.ParentViewController, true);
				mapView.Settings.MyLocationButton = true;
				AddImageButton.Hidden = true;
			}
			else {
				UIUtils.ShowStatusBar(false);
				UIUtils.ShowTabBar(this.ParentViewController, false);
				mapView.Settings.MyLocationButton = true;
				AddImageButton.Hidden = true;
			}
		}

		private void InitChrome() {
			UIUtils.SetStatusBarStyle(StatusbarStyle.Black);
			ChromeShown = true;
			UpdateChrome();

			var subviews = UIUtils.EnumerateSubviews(mapView);
			var myLocationButton = subviews.SingleOrDefault(v => v.AccessibilityIdentifier == "my_location") as UIButton;
			if (myLocationButton != null) {
				mapView.Padding = new UIEdgeInsets(0, 0, 46, 0);
				mapView.LayoutIfNeeded();
			}
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (appLifecycleSubscription != null) {
					appLifecycleSubscription.Dispose();
					appLifecycleSubscription = null;
				}

				if (locationService != null) {
					locationService.LocationChanged -= OnLocationServiceLocationChanged;
					locationService.StopMonitorLocation();
					locationService = null;
				}

				if (doubleTapRecognizer != null && doubleTapRecognizer.Handle != IntPtr.Zero) {
					if (mapView != null && mapView.Handle != IntPtr.Zero) {
						mapView.RemoveGestureRecognizer(doubleTapRecognizer);
					}
					doubleTapRecognizer.Dispose();
					doubleTapRecognizer = null;
				}

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
					mapView.RemoveFromSuperview();
					mapView.Dispose();
					mapView = null;
				}

				if (geocoder != null) {
					geocoder.Dispose();
					geocoder = null;
				}

				if (actionSheet != null) {
					actionSheet.Dispose();
					actionSheet = null;
				}

				if (AddImageButton != null && AddImageButton.Handle != IntPtr.Zero) {
					AddImageButton.TouchUpInside -= OnAddImageButtonTouchUpInside;
				}

				ReleaseDesignerOutlets();
			}

			base.Dispose(disposing);
		}
	}

	internal class SearchMapViewDelegate : MapViewDelegate {
	}
}