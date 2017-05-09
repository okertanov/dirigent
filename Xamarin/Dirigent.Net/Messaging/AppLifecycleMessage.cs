using System;
using TinyMessenger;

namespace Dirigent.Net.Messaging {
	public class AppLifecycleMessage : GenericTinyMessage<AppLifecycleState> {
		public AppLifecycleMessage(object sender, AppLifecycleState state) : base(sender, state) {
		}
	}

	public enum AppLifecycleState {
		Launched,
		Terminate,
		ResignActivation,
		Activated,
		EnterBackground,
		EnterForeground
	}
}
