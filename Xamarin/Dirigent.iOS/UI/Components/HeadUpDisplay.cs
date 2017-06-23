using System;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Dirigent.iOS.Sys.Helpers;
using UIKit;

namespace Dirigent.iOS.UI.Components {
	internal class HeadUpDisplay : IDisposable {
		private UIView hudView;
		private UIGestureRecognizer gestureRecognizer;

		public HeadUpDisplay() {
		}

		public Task Present(UIView parentView, string what, CancellationToken ct) {
			ct.Register(() => Unpresent(true), true);
			var task = PresentImpl(parentView, what, true);
			return task;
		}

		public void Dispose() {
			Unpresent(false);
		}

		private Task PresentImpl(UIView parentView, string what, bool animate) {
			var tcs = new TaskCompletionSource<bool>();
			ThreadUtils.SafeBeginInvokeOnMainThread(async () => {
				var center = new CGPoint(Math.Round(parentView.Bounds.Width / 2), Math.Round(parentView.Bounds.Height / 2));
				var hudViewRect = new CGRect(parentView.Bounds.Left + 10, center.Y - 30, parentView.Bounds.Right - 20, 60);
				hudView = new UIView(hudViewRect) {
					BackgroundColor = UIColor.Black,
					Alpha = 0f,
					UserInteractionEnabled = true,
					AutoresizingMask = UIViewAutoresizing.All,
					AutosizesSubviews = true
				};
				hudView.Layer.CornerRadius = 24f;
				var label = new UILabel(hudView.Bounds) {
					TextAlignment = UITextAlignment.Center,
					TextColor = UIColor.White,
					BackgroundColor = UIColor.Clear,
					Lines = 0,
					LineBreakMode = UILineBreakMode.WordWrap,
					Font = UIFont.FromName("HelveticaNeue-Medium", 24f),
					Text = what
				};
				hudView.AddSubview(label);
				parentView.AddSubview(hudView);
				parentView.BringSubviewToFront(hudView);
				await ShowHudView(show: true, animate: animate).ContinueWith(t => tcs.TrySetResult(true));
			});
			return tcs.Task;
		}

		private void Unpresent(bool animate) {
			ThreadUtils.SafeInvokeOnMainThread(async () => await UnpresentImpl(animate));
		}

		private Task UnpresentImpl(bool animate) {
			Action removeAction = () => {
				hudView.Hidden = true;
				hudView.RemoveGestureRecognizer(gestureRecognizer);
				hudView.RemoveFromSuperview();
				hudView.Dispose();
				hudView = null;
			};

			if (!animate) {
				removeAction();
				return Task.CompletedTask;
			}

			return ShowHudView(show: false, animate: animate).ContinueWith(t => removeAction);
		}

		private Task ShowHudView(bool show, bool animate) {
			var tcs = new TaskCompletionSource<bool>();

			if (hudView != null && hudView.Handle != IntPtr.Zero) {
				UIView.Animate(0.9f, 0f, UIViewAnimationOptions.AllowAnimatedContent,
					() => hudView.Alpha = show ? 0.5f : 0f,
					() => tcs.TrySetResult(true));
			}
			else {
				tcs.TrySetCanceled();
			}

			return tcs.Task;
		}
}
}