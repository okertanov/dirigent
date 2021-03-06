using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using CoreLocation;
using Dirigent.Auth.Services;
using Dirigent.Common.iOS.Core.Helpers;
using Dirigent.iOS.Logging;
using Dirigent.iOS.Messaging;
using Dirigent.iOS.Services;
using Dirigent.iOS.UI.Components;
using Foundation;
using Google.Maps;
using TinyIoC;
using TinyMessenger;
using UIKit;

namespace Dirigent.iOS.UI {
	internal partial class FirstViewController : UIViewController {
		private const float DefaultZoom = 16f;

		private readonly IAuthService authService;
        private readonly IAuthenticator authenticator;
		private readonly ILocationService locationService;
		private readonly IPhotoLibraryService photoLibraryService;

		private UIGestureRecognizer doubleTapRecognizer;
		private Lazy<ActionSheet> actionSheet;
		private MapView mapView;
		private Geocoder geocoder;
		private IDisposable appLifecycleSubscription;

		private static readonly Logger Logger = LogManager.GetLogger<FirstViewController>();

		public bool InitialPlaceDetected { get; private set; }

		public bool ChromeShown { get; private set; }

		protected FirstViewController(IntPtr handle) : base(handle) {
            authService = TinyIoCContainer.Current.Resolve<IAuthService>();
			authenticator = TinyIoCContainer.Current.Resolve<IAuthenticator>();
			locationService = TinyIoCContainer.Current.Resolve<ILocationService>();
			photoLibraryService = TinyIoCContainer.Current.Resolve<IPhotoLibraryService>();

			actionSheet = new Lazy<ActionSheet>(CreateActionSheet);
		}

		public override void ViewDidLoad() {
			base.ViewDidLoad();

			var messengerHub = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>();
			appLifecycleSubscription = messengerHub.Subscribe<AppLifecycleMessage>(OnAppLifecycleMessage);

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
			await actionSheet.Value.Present(sender as UIView).ContinueWith(async t => {
				if (!t.IsCanceled && !t.IsFaulted && t.IsCompleted) {
					await t.Result.Handler(t.Result);
				}
				else {
					Logger.Debug("Menu item '{0}'", t.IsCanceled ? "cancelled" : "faulted");
				}
			});
		}

        private async Task OnAuthenticateUser(ActionSheetItem arg) {
			Logger.Debug("Menu item selected: '{0}'", arg.Title);
			await ThreadUtils.SafeBeginInvokeOnMainThreadAsync(OnAuthenticateUserImpl);
        }

		private async Task OnAuthenticateUserImpl() {
			var info = await authenticator.Authenticate();
			Logger.Debug("Authenticate status: {0}", info);
		}

		private async Task OnZoomMyLocation(ActionSheetItem arg) {
			Logger.Debug("Menu item selected: '{0}'", arg.Title);
			var location = mapView.MyLocation;
			await ZoomToLocation(location.Coordinate.Latitude, location.Coordinate.Longitude, DefaultZoom);
		}

		private async Task OnNavigateByPhoto(ActionSheetItem arg) {
			Logger.Debug("Menu item selected: '{0}'", arg.Title);
			await ThreadUtils.SafeBeginInvokeOnMainThreadAsync(OnNavigateByPhotoImpl);
		}

