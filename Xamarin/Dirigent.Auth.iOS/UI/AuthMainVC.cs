using CoreGraphics;
using UIKit;

namespace Dirigent.Auth.iOS.UI {
	internal partial class AuthMainVC : UIViewController {
		public AuthMainVC() : base("AuthMainVC", null) {
		}

		public override void ViewDidLoad() {
			base.ViewDidLoad();

			var continueAsDemoButton1 = new UIButton(UIButtonType.RoundedRect) {
				Bounds = new CGRect(0, 0, 100, 40),
				Enabled = true,
				Hidden = false,
				BackgroundColor = UIColor.Red
			};
			continueAsDemoButton1.WidthAnchor.ConstraintEqualTo(continueAsDemoButton1.Bounds.Width).Active = true;
			continueAsDemoButton1.HeightAnchor.ConstraintEqualTo(continueAsDemoButton1.Bounds.Height).Active = true;
			continueAsDemoButton1.SetTitle("Other User", UIControlState.Normal);
			continueAsDemoButton1.SetTitleColor(UIColor.DarkTextColor, UIControlState.Normal);

			StackView.Spacing = 8f;
			StackView.Distribution = UIStackViewDistribution.EqualSpacing;
			StackView.Alignment = UIStackViewAlignment.Center;
			StackView.TranslatesAutoresizingMaskIntoConstraints = false;

			StackView.AddArrangedSubview(continueAsDemoButton1);

			StackView.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor).Active = true;
			StackView.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor).Active = true;
		}
	}
}