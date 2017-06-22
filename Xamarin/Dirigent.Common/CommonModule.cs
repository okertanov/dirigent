using System.Threading.Tasks;
using Dirigent.Common.Core.IoC;
using Dirigent.Common.Core.Module;

namespace Dirigent.Common {
	public class CommonModule : IModule {
		public CommonModule(IIoCContainer container, IMessengerHub messenger) {
		}

		public Task Init() {
			return Task.FromResult(true);
		}
	}
}