using System;

namespace Dirigent.Common.Core.IoC {
	public interface IIoCContainer {
		void Register<RegisterType, RegisterImplementation>() where RegisterType : class where RegisterImplementation : class, RegisterType;
		ResolveType Resolve<ResolveType>() where ResolveType : class;

	}
}