using CoreGraphics;
using MonoTouch.Fabric.TwitterKit;
using UIKit;

namespace Dirigent.Auth.iOS.UI {
	internal partial class AuthMainVC : UIViewController {
		public AuthMainVC() : base("AuthMainVC", null) {
		}

		public override void ViewDidLoad() {
			base.ViewDidLoad();

			/*var twiterButton = new TWTRLogInButton() {
				Bounds = new CGRect(0, 0, 100, 40),
				Enabled = true,
				Hidden = false,
				BackgroundColor = UIColor.Red
			};
			twiterButton.WidthAnchor.ConstraintEqualTo(twiterButton.Bounds.Width).Active = true;
			twiterButton.HeightAnchor.ConstraintEqualTo(twiterButton.Bounds.Height).Active = true;*/

			var continueAsDemoButton = new UIButton(UIButtonType.RoundedRect) {
				Bounds = new CGRect(0, 0, 100, 40),
				Enabled = true,
				Hidden = false,
				BackgroundColor = UIColor.Green
			};
			continueAsDemoButton.WidthAnchor.ConstraintEqualTo(continueAsDemoButton.Bounds.Width).Active = true;
			continueAsDemoButton.HeightAnchor.ConstraintEqualTo(continueAsDemoButton.Bounds.Height).Active = true;
			continueAsDemoButton.SetTitle("Demo User", UIControlState.Normal);
			continueAsDemoButton.SetTitleColor(UIColor.DarkTextColor, UIControlState.Normal);

			StackView.Spacing = 8f;
			StackView.Distribution = UIStackViewDistribution.EqualSpacing;
			StackView.Alignment = UIStackViewAlignment.Center;
			StackView.TranslatesAutoresizingMaskIntoConstraints = false;

			//StackView.AddArrangedSubview(twiterButton);
			StackView.AddArrangedSubview(continueAsDemoButton);

			StackView.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor).Active = true;
			StackView.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor).Active = true;
		}
	}
}