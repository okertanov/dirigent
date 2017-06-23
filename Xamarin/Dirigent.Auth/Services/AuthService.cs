using Dirigent.Auth.Entity;
using Dirigent.Common.Services;

namespace Dirigent.Auth.Services {
	public class AuthService : IAuthService {
		public AuthService() {
		}

        public virtual UserInfo GetUser() {
            return UserInfo.Empty;
        }
    }
}