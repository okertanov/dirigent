using System.Threading.Tasks;
using Dirigent.Auth.iOS.Services.Impl;
using Dirigent.Auth.Services;
using Dirigent.Common.Core.IoC;
using Dirigent.Common.Core.Module;

namespace Dirigent.Auth.iOS {
	public sealed class AuthIosModule : IModule {
		private readonly IIoCContainer container;
		private readonly IMessengerHub messenger;

		public AuthIosModule(IIoCContainer container, IMessengerHub messenger) {
			this.container = container;
			this.messenger = messenger;
		}

		public Task Init() {
			container.Register<IAuthService, FirebaseAuthService>();
			container.Register<IAuthenticator, VisualAuthenticator>();

			// Firebase API
			Firebase.Analytics.App.Configure();

			return Task.FromResult(true);
		}
	}
}