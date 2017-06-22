using System.Threading.Tasks;
using Dirigent.Common.Core.IoC;
using Dirigent.Common.Core.Module;

namespace Dirigent.Auth {
	public sealed class AuthModule : IModule {
		public AuthModule(IIoCContainer container, IMessengerHub messenger) {
		}

		public Task Init() {
			return Task.FromResult(true);
		}
	}
}