using System.Threading.Tasks;
using Dirigent.Common.Core.IoC;
using Dirigent.Common.Core.Module;

namespace Dirigent.Remote {
	public sealed class RemoteModule : IModule {
		public RemoteModule(IIoCContainer container, IMessengerHub messenger) {
		}

		public Task Init() {
			return Task.FromResult(true);
		}
	}
}