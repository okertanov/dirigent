using System.Linq;
using System.Threading.Tasks;
using Dirigent.Common.Core.IoC;
using Dirigent.Common.Core.Module;
using Dirigent.iOS.Logging;
using Dirigent.iOS.Messaging;
using Dirigent.iOS.Services;
using Dirigent.iOS.Services.Impl;
using Dirigent.iOS.Sys.Core;
using Google.Maps;
using TinyIoC;
using TinyMessenger;

namespace Dirigent.iOS.Main {
	internal sealed class Bootstrapper {
		private readonly IIoCContainer container;
		private readonly IMessengerHub messenger;

		private static readonly Logger Logger = LogManager.GetLogger<Bootstrapper>();

		internal Bootstrapper() {
			container = new IoCContainer(TinyIoCContainer.Current);

			container.Register<ITinyMessengerHub, TinyMessengerHub>();
			messenger = new MessengerHub(container.Resolve<ITinyMessengerHub>());

			container.Register<ILocationService, LocationService>();
			container.Register<IPhotoLibraryService, PhotoLibraryService>();
		}

		internal async void Launched() {
			Logger.Debug("Launched");

			await InitModules();
			await InitExternal();

			var hub = container.Resolve<ITinyMessengerHub>();
			messenger.PublishAsync(new AppLifecycleMessage(this, AppLifecycleState.Launched));
		}

		internal void Terminate() {
			Logger.Debug("Terminate");
			messenger.PublishAsync(new AppLifecycleMessage(this, AppLifecycleState.Terminate));
		}

		internal void ResignActivation() {
			Logger.Debug("Resign Activation");
			messenger.PublishAsync(new AppLifecycleMessage(this, AppLifecycleState.ResignActivation));
		}

		internal void Activated() {
			Logger.Debug("Activated");
			messenger.PublishAsync(new AppLifecycleMessage(this, AppLifecycleState.Activated));
		}

		internal void EnterBackground() {
			Logger.Debug("Enter Background");
			messenger.PublishAsync(new AppLifecycleMessage(this, AppLifecycleState.EnterBackground));
		}

		internal void EnterForeground() {
			Logger.Debug("Enter Foreground");
			messenger.PublishAsync(new AppLifecycleMessage(this, AppLifecycleState.EnterForeground));
		}

		private Task InitModules() {
			var modules = new IModule[] {
				new Common.CommonModule(container, messenger),
				new Auth.AuthModule(container, messenger),
				new Auth.iOS.AuthIosModule(container, messenger),
				new Remote.RemoteModule(container, messenger),
				new Remote.iOS.RemoteIosModule(container, messenger)
			};
			var tasks = modules.Select(m => m.Init());

			return Task.WhenAll(tasks);
		}
			
		private Task InitExternal() {
			// Google Maps API
			MapServices.ProvideAPIKey("AIzaSyBFI1eo79_NB3nflo4ac48fUlkGos__WN4");

			return Task.CompletedTask;
		}
	}
}