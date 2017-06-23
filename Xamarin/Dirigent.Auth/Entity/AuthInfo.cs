using System;

namespace Dirigent.Auth.Entity {
	public sealed class AuthInfo {
		public string AccessToken { get; }
		public string RefreshToken { get; }
		public DateTimeOffset Expires { get; }
		public UserInfo User { get; }

		public static AuthInfo Empty = new AuthInfo();

		private AuthInfo() {
			AccessToken = String.Empty;
			RefreshToken = String.Empty;
			Expires = DateTimeOffset.MinValue;
			User = UserInfo.Empty;
		}
	}
}