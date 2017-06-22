using System.Threading.Tasks;
using Dirigent.Common.Core.IoC;
using Dirigent.Common.Core.Module;

namespace Dirigent.Auth {
	public class AuthModule : IModule {
		public AuthModule() {
		}

		public Task Init(IIoCContainer container, IMessengerHub messenger) {
			return Task.FromResult(true);
		}
	}
}