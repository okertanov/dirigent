using System.Threading.Tasks;
using Dirigent.Common.Core.IoC;
using Dirigent.Common.Core.Module;

namespace Dirigent.Common {
	public class CommonModule : IModule {
		public CommonModule() {
		}

		public Task Init(IIoCContainer container, IMessengerHub messenger) {
			return Task.FromResult(true);
		}
	}
}