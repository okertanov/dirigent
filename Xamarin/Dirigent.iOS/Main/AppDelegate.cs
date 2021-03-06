using System;
using Foundation;
using TinyIoC;
using UIKit;

namespace Dirigent.iOS.Main {
	[Register("AppDelegate")]
	internal sealed class AppDelegate : UIApplicationDelegate {
		private static readonly Bootstrapper bootstrapper;

		public override UIWindow Window { get; set; }

		static AppDelegate() {
			bootstrapper = new Bootstrapper();
		}

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions) {
			bootstrapper.Launched();
			return true;
		}

		public override void OnResignActivation(UIApplication application) {
			bootstrapper.ResignActivation();
		}

		public override void OnActivated(UIApplication application) {
			bootstrapper.Activated();
		}

		public override void DidEnterBackground(UIApplication application) {
			bootstrapper.EnterBackground();
		}

		public override void WillEnterForeground(UIApplication application) {
			bootstrapper.EnterForeground();
		}

		public override void WillTerminate(UIApplication application) {
			bootstrapper.Terminate();
		}
	}
}