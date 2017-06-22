using System.Threading.Tasks;
using Dirigent.Common.Core.IoC;

namespace Dirigent.Common.Core.Module {
	public interface IModule {
		Task Init(IIoCContainer container, IMessengerHub messenger);
	}
}