		private async Task OnNavigateByPhotoImpl() {
			var imagePaths = await photoLibraryService.PickAsync(this);

			var selectedAssets = await photoLibraryService.RequestAssetsAsync(imagePaths);
			var selectedAsset = selectedAssets.First();

			var allCollections = await photoLibraryService.RequestCollectionsAsync();
			var collectionsForAsset = await photoLibraryService.RequestCollectionsContainingAsync(selectedAsset);

			var assetLocation = selectedAsset.Location ??
			                    collectionsForAsset
			                    	.Where(c => c.ApproximateLocation != null)
			                        .Select(c => c.ApproximateLocation)
			                        .FirstOrDefault() ?? null;
			Logger.Debug("Asset: {0}, Location: {1}", selectedAsset, assetLocation);

			var imageRequest = await photoLibraryService.RequestImageAsync(selectedAsset, CGSize.Empty);
			var imageDataRequest = await photoLibraryService.RequestImageDataAsync(selectedAsset);
			Logger.Debug("Image: {0}, info: {1} ({2}-{3}-{4}-{5})", imageRequest.Image, imageRequest.Info, imageDataRequest.Data.Length, imageDataRequest.DataUti, imageDataRequest.Orientation, imageDataRequest.Info);

			if (assetLocation != null) {
				var geocodingInfo = await GetReverseGeocodingInfo(assetLocation.Coordinate);
				await DropPin(assetLocation.Coordinate, geocodingInfo.Item1, geocodingInfo.Item1, imageRequest.Image);
				await ZoomToLocation(assetLocation.Coordinate.Latitude, assetLocation.Coordinate.Longitude, mapView.Layer.CameraZoomLevel);
			}
			else {
				var fileName = imageDataRequest.Info["PHImageFileURLKey"]?.ToString() ?? selectedAsset.LocalIdentifier;
				var title = String.Format("No Location for image {0}", fileName);
				var description = "Please select another one.";
				using (var alert = new AlertView(AlertViewType.Close, title, description)) {
					var cts = new CancellationTokenSource();
					var res = await alert.ShowAsync(cts);
				}
			}
		}

		private Task OnBookmarkPlace(ActionSheetItem arg) {
			Logger.Debug("Menu item selected: '{0}'", arg.Title);
			return Task.FromResult(false);
		}

		private Task OnRouteTracking(ActionSheetItem arg) {
			Logger.Debug("Menu item selected: '{0}'", arg.Title);
			return Task.FromResult(false);
		}

		private Task OnChangeMapType(ActionSheetItem arg) {
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
			return Task.FromResult(true);
		}

		private Task OnOpenGoogleMaps(ActionSheetItem arg) {
			Logger.Debug("Menu item selected: '{0}'", arg.Title);
			ThreadUtils.SafeBeginInvokeOnMainThread(() => OnOpenGoogleMapsImpl());
			return Task.FromResult(true);
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

		private void OnCameraPositionChanged(object sender, GMSCameraEventArgs args) {
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

		private void OnAppLifecycleMessage(AppLifecycleMessage message) {
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

			return Task.FromResult(cameraMapView);
		}

		private Task<Marker> DropPin(CLLocationCoordinate2D coordinate, string title, string description, UIImage icon) {
			var marker = new Marker {
				Position = coordinate,
				Title = title,
				Snippet = description,
				Map = mapView,
				AppearAnimation = MarkerAnimation.Pop,
				Icon = icon
			};
			return Task.FromResult(marker);
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

		private ActionSheet CreateActionSheet() {
            var user = authService.GetUser();

            var loginActionSheetTitle = user.IsAuthenticated ? 
                String.Format("Hello {0}", user.DisplayName) : 
                String.Format("{0} user. Sign-in", user.IsDemo ? "Demo" : "Unknown" );

            var sheet = new ActionSheet(null, "Close", new[] {
                    new ActionSheetItem(loginActionSheetTitle, OnAuthenticateUser),
					new ActionSheetItem("Zoom to my Location", OnZoomMyLocation),
					new ActionSheetItem("Navigate by Photo", OnNavigateByPhoto),
					new ActionSheetItem("Bookmark this Place", OnBookmarkPlace),
					new ActionSheetItem("Route Tracking", OnRouteTracking),
					new ActionSheetItem("Change Map Type", OnChangeMapType),
					new ActionSheetItem("Open Google Maps", OnOpenGoogleMaps)
				});
			return sheet;
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

				if (actionSheet != null && actionSheet.Value != null) {
					actionSheet.Value.Dispose();
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
}