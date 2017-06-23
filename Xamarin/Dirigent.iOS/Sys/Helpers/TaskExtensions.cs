using System;
using System.Threading.Tasks;

namespace Dirigent.iOS.Sys.Helpers {
	public static class TaskExtensions {
		public static Task CompletedTask { get; } = Task.FromResult(true);

		public static void CheckException(this Task task) {
			if (task.IsCanceled) {
				throw new TaskCanceledException(task);
			}

			if (task.IsFaulted) {
				throw task.Exception.Unwrap();
			}
		}
	}
}