using System.Threading.Tasks;
using Dirigent.Auth.iOS.Services.Impl;
using Dirigent.Common.Core.IoC;
using Dirigent.Common.Core.Module;
using Dirigent.Common.Services;

namespace Dirigent.Auth.iOS {
	public class AuthIosModule : IModule {
		private readonly IIoCContainer container;
		private readonly IMessengerHub messenger;

		public AuthIosModule(IIoCContainer container, IMessengerHub messenger) {
			this.container = container;
			this.messenger = messenger;
		}

		public Task Init() {
            container.Register<IAuthService, FirebaseAuthService>();

			Firebase.Analytics.App.Configure();

			return Task.FromResult(true);
		}
	}
}