using System.Threading.Tasks;
using Dirigent.Common.Core.IoC;
using Dirigent.Common.Core.Module;
using UIKit;

namespace Dirigent.Remote.iOS {
	public class RemoteIosModule : IModule {
		public RemoteIosModule(IIoCContainer container, IMessengerHub messenger) {
			var label = new UILabel();
		}

		public Task Init() {
			return Task.FromResult(true);
		}
	}
}