using System;
using System.Threading.Tasks;
using Dirigent.Auth.Entity;

namespace Dirigent.Auth.Services {
	public interface IAuthenticator {
		Task<AuthInfo> Authenticate();
	}
}