using System.Threading.Tasks;
using Dirigent.Common.Core.IoC;
using Dirigent.Common.Core.Module;
using Dirigent.Remote.iOS.Services.Impl;
using Dirigent.Remote.Services;

namespace Dirigent.Remote.iOS {
	public class RemoteIosModule : IModule {
        private readonly IIoCContainer container;
        private readonly IMessengerHub messenger;

		public RemoteIosModule(IIoCContainer container, IMessengerHub messenger) {
			this.container = container;
			this.messenger = messenger;
		}

		public Task Init() {
			container.Register<IDatabaseService, FirebaseDatabaseService>();
			container.Register<IStorageService, FirebaseStorageService>();
			return Task.FromResult(true);
		}
	}
}