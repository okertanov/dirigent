using System;

namespace Dirigent.Common.Entity {
    public sealed class UserInfo {
        public bool IsAuthenticated { get; }
        public bool IsDemo { get; }
        public string DisplayName => GetDisplayName();

        public string Email { get; }
        public string Password { get; }
		public string FullName { get; }

        public static UserInfo Empty = new UserInfo();

        private UserInfo() {
            IsAuthenticated = false;
            IsDemo = false;
			Email = String.Empty;
			Password = String.Empty;
			FullName = String.Empty;
        }

        private string GetDisplayName() {
            var displayName = String.Empty;

            if (IsAuthenticated) {
                displayName = String.IsNullOrEmpty(FullName) ? String.IsNullOrEmpty(Email) ? "No Mail" : Email : FullName;
            }
            else if (IsDemo) {
                displayName = "Demo";
            }
            else {
                displayName = "Unknown";
            }

            return displayName;
        }
    }
}