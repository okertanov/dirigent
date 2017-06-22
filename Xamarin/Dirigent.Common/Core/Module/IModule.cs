using System;
using System.Threading.Tasks;

namespace Dirigent.Common.Core.Module {
	public interface IModule {
		Task Init();
	}
}