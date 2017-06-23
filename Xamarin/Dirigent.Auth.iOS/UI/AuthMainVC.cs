using CoreGraphics;
using UIKit;

namespace Dirigent.Auth.iOS.UI {
	internal partial class AuthMainVC : UIViewController {
		public AuthMainVC() : base("AuthMainVC", null) {
		}

		public override void ViewDidLoad() {
			base.ViewDidLoad();

			// FB
			var fbButton = new UIButton(UIButtonType.RoundedRect) {
				Bounds = new CGRect(0, 0, 200, 40),
				Enabled = true,
				Hidden = false
			};
			fbButton.WidthAnchor.ConstraintEqualTo(fbButton.Bounds.Width).Active = true;
			fbButton.HeightAnchor.ConstraintEqualTo(fbButton.Bounds.Height).Active = true;
			fbButton.SetTitle("Sign in with Facebook", UIControlState.Normal);
			fbButton.SetTitleColor(UIColor.DarkTextColor, UIControlState.Normal);

			// TWITTER
			var twiterButton = new UIButton(UIButtonType.RoundedRect) {
				Bounds = new CGRect(0, 0, 200, 40),
				Enabled = true,
				Hidden = false
			};
			twiterButton.WidthAnchor.ConstraintEqualTo(twiterButton.Bounds.Width).Active = true;
			twiterButton.HeightAnchor.ConstraintEqualTo(twiterButton.Bounds.Height).Active = true;
			twiterButton.SetTitle("Sign in with Twitter", UIControlState.Normal);
			twiterButton.SetTitleColor(UIColor.DarkTextColor, UIControlState.Normal);

			// DEMO
			var continueAsDemoButton = new UIButton(UIButtonType.RoundedRect) {
				Bounds = new CGRect(0, 0, 200, 40),
				Enabled = true,
				Hidden = false
			};
			continueAsDemoButton.WidthAnchor.ConstraintEqualTo(continueAsDemoButton.Bounds.Width).Active = true;
			continueAsDemoButton.HeightAnchor.ConstraintEqualTo(continueAsDemoButton.Bounds.Height).Active = true;
			continueAsDemoButton.SetTitle("Be Demo User", UIControlState.Normal);
			continueAsDemoButton.SetTitleColor(UIColor.DarkTextColor, UIControlState.Normal);

			StackView.Spacing = 8f;
			StackView.Distribution = UIStackViewDistribution.EqualSpacing;
			StackView.Alignment = UIStackViewAlignment.Center;
			StackView.TranslatesAutoresizingMaskIntoConstraints = false;

			StackView.AddArrangedSubview(fbButton);
			StackView.AddArrangedSubview(twiterButton);
			StackView.AddArrangedSubview(continueAsDemoButton);

			StackView.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor).Active = true;
			StackView.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor).Active = true;
		}
	}
}