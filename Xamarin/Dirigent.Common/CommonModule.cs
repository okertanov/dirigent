using System;
using System.Threading.Tasks;
using Dirigent.Common.Core.Module;

namespace Dirigent.Common {
	public class CommonModule : IModule {
		public CommonModule() {
		}

		public Task Init() {
			return Task.FromResult(true);
		}
	}
}