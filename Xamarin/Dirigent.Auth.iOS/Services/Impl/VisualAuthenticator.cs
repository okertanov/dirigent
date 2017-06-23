using System.Threading.Tasks;
using Dirigent.Auth.Entity;
using Dirigent.Auth.iOS.UI;
using Dirigent.Auth.Services;
using UIKit;

namespace Dirigent.Auth.iOS.Services.Impl {
	internal sealed class VisualAuthenticator : IAuthenticator {
		private readonly IAuthService authService;

		public VisualAuthenticator(IAuthService authService) {
			this.authService = authService;
		}

		public async Task<AuthInfo> Authenticate() {
			var rootVc = UIApplication.SharedApplication.KeyWindow.RootViewController;
			var navigationController = rootVc is UINavigationController ? rootVc as UINavigationController : rootVc.NavigationController;
			var authVc = new AuthMainVC();
			await navigationController.PresentViewControllerAsync(authVc, true);
			return AuthInfo.Empty;
		}
	}
}