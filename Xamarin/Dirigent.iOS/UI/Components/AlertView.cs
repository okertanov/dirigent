using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Dirigent.iOS.UI.Components {
	internal class AlertView : IDisposable {
		private readonly UIAlertView alertView;
		private readonly TaskCompletionSource<AlertViewResult> tcs;
			
		public AlertViewType AlertType { get; }
		public string Title { get; }
		public string Description { get; }
		public IEnumerable<string> Buttons { get; }

		public AlertView(AlertViewType alertType, string title, string description) {
			AlertType = alertType;
			Title = title;
			Description = description;
			Buttons = CreateButtons(AlertType);
			tcs = new TaskCompletionSource<AlertViewResult>();
			var alertViewDelegate = new AlertViewDelegate(this);
			alertView = new UIAlertView(Title, Description, alertViewDelegate, null, Buttons.ToArray());
		}

		public AlertViewResult ShowModal(CancellationTokenSource cts) {
			cts.Token.Register(t => ((TaskCompletionSource<AlertViewResult>)t).TrySetCanceled(), tcs);

			alertView.Show();

			while (!(cts.IsCancellationRequested || tcs.Task.IsFaulted || tcs.Task.IsCanceled || tcs.Task.IsCompleted)) {
				NSRunLoop.Main.RunUntil(NSRunLoopMode.UITracking, NSDate.DistantFuture);
			}

			var result = (cts.IsCancellationRequested || tcs.Task.IsCanceled || tcs.Task.IsFaulted) ?
				AlertViewResult.Interrupted :
			    tcs.Task.Result;

			return result;
		}

		public Task<AlertViewResult> ShowAsync(CancellationTokenSource cts) {
			cts.Token.Register(t => ((TaskCompletionSource<AlertViewResult>)t).TrySetCanceled(), tcs);

			alertView.Show();

			return tcs.Task;
		}

		private IEnumerable<string> CreateButtons(AlertViewType alertType) {
			var buttons = Enumerable.Empty<string>();

			switch (alertType) {
				case AlertViewType.Close:
				buttons = new[] { "Close" };
				break;
				case AlertViewType.YesNo:
				buttons = new[] { "Yes", "No" };
				break;
				case AlertViewType.OKCancel:
				buttons = new[] { "OK", "Cancel" };
				break;
				default:
				throw new ArgumentOutOfRangeException(nameof(alertType));
			}

			return buttons;
		}

		internal void OnClicked(int idx) {
			var result = (tcs.Task.IsCanceled || tcs.Task.IsFaulted) ? AlertViewResult.Interrupted :
				idx > 0 ? AlertViewResult.No : AlertViewResult.Yes;
			tcs.TrySetResult(result);
		}


		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (tcs != null && tcs.Task != null) {
					if (!tcs.Task.IsCompleted) {
						tcs.TrySetCanceled();
					}
					tcs.Task.Dispose();	
				}

				if (alertView != null) {
					alertView.Dispose();
				}
			}
		}
	}

	internal sealed class AlertViewDelegate : UIAlertViewDelegate {
		private readonly AlertView parentRef;

		public AlertViewDelegate(AlertView parent) {
			parentRef = parent;
		}

		public override void Clicked(UIAlertView alertview, nint buttonIndex) {
			parentRef.OnClicked((int)buttonIndex);
		}

		public override void Dismissed(UIAlertView alertView, nint buttonIndex) {
		}

		public override void Canceled(UIAlertView alertView) {
			throw new NotImplementedException();
		}
	}

	public enum AlertViewType {
		Close,
		YesNo,
		OKCancel
	}

	public enum AlertViewResult {
		Yes,
		No,
		Interrupted
	}
}