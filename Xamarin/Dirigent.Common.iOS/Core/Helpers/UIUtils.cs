using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace Dirigent.Common.iOS.Core.Helpers {
	public static class UIUtils {
		public static void SetStatusBarStyle(StatusbarStyle style) {
			switch (style) {
				case StatusbarStyle.Black:
				UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.Default;
				break;
				case StatusbarStyle.White:
				UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
				break;
				default:
				throw new ArgumentOutOfRangeException(nameof(style));
			}
		}

		public static void ShowStatusBar(bool show) {
			UIApplication.SharedApplication.SetStatusBarHidden(!show, UIStatusBarAnimation.Fade);
		}

		public static void ShowTabBar(UIViewController controller, bool show) {
			UIView.BeginAnimations("ShowTabBar");
			UIView.SetAnimationDuration(0.5f);
			foreach (var view in controller.View.Subviews) {
				if (view is UITabBar) {
					view.Alpha = show ? 0.7f : 0f;
				}
			}
			UIView.CommitAnimations();
		}

		public static IEnumerable<UIView> EnumerateSubviews(UIView parent) {
			if (!parent.Subviews.Any()) {
				yield break;
			}

			foreach (var view in parent.Subviews) {
				foreach (var subview in EnumerateSubviews(view)) {
					yield return subview;
				}
				yield return view;
			}
		}
	}

	public enum StatusbarStyle {
		Black,
		White
	}
}