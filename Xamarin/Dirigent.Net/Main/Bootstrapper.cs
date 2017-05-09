using System;
using Dirigent.Net.Logging;
using Dirigent.Net.Messaging;
using Google.Maps;
using TinyIoC;
using TinyMessenger;

namespace Dirigent.Net.Main {
	public class Bootstrapper {
		private readonly TinyIoCContainer container;
		private readonly ITinyMessengerHub messenger;

		private static readonly Logger Logger = LogManager.GetLogger<Bootstrapper>();

		public Bootstrapper(TinyIoCContainer container) {
			this.container = container;
			this.container.Register<ITinyMessengerHub, TinyMessengerHub>().AsSingleton();
			this.messenger = this.container.Resolve<ITinyMessengerHub>();
		}

		public void Launched() {
			Logger.Debug("Launched");
			MapServices.ProvideAPIKey("AIzaSyBFI1eo79_NB3nflo4ac48fUlkGos__WN4");
			messenger.PublishAsync(new AppLifecycleMessage(this, AppLifecycleState.Launched));
		}

		public void Terminate() {
			Logger.Debug("Terminate");
			messenger.PublishAsync(new AppLifecycleMessage(this, AppLifecycleState.Terminate));
		}

		public void ResignActivation() {
			Logger.Debug("Resign Activation");
			messenger.PublishAsync(new AppLifecycleMessage(this, AppLifecycleState.ResignActivation));
		}

		public void Activated() {
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
	}
}