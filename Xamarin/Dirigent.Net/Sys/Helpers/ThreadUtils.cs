using System;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Dirigent.Net.Sys.Helpers {
	public static class ThreadUtils {
		public static void SafeInvokeOnMainThread(Action action) {
			if (NSThread.IsMain) {
				action.Invoke();
			}
			else {
				UIApplication.SharedApplication.InvokeOnMainThread(action);
			}
		}

		public static void SafeBeginInvokeOnMainThread(Action action) {
			if (NSThread.IsMain) {
				action.Invoke();
			}
			else {
				UIApplication.SharedApplication.BeginInvokeOnMainThread(action);
			}
		}

		public static void SafeBeginInvokeOnMainThread(Func<Task> awaitable) {
			if (NSThread.IsMain) {
				awaitable.Invoke();
			}
			else {
				UIApplication.SharedApplication.BeginInvokeOnMainThread(() => awaitable.Invoke());
			}
		}
	}
}
