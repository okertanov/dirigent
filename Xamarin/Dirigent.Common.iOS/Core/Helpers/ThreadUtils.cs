using System;
using System.Threading.Tasks;
using Dirigent.Common.Core.Helpers;
using Foundation;

namespace Dirigent.Common.iOS.Core.Helpers {
	public static class ThreadUtils {
		public static void SafeInvokeOnMainThread(Action action) {
			if (NSThread.IsMain) {
				action.Invoke();
			}
			else {
				NSThread.MainThread.InvokeOnMainThread(action);
			}
		}

		public static void SafeBeginInvokeOnMainThread(Action action) {
			if (NSThread.IsMain) {
				action.Invoke();
			}
			else {
				NSThread.MainThread.BeginInvokeOnMainThread(action);
			}
		}

		public static Task SafeBeginInvokeOnMainThreadAsync(Func<Task> action) {
			if (NSThread.IsMain) {
				return action.Invoke();
			}
			else {
				var tcs = new TaskCompletionSource<bool>();
				NSThread.MainThread.BeginInvokeOnMainThread(() => {
					try {
						action().ContinueWith(t => {
							try {
								t.CheckException();
								tcs.TrySetResult(true);
							}
							catch (ObjectDisposedException) {
								tcs.TrySetResult(true);
							}
							catch (Exception ex) {
								tcs.TrySetException(ex);
							}
						}, TaskContinuationOptions.ExecuteSynchronously);
					}
					catch (Exception e) {
						tcs.TrySetException(e);
					}
				});

				return tcs.Task;
			}
		}

		public static Task<T> SafeBeginInvokeOnMainThreadAsync<T>(Func<Task<T>> action) {
			if (NSThread.IsMain) {
				return action.Invoke();
			}
			else {
				var tcs = new TaskCompletionSource<T>();
				NSThread.MainThread.BeginInvokeOnMainThread(() => {
					try {
						action().ContinueWith(t => {
							try {
								t.CheckException();
								tcs.TrySetResult(t.Result);
							}
							catch (ObjectDisposedException) {
								tcs.TrySetResult(t.Result);
							}
							catch (Exception ex) {
								tcs.TrySetException(ex);
							}
						}, TaskContinuationOptions.ExecuteSynchronously);
					}
					catch (Exception e) {
						tcs.TrySetException(e);
					}
				});

				return tcs.Task;
			}
		}
	}
}