using System;
using System.Threading.Tasks;
using Dirigent.Common.Core.Module;

namespace Dirigent.Auth {
	public class AuthModule : IModule {
		public AuthModule() {
		}

		public Task Init() {
			return Task.FromResult(true);
		}
	}
}