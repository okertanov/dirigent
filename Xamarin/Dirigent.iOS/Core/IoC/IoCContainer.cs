using Dirigent.Common.Core.IoC;
using TinyIoC;

namespace Dirigent.iOS.Core.IoC {
	internal class IoCContainer : IIoCContainer {
		private readonly TinyIoCContainer container;

		public IoCContainer(TinyIoCContainer container) {
			this.container = container;
		}

		public void Register<RegisterType, RegisterImplementation>()
			where RegisterType : class where RegisterImplementation : class, RegisterType {
			container.Register(typeof(RegisterType), typeof(RegisterImplementation)).AsSingleton();
		}

		public ResolveType Resolve<ResolveType>() where ResolveType : class {
			return (ResolveType)container.Resolve(typeof(ResolveType));
		}
	}
